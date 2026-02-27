use anyhow::{bail, Result};
use cargo_metadata::MetadataCommand;
use clap::{Args, Parser, Subcommand};
use std::fs;
use std::io::{Read, Write};
use std::path::{Path, PathBuf};
use std::process::Command;
use std::time::SystemTime;
use zip::write::SimpleFileOptions;
use zip::{CompressionMethod, ZipWriter};

// =========================================================
// CLI 定義
// =========================================================

#[derive(Parser)]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    /// ワークスペース全体をビルドする（フロントエンド変更検知 + Tauri ビルド含む）
    Build(BuildArgs),
    /// デバッグビルドしてローカルにインストールする
    Install(InstallArgs),
    /// 単一プラグインをリリースビルドして ZIP 化する
    Pack(PackArgs),
    /// apps/mcv と指定プラグインをリリースビルドして配布用 ZIP を生成する
    Dist(DistArgs),
}

#[derive(Args)]
struct BuildArgs {
    /// リリースビルド（省略時はデバッグビルド）
    #[arg(long)]
    release: bool,
}

#[derive(Args)]
struct InstallArgs {
    /// インストール先ディレクトリ
    #[arg(long)]
    dir: PathBuf,

    /// インストールするプラグインの ID（複数指定可、plugins.json に定義）
    #[arg(long = "plugin")]
    plugins: Vec<String>,
}

#[derive(Args)]
struct PackArgs {
    /// プラグイン ID（plugins.json に定義）
    #[arg(long)]
    plugin: String,

    /// 配布チャンネル
    #[arg(long, default_value = "alpha", value_parser = ["stable", "beta", "alpha"])]
    channel: String,
}

#[derive(Args)]
struct DistArgs {
    /// 配布チャンネル
    #[arg(long, default_value = "alpha", value_parser = ["stable", "beta", "alpha"])]
    channel: String,

    /// 対象プラグイン ID（省略時は plugins.json の全プラグイン）
    #[arg(long = "plugin")]
    plugins: Vec<String>,
}

// =========================================================
// plugins.json
// =========================================================

#[derive(serde::Deserialize, Clone)]
struct PluginInfo {
    /// Cargo.toml があるディレクトリ（ワークスペースルートからの相対パス）
    path: String,
    id: String,
    name: String,
    description: String,
    entry: String,
    api: String,
    has_channel_feature: bool,
}

fn read_plugins_json() -> Result<Vec<PluginInfo>> {
    let path = Path::new("tools/plugins.json");
    let content = fs::read_to_string(path)
        .map_err(|e| anyhow::anyhow!("tools/plugins.json を読み込めません: {}", e))?;
    let plugins: Vec<PluginInfo> = serde_json::from_str(&content)
        .map_err(|e| anyhow::anyhow!("tools/plugins.json のパースに失敗しました: {}", e))?;
    Ok(plugins)
}

fn find_plugin<'a>(plugins: &'a [PluginInfo], id: &str) -> Result<&'a PluginInfo> {
    plugins
        .iter()
        .find(|p| p.id == id)
        .ok_or_else(|| anyhow::anyhow!("plugins.json にプラグイン '{}' が見つかりません", id))
}

/// entry が .exe かどうかを判定する（EXE プラグインは DLL と扱いが異なる）
fn is_exe(info: &PluginInfo) -> bool {
    info.entry.ends_with(".exe")
}

// =========================================================
// エントリポイント
// =========================================================

fn main() -> Result<()> {
    let cli = Cli::parse();
    match cli.command {
        Commands::Build(args) => build(args),
        Commands::Install(args) => install(args),
        Commands::Pack(args) => pack(args),
        Commands::Dist(args) => dist(args),
    }
}

// =========================================================
// build コマンド
// =========================================================

fn build(args: BuildArgs) -> Result<()> {
    let profile = if args.release { "release" } else { "debug" };
    build_non_tauri(profile)?;
    build_tauri(profile)?;
    Ok(())
}

// =========================================================
// install コマンド
// =========================================================

