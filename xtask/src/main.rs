use anyhow::{bail, Result};
use cargo_metadata::{MetadataCommand, Package};
use clap::{Args, Parser, Subcommand};
use std::fs;
use std::path::{Path, PathBuf};
use std::process::Command;
use std::time::SystemTime;

#[derive(Parser)]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    Build(BuildArgs),
}

#[derive(Args)]
struct BuildArgs {
    #[arg(long)]
    release: bool,

    #[arg(long)]
    dir: Option<PathBuf>,

    /// フォーマット: PATH,FEATURE,SUBDIR  (例: crates/plugin-twitch,alpha,plugins/plugin-twitch)
    /// FEATURE が空の場合はデフォルト features でビルドされる (例: crates/plugin-dummy,,dummy)
    #[arg(long = "crate", value_name = "PATH,FEATURE,SUBDIR")]
    crates: Vec<String>,
}

struct CrateDeploy {
    path: String,
    feature: String,
    subdir: String,
}

fn main() -> Result<()> {
    let cli = Cli::parse();
    match cli.command {
        Commands::Build(args) => build(args),
    }
}

fn build(args: BuildArgs) -> Result<()> {
    let profile = if args.release { "release" } else { "debug" };
    let deploys = parse_crates(&args.crates)?;

    // ステップ1: xtask を除くワークスペース全体をビルド
    build_workspace(profile)?;

    // ステップ2: feature 指定クレートを個別に再ビルド
    // ワークスペースビルドは --features を指定できないため（全パッケージに適用されてしまう）、
    // --crate で feature が指定されたクレートのみ個別ビルドで上書きする。
    // これにより、例えば plugin-twitch の "alpha" フィーチャーだけを有効にしたバイナリを
    // デプロイ対象として生成できる。
    for c in &deploys {
        if !c.feature.is_empty() {
            println!("== Rebuild {} with feature: {} ==", c.path, c.feature);
            let mut cmd = Command::new("cargo");
            cmd.arg("build")
                .arg("--manifest-path")
                .arg(format!("{}/Cargo.toml", c.path))
                .arg("--features")
                .arg(&c.feature);
            if profile == "release" {
                cmd.arg("--release");
            }
            run(cmd)?;
        }
    }

    // ステップ3: Tauri アプリのインクリメンタルビルド
    build_tauri(profile)?;

    // ステップ4: デプロイ（--dir 指定時のみ）
    if let Some(dir) = args.dir {
        deploy(profile, &dir, &deploys)?;
    }

    Ok(())
}

fn parse_crates(specs: &[String]) -> Result<Vec<CrateDeploy>> {
    let mut out = Vec::new();
    for s in specs {
        let parts: Vec<_> = s.split(',').collect();
        if parts.len() != 3 {
            bail!("Invalid --crate format: {}", s);
        }
        out.push(CrateDeploy {
            path: parts[0].to_string(),
            feature: parts[1].to_string(),
            subdir: parts[2].to_string(),
        });
    }
    Ok(out)
}

fn build_workspace(profile: &str) -> Result<()> {
    println!("== Workspace build ({}) ==", profile);

    // Tauri アプリは cargo tauri build で別途ビルドするため除外する。
    // cargo build --workspace でそのまま含めると tauri-winres のビルドスクリプトが
    // RC.EXE を要求し、Visual Studio 環境がない場合にエラーになるため。
    let metadata = MetadataCommand::new().exec()?;
    let tauri_packages: Vec<String> = metadata
        .packages
        .iter()
        .filter(|pkg| tauri_project_dir(pkg).is_some())
        .map(|pkg| pkg.name.clone())
        .collect();

    let mut cmd = Command::new("cargo");
    cmd.arg("build")
        .arg("--workspace")
        .arg("--exclude")
        .arg("xtask");

    for tauri_pkg in &tauri_packages {
        cmd.arg("--exclude").arg(tauri_pkg);
    }

    if profile == "release" {
        cmd.arg("--release");
    }

    run(cmd)
}

fn build_tauri(profile: &str) -> Result<()> {
    let metadata = MetadataCommand::new().exec()?;

    for pkg in metadata.packages {
        if let Some(manifest_dir) = tauri_project_dir(&pkg) {
            println!("== Tauri build: {} ==", pkg.name);

            // ステップ3a: フロントエンドの変更チェックと npm run build
            build_frontend_if_needed(&manifest_dir)?;

            // ソースの更新日時と出力 exe を比較し、変更がなければスキップ
            if !should_build_tauri(&pkg, &manifest_dir, profile) {
                continue;
            }

            let mut cmd = Command::new("cargo");
            cmd.arg("tauri").arg("build");

            if profile == "debug" {
                cmd.arg("--debug");
            }

            cmd.current_dir(&manifest_dir);
            cmd.envs(std::env::vars());
            run(cmd)?;
        }
    }

    Ok(())
}

