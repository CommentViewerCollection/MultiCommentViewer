use anyhow::{bail, Result};
use cargo_metadata::MetadataCommand;
use clap::Parser;
use std::{fs, path::PathBuf, process::Command};

#[derive(Parser)]
struct Args {
    #[arg(long)]
    crate_name: String,

    #[arg(long, value_delimiter = ',')]
    plugin: Vec<String>,

    #[arg(long)]
    target: String,

    #[arg(long)]
    channel: String,
}

fn main() -> Result<()> {
    let args = Args::parse();

    let metadata = MetadataCommand::new().exec()?;

    let core_pkg = metadata
        .packages
        .iter()
        .find(|p| p.name == args.crate_name)
        .ok_or_else(|| anyhow::anyhow!("core crate not found"))?;

    let version = core_pkg.version.to_string();

    // build core
    run_cargo_build(&args.crate_name, &args.target, &args.channel)?;

    // build plugins
    for plugin in &args.plugin {
        run_cargo_build(plugin, &args.target, &args.channel)?;
    }

    // staging
    let stage_root = PathBuf::from(format!(
        "output/{}_v{}_{}",
        args.crate_name, version, args.channel
    ));
    // copy core artifacts
    copy_binary(&args.crate_name, &args.target, &stage_root)?;
    copy_pdb(&args.crate_name, &args.target, &stage_root)?;
    
    // copy plugins
    if !args.plugin.is_empty() {
        let plugin_dir = stage_root.join("plugins");
        fs::create_dir_all(&plugin_dir)?;
        for plugin in &args.plugin {
            let dst = plugin_dir.join(plugin);
            fs::create_dir_all(&dst)?;
            copy_plugin(plugin, &args.target, &dst)?;
        }
    }

    // zip
    let zip_name = format!("{}.zip", stage_root.display());
    run_zip(&stage_root, &zip_name)?;

    Ok(())
}

fn run_cargo_build(crate_name: &str, target: &str, channel: &str) -> Result<()> {
    let mut cmd = Command::new("cargo");
    cmd.args(["build", "-p", crate_name, "--release", "--target", target]);

    if !channel.is_empty() {
        cmd.args(["--features", channel]);
    }

    let status = cmd.status()?;
    if !status.success() {
        bail!("build failed: {}", crate_name);
    }

    Ok(())
}

fn copy_binary(crate_name: &str, target: &str, dst: &PathBuf) -> Result<()> {
    let exe = if cfg!(windows) {
        format!("{}.exe", crate_name)
    } else {
        crate_name.to_string()
    };
    let src = PathBuf::from(format!("target/{}/release/{}", target, exe));
    fs::create_dir_all(&dst)?;
    fs::copy(src, dst.join(exe))?;
    Ok(())
}

fn copy_pdb(crate_name: &str, target: &str, dst: &PathBuf) -> Result<()> {
    let pdb = format!("{}.pdb", crate_name);
    let src = PathBuf::from(format!("target/{}/release/{}", target, pdb));
    if src.exists() {
        fs::copy(src, dst.join(pdb))?;
    }
    Ok(())
}
fn normalize_crate_name(name: &str) -> String {
    name.replace('-', "_")
}

fn copy_plugin(crate_name: &str, target: &str, dst: &PathBuf) -> Result<()> {
    let base = normalize_crate_name(crate_name);

    let lib = if cfg!(windows) {
        format!("{}.dll", base)
    } else if cfg!(target_os = "macos") {
        format!("lib{}.dylib", base)
    } else {
        format!("lib{}.so", base)
    };

    let src = PathBuf::from(format!("target/{}/release/{}", target, lib));
    fs::copy(src, dst.join(lib))?;
    Ok(())
}

fn run_zip(dir: &PathBuf, zip_name: &str) -> Result<()> {
    let path = format!(r"{}\*", dir.display());

    let status = Command::new("powershell")
        .args([
            "-NoProfile",
            "-Command",
            &format!(
                "Compress-Archive -Path '{}' -DestinationPath '{}' -Force",
                path, zip_name
            ),
        ])
        .status()?;

    if !status.success() {
        bail!("zip failed");
    }

    Ok(())
}