fn install(args: InstallArgs) -> Result<()> {
    // ワークスペース全体をデバッグビルド（Tauri 含む）
    build_non_tauri("debug")?;
    build_tauri("debug")?;

    let plugins = read_plugins_json()?;
    let target_debug = PathBuf::from("target/debug");

    // 指定プラグインをデバッグビルドしてデプロイ
    for plugin_id in &args.plugins {
        let info = find_plugin(&plugins, plugin_id)?;
        println!("== Install plugin: {} ==", info.id);

        // プラグイン個別ビルド
        // EXE プラグイン（Tauri アプリ等）は cargo xtask build で別途ビルド済みを前提とする
        if !is_exe(info) {
            let mut cmd = Command::new("cargo");
            cmd.arg("build")
                .arg("--manifest-path")
                .arg(format!("{}/Cargo.toml", info.path));
            if info.has_channel_feature {
                // デバッグ用には alpha を使用
                cmd.arg("--features").arg("alpha");
            }
            run(cmd)?;
        }

        // {dir}/plugins/{id}/ にデプロイ
        let dest_dir = args.dir.join("plugins").join(&info.id);
        fs::create_dir_all(&dest_dir)?;

        let src = target_debug.join(&info.entry);
        if !src.exists() {
            if is_exe(info) {
                eprintln!(
                    "  ⚠ {} が見つかりません（先に cargo xtask build を実行してください）",
                    src.display()
                );
                continue;
            } else {
                bail!("{} が見つかりません", src.display());
            }
        }
        fs::copy(&src, dest_dir.join(&info.entry))?;

        // PDB のコピー（DLL のみ）
        if !is_exe(info) {
            let pdb_name = info.entry.replace(".dll", ".pdb");
            let pdb_src = target_debug.join(&pdb_name);
            if pdb_src.exists() {
                fs::copy(&pdb_src, dest_dir.join(&pdb_name))?;
            }
        }

        // 新形式 plugin.json (id 付き)
        let version = get_crate_version(&info.path).unwrap_or_else(|_| "0.0.0".to_string());
        // install コマンドは has_channel_feature=true の場合 alpha でビルドするため "alpha" を使用
        let channel = if info.has_channel_feature { "alpha" } else { "stable" };
        write_plugin_json_v2(&dest_dir, info, &version, channel)?;
        println!("  -> {:?}", dest_dir);
    }

    // mcv 本体 (MultiCommentViewer.exe) をコピー
    let exe_src = target_debug.join("MultiCommentViewer.exe");
    if exe_src.exists() {
        fs::copy(&exe_src, args.dir.join("MultiCommentViewer.exe"))?;
        println!("  -> {:?}", args.dir.join("MultiCommentViewer.exe"));
    }

    println!("Done: install to {:?}", args.dir);
    Ok(())
}

// =========================================================
// pack コマンド
// =========================================================

fn pack(args: PackArgs) -> Result<()> {
    let plugins = read_plugins_json()?;
    let info = find_plugin(&plugins, &args.plugin)?;
    pack_plugin(info, &args.channel)?;
    Ok(())
}

/// 単一プラグインをリリースビルドして ZIP 化し、output/ に出力する。
/// ZIP パスを返す。
fn pack_plugin(info: &PluginInfo, channel: &str) -> Result<PathBuf> {
    println!("== Pack plugin: {} (channel: {}) ==", info.id, channel);

    // リリースビルド
    // EXE プラグイン（Tauri アプリ等）は cargo xtask build --release で別途ビルド済みを前提とする
    if !is_exe(info) {
        let mut cmd = Command::new("cargo");
        cmd.arg("build")
            .arg("--manifest-path")
            .arg(format!("{}/Cargo.toml", info.path))
            .arg("--release");
        if info.has_channel_feature {
            cmd.arg("--features").arg(channel);
        }
        run(cmd)?;
    }

    // バージョン取得
    let version = get_crate_version(&info.path)?;

    // ステージングディレクトリ
    let output_dir = PathBuf::from("output");
    fs::create_dir_all(&output_dir)?;
    let work_dir = output_dir.join(format!("{}-{}-{}", info.id, version, channel));
    if work_dir.exists() {
        fs::remove_dir_all(&work_dir)?;
    }
    fs::create_dir_all(&work_dir)?;

    // エントリファイルをコピー
    let target_release = PathBuf::from("target/release");
    let entry_src = target_release.join(&info.entry);
    if !entry_src.exists() {
        if is_exe(info) {
            bail!(
                "{} が見つかりません（先に cargo xtask build --release を実行してください）",
                entry_src.display()
            );
        } else {
            bail!("{} が見つかりません", entry_src.display());
        }
    }
    fs::copy(&entry_src, work_dir.join(&info.entry))?;

    // PDB のコピー（DLL のみ）
    if !is_exe(info) {
        let pdb_name = info.entry.replace(".dll", ".pdb");
        let pdb_src = target_release.join(&pdb_name);
        if pdb_src.exists() {
            fs::copy(&pdb_src, work_dir.join(&pdb_name))?;
        }
    }

    // 新形式 plugin.json
    write_plugin_json_v2(&work_dir, info, &version, channel)?;

    // ZIP 化
    let zip_path = output_dir.join(format!("{}-{}-{}.zip", info.id, version, channel));
    if zip_path.exists() {
        fs::remove_file(&zip_path)?;
    }
    create_zip_from_dir(&work_dir, &zip_path)?;
    fs::remove_dir_all(&work_dir)?;

    println!("Done: {}", zip_path.display());
    Ok(zip_path)
}