/// フロントエンドの依存関係・ソースを確認し、必要に応じて npm install / npm run build を実行する
fn build_frontend_if_needed(manifest_dir: &Path) -> Result<()> {
    // manifest_dir = apps/mcv/src-tauri/  →  parent = apps/mcv/
    let frontend_dir = match manifest_dir.parent() {
        Some(p) => p,
        None => return Ok(()),
    };

    // ① npm install が必要か判定
    //    node_modules/.package-lock.json は npm install 実行時に更新されるため、
    //    package.json / package-lock.json より古ければ install が必要
    let installed_marker = frontend_dir.join("node_modules/.package-lock.json");
    let installed_mtime = latest_mtime(&installed_marker);

    let dep_files: Vec<PathBuf> = vec![
        frontend_dir.join("package.json"),
        frontend_dir.join("package-lock.json"),
    ];
    let needs_install = match installed_mtime {
        None => true, // node_modules が未セットアップ
        Some(installed) => dep_files
            .iter()
            .any(|p| latest_mtime(p).map(|m| m > installed).unwrap_or(false)),
    };

    if needs_install {
        println!("  依存関係に変更あり → npm install を実行");
        run_npm_install(frontend_dir)?;
    }

    // ② npm run build が必要か判定
    let dist_dir = frontend_dir.join("dist");
    let dist_mtime = match latest_mtime(&dist_dir) {
        Some(m) => m,
        None => {
            println!("  フロントエンド: dist/ が存在しないため npm run build を実行");
            return run_npm_build(frontend_dir);
        }
    };

    // 監視対象のフロントエンドソースファイル
    let src_paths: Vec<PathBuf> = vec![
        frontend_dir.join("src"),
        frontend_dir.join("index.html"),
        frontend_dir.join("vite.config.ts"),
        frontend_dir.join("package.json"),
        frontend_dir.join("tailwind.config.js"),
        frontend_dir.join("tsconfig.json"),
    ];

    let needs_build = src_paths
        .iter()
        .any(|p| latest_mtime(p).map(|m| m > dist_mtime).unwrap_or(false));

    if needs_build {
        println!("  フロントエンドに変更あり → npm run build を実行");
        run_npm_build(frontend_dir)?;
    } else {
        println!("  フロントエンド: 変更なし (スキップ)");
    }

    Ok(())
}

fn run_npm_install(dir: &Path) -> Result<()> {
    // Windows では npm は npm.cmd (バッチファイル) のため cmd /C 経由で実行する
    let mut cmd = npm_command();
    cmd.arg("install");
    cmd.current_dir(dir);
    run(cmd)
}

fn run_npm_build(dir: &Path) -> Result<()> {
    let mut cmd = npm_command();
    cmd.arg("run").arg("build");
    cmd.current_dir(dir);
    run(cmd)
}

/// Windows では `cmd /C npm`、それ以外では `npm` を返す
fn npm_command() -> Command {
    if cfg!(windows) {
        let mut cmd = Command::new("cmd");
        cmd.args(["/C", "npm"]);
        cmd
    } else {
        Command::new("npm")
    }
}

/// ソースの最新更新日時と出力 exe の更新日時を比較し、ビルドが必要かどうかを返す
fn should_build_tauri(pkg: &Package, manifest_dir: &Path, profile: &str) -> bool {
    // manifest_dir は src-tauri/ を指す。出力 exe はワークスペースの target/ に生成される
    let exe_path = PathBuf::from("target")
        .join(profile)
        .join(format!("{}.exe", pkg.name));

    let exe_mtime = match fs::metadata(&exe_path) {
        Ok(m) => m.modified().unwrap_or(SystemTime::UNIX_EPOCH),
        Err(_) => return true, // exe が存在しない → ビルド必要
    };

    // manifest_dir = apps/mcv/src-tauri/ なので parent() = apps/mcv/
    // フロントエンドは build_frontend_if_needed で事前にビルド済みのため、
    // dist/ の更新日時で判断する（src/ は見ない）
    let watch_paths: Vec<PathBuf> = vec![
        manifest_dir.join("src"),                                    // Rust バックエンド
        manifest_dir.join("tauri.conf.json"),                        // Tauri 設定
        manifest_dir.join("Cargo.toml"),                             // クレート依存
        manifest_dir.parent().unwrap().join("dist"),                 // ビルド済みフロントエンド
    ];

    for path in &watch_paths {
        if let Some(mtime) = latest_mtime(path) {
            if mtime > exe_mtime {
                return true;
            }
        }
    }

    println!("  (skipped: no changes)");
    false
}

