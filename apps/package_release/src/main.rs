use anyhow::{anyhow, Context, Result};
use clap::Parser;
use serde::Deserialize;
use std::{
    fs::{self, File},
    io::{self, Read},
    path::{Path, PathBuf},
};
use zip::{write::SimpleFileOptions, ZipWriter};

#[derive(Deserialize)]
struct TauriConfig {
    version: String,
}
#[derive(Parser)]
struct Args {
    /// 対象バイナリ名（拡張子なし）
    #[arg(long)]
    bin: String,

    /// 出力ディレクトリ
    #[arg(long, default_value = "output")]
    output: PathBuf,

    #[arg(long)]
    channel: String,
}
fn main() -> Result<()> {
    let args = Args::parse();

    let root = workspace_root()?;
    let version = read_version(&root, &args.bin)?;
    let (exe, pdb) = find_artifacts(&root)?;

    let output_dir = root.join("output");
    fs::create_dir_all(&output_dir).context("failed to create output directory")?;

    let zip_name = match args.bin.as_str() {
        "mcv" => format!("MultiCommentViewer_v{}_{}.zip", version, args.channel),
        "installer" => format!("mcv-installer_v{}_{}.zip", version, args.channel),
        _ => return Err(anyhow!("unknown binary name: {}", args.bin)),
    };
    // let zip_name = format!("MultiCommentViewer_v{}.zip", version);
    let zip_path = output_dir.join(zip_name);

    create_zip(&zip_path, &[exe, pdb])?;

    println!("Created: {}", zip_path.display());
    Ok(())
}

fn workspace_root() -> Result<PathBuf> {
    let exe = std::env::current_exe()?;
    let mut dir = exe
        .parent()
        .ok_or_else(|| anyhow!("cannot determine exe parent"))?;

    for _ in 0..2 {
        dir = dir
            .parent()
            .ok_or_else(|| anyhow!("cannot find workspace root"))?;
    }
    Ok(dir.to_path_buf())
}

fn read_version(root: &Path, bin:&impl AsRef<Path>) -> Result<String> {
    let path = root
        .join("apps")
        .join(bin)
        .join("src-tauri")
        .join("tauri.conf.json");
    let mut buf = String::new();

    File::open(&path)
        .with_context(|| format!("failed to open {}", path.display()))?
        .read_to_string(&mut buf)?;

    let config: TauriConfig =
        serde_json::from_str(&buf).context("failed to parse tauri.conf.json")?;

    Ok(config.version)
}

fn find_artifacts(root: &Path) -> Result<(PathBuf, PathBuf)> {
    let dir = root.join("target").join("release");

    let exe = dir
        .read_dir()?
        .filter_map(Result::ok)
        .map(|e| e.path())
        .find(|p| p.extension().and_then(|e| e.to_str()) == Some("exe"))
        .ok_or_else(|| anyhow!("exe not found in target/release"))?;

    let pdb = dir
        .read_dir()?
        .filter_map(Result::ok)
        .map(|e| e.path())
        .find(|p| p.extension().and_then(|e| e.to_str()) == Some("pdb"))
        .ok_or_else(|| anyhow!("pdb not found in target/release"))?;

    Ok((exe, pdb))
}

fn create_zip(zip_path: &Path, files: &[PathBuf]) -> Result<()> {
    let file = File::create(zip_path)?;
    let mut zip = ZipWriter::new(file);

    let options = SimpleFileOptions::default()
        .compression_method(zip::CompressionMethod::Deflated)
        .unix_permissions(0o755);

    for path in files {
        let name = path
            .file_name()
            .and_then(|n| n.to_str())
            .ok_or_else(|| anyhow!("invalid filename"))?;

        zip.start_file(name, options)?;
        let mut f = File::open(path)?;
        io::copy(&mut f, &mut zip)?;
    }

    zip.finish()?;
    Ok(())
}