// =========================================================
// dist コマンド
// =========================================================

fn dist(args: DistArgs) -> Result<()> {
    let all_plugins = read_plugins_json()?;

    // 対象プラグインを解決（指定なし = 全プラグイン）
    let target_plugins: Vec<&PluginInfo> = if args.plugins.is_empty() {
        all_plugins.iter().collect()
    } else {
        args.plugins
            .iter()
            .map(|id| find_plugin(&all_plugins, id))
            .collect::<Result<Vec<_>>>()?
    };

    let channel = args.channel.as_str();

    // フロントエンドビルド
    let mcv_manifest_dir = PathBuf::from("apps/mcv/src-tauri");
    build_frontend_if_needed(&mcv_manifest_dir)?;

    // mcv 本体リリースビルド
    println!("== Build MultiCommentViewer (channel: {}) ==", channel);
    let mut build_mcv = Command::new("cargo");
    build_mcv
        .arg("build")
        .arg("--manifest-path")
        .arg("apps/mcv/src-tauri/Cargo.toml")
        .arg("--release")
        .arg("--features")
        .arg(channel);
    run(build_mcv)?;

    // mcv バージョン取得
    let mcv_version = get_crate_version("apps/mcv/src-tauri")?;

    // 各プラグインをビルドして ZIP 化
    let mut plugin_zips: Vec<PathBuf> = Vec::new();
    for info in &target_plugins {
        let zip_path = pack_plugin(info, channel)?;
        plugin_zips.push(zip_path);
    }

    // ステージングディレクトリ
    let bundle_name = format!("MultiCommentViewer_v{}_{}", mcv_version, channel);
    let output_dir = PathBuf::from("output");
    let stage_dir = output_dir.join(&bundle_name);
    let zip_path = output_dir.join(format!("{}.zip", bundle_name));

    if stage_dir.exists() {
        fs::remove_dir_all(&stage_dir)?;
    }
    fs::create_dir_all(stage_dir.join("plugins"))?;

    // mcv 本体コピー
    let target_release = PathBuf::from("target/release");
    let exe_src = target_release.join("MultiCommentViewer.exe");
    if !exe_src.exists() {
        bail!("{} が見つかりません", exe_src.display());
    }
    fs::copy(&exe_src, stage_dir.join("MultiCommentViewer.exe"))?;

    let pdb_src = target_release.join("MultiCommentViewer.pdb");
    if pdb_src.exists() {
        fs::copy(&pdb_src, stage_dir.join("MultiCommentViewer.pdb"))?;
    }

    // プラグイン ZIP を plugins/ にコピー
    for zip in &plugin_zips {
        let dest = stage_dir.join("plugins").join(zip.file_name().unwrap());
        fs::copy(zip, &dest)?;
        println!("  + plugins/{}", zip.file_name().unwrap().to_string_lossy());
    }

    // 最終 ZIP 化
    println!("== Creating final ZIP ==");
    if zip_path.exists() {
        fs::remove_file(&zip_path)?;
    }
    create_zip_from_dir(&stage_dir, &zip_path)?;
    fs::remove_dir_all(&stage_dir)?;

    println!("Done: {}", zip_path.display());
    Ok(())
}

// =========================================================
// ユーティリティ
// =========================================================