/// ディレクトリまたはファイルの最新更新日時を再帰的に取得する
fn latest_mtime(path: &Path) -> Option<SystemTime> {
    if path.is_file() {
        return fs::metadata(path).ok()?.modified().ok();
    }
    if path.is_dir() {
        return fs::read_dir(path)
            .ok()?
            .filter_map(|e| e.ok())
            .filter_map(|e| latest_mtime(&e.path()))
            .max();
    }
    None
}

fn tauri_project_dir(pkg: &Package) -> Option<PathBuf> {
    let manifest_dir = pkg.manifest_path.parent()?;

    let candidates = ["tauri.conf.json", "tauri.conf.json5", "Tauri.toml"];

    for file in candidates {
        if manifest_dir.join(file).exists() {
            return Some(manifest_dir.to_path_buf().into());
        }
    }

    None
}

fn deploy(profile: &str, base_dir: &Path, crates: &[CrateDeploy]) -> Result<()> {
    println!("== Deploy to {:?} ==", base_dir);

    let target_dir = PathBuf::from("target").join(profile);

    for c in crates {
        let name = crate_name_from_path(&c.path)?;
        let dest = base_dir.join(&c.subdir);
        fs::create_dir_all(&dest)?;

        println!("Deploying {} -> {:?}", name, dest);
        let artifact_name = copy_artifacts(&target_dir, &dest, &name)?;

        // subdir が 2 階層以上深い場合（例: "plugins/plugin-twitch"）のみ manifest.json を生成する。
        // 1 階層（"plugins"）はメインアプリや汎用 DLL の配置先で manifest.json は不要なため。
        if c.subdir.contains('/') || c.subdir.contains('\\') {
            if let Some(artifact) = artifact_name {
                create_manifest(&dest, &artifact)?;
            }
        }
    }

    Ok(())
}

fn crate_name_from_path(path: &str) -> Result<String> {
    let manifest = Path::new(path).join("Cargo.toml");
    let content = fs::read_to_string(manifest)?;
    for line in content.lines() {
        if line.trim().starts_with("name") {
            let name = line.split('=').nth(1).unwrap().trim();
            return Ok(name.trim_matches('"').to_string());
        }
    }
    bail!("Could not determine crate name for {}", path);
}

/// コピーした成果物のメインファイル名を返す（manifest.json に記載するファイル名）。
/// DLL が存在すれば DLL 名を、なければ EXE 名を返す。
fn copy_artifacts(target: &Path, dest: &Path, name: &str) -> Result<Option<String>> {
    let dll_name = format!("{}.dll", name.replace('-', "_"));
    let exe_name = format!("{}.exe", name);
    let pdb_name = format!("{}.pdb", name.replace('-', "_"));

    let dll = target.join(&dll_name);
    let exe = target.join(&exe_name);
    let pdb = target.join(&pdb_name);

    let mut main_artifact = None;

    if dll.exists() {
        fs::copy(&dll, dest.join(&dll_name))?;
        main_artifact = Some(dll_name);
    }
    if exe.exists() {
        fs::copy(&exe, dest.join(&exe_name))?;
        // DLL がない場合（EXE プラグイン）は EXE 名をメインアーティファクトとする
        if main_artifact.is_none() {
            main_artifact = Some(exe_name);
        }
    }
    if pdb.exists() {
        fs::copy(&pdb, dest.join(&pdb_name))?;
    }

    Ok(main_artifact)
}

fn create_manifest(dest: &Path, dll_name: &str) -> Result<()> {
    let manifest_path = dest.join("manifest.json");

    let content = format!("{{\n  \"path\": \"{}\"\n}}\n", dll_name);

    fs::write(&manifest_path, content)?;
    println!("Created manifest: {:?}", manifest_path);

    Ok(())
}

fn run(mut cmd: Command) -> Result<()> {
    let status = cmd.status()?;
    if !status.success() {
        bail!("Command failed");
    }
    Ok(())
}