/// Cargo.toml のパスからクレートのバージョンを取得する
fn get_crate_version(path: &str) -> Result<String> {
    let abs_manifest = fs::canonicalize(PathBuf::from(path).join("Cargo.toml"))
        .map_err(|e| anyhow::anyhow!("マニフェストが見つかりません '{}': {}", path, e))?;
    let metadata = MetadataCommand::new().exec()?;
    metadata
        .packages
        .iter()
        .find(|p| {
            fs::canonicalize(p.manifest_path.as_std_path())
                .map(|mp| mp == abs_manifest)
                .unwrap_or(false)
        })
        .map(|p| p.version.to_string())
        .ok_or_else(|| anyhow::anyhow!("パス '{}' のクレートが見つかりません", path))
}

/// 新形式 plugin.json を書く（id, name, description, version, channel, entry, api）
fn write_plugin_json_v2(
    dest: &Path,
    plugin: &PluginInfo,
    version: &str,
    channel: &str,
) -> Result<()> {
    let manifest = serde_json::json!({
        "id":          plugin.id,
        "name":        plugin.name,
        "description": plugin.description,
        "version":     version,
        "channel":     channel,
        "entry":       plugin.entry,
        "api":         plugin.api,
    });
    let json = serde_json::to_string_pretty(&manifest)?;
    fs::write(dest.join("plugin.json"), json + "\n")?;
    Ok(())
}

fn build_non_tauri(profile: &str) -> Result<()> {
    println!("== Non-Tauri build ({}) ==", profile);

    // Tauri アプリは cargo tauri build で別途ビルドするため除外する。
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

            build_frontend_if_needed(&manifest_dir)?;

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
    let installed_marker = frontend_dir.join("node_modules/.package-lock.json");
    let installed_mtime = latest_mtime(&installed_marker);

    let dep_files: Vec<PathBuf> = vec![
        frontend_dir.join("package.json"),
        frontend_dir.join("package-lock.json"),
    ];
    let needs_install = match installed_mtime {
        None => true,
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
fn should_build_tauri(
    pkg: &cargo_metadata::Package,
    manifest_dir: &Path,
    profile: &str,
) -> bool {
    let exe_path = PathBuf::from("target")
        .join(profile)
        .join(format!("{}.exe", pkg.name));

    let exe_mtime = match fs::metadata(&exe_path) {
        Ok(m) => m.modified().unwrap_or(SystemTime::UNIX_EPOCH),
        Err(_) => return true,
    };

    let watch_paths: Vec<PathBuf> = vec![
        manifest_dir.join("src"),
        manifest_dir.join("tauri.conf.json"),
        manifest_dir.join("Cargo.toml"),
        manifest_dir.parent().unwrap().join("dist"),
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

fn tauri_project_dir(pkg: &cargo_metadata::Package) -> Option<PathBuf> {
    let manifest_dir = pkg.manifest_path.parent()?;

    let candidates = ["tauri.conf.json", "tauri.conf.json5", "Tauri.toml"];

    for file in candidates {
        if manifest_dir.join(file).exists() {
            return Some(manifest_dir.to_path_buf().into());
        }
    }

    None
}

fn create_zip_from_dir(src_dir: &Path, zip_path: &Path) -> Result<()> {
    let zip_file = fs::File::create(zip_path)?;
    let mut zip = ZipWriter::new(zip_file);
    let options = SimpleFileOptions::default()
        .compression_method(CompressionMethod::Deflated)
        .compression_level(Some(9));

    add_dir_to_zip(&mut zip, src_dir, src_dir, options)?;
    zip.finish()?;
    Ok(())
}

fn add_dir_to_zip(
    zip: &mut ZipWriter<fs::File>,
    root: &Path,
    current: &Path,
    options: SimpleFileOptions,
) -> Result<()> {
    for entry in fs::read_dir(current)? {
        let entry = entry?;
        let path = entry.path();
        if path.is_dir() {
            add_dir_to_zip(zip, root, &path, options)?;
            continue;
        }

        let rel = path.strip_prefix(root)?;
        let rel_name = rel.to_string_lossy().replace('\\', "/");
        zip.start_file(rel_name, options)?;

        let mut f = fs::File::open(&path)?;
        let mut buffer = Vec::new();
        f.read_to_end(&mut buffer)?;
        zip.write_all(&buffer)?;
    }
    Ok(())
}

fn run(mut cmd: Command) -> Result<()> {
    let status = cmd.status()?;
    if !status.success() {
        bail!("Command failed");
    }
    Ok(())
}
