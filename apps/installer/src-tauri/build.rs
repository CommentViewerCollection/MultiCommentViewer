fn main() {
    tauri_build::build();

    // Windows専用: manifestファイルを埋め込む
    #[cfg(windows)]
    {
        let manifest = "mcv-installer.exe.manifest";
        if std::path::Path::new(manifest).exists() {
            embed_resource::compile(manifest, embed_resource::NONE);
        } else {
            println!("cargo:warning=Manifest file {} not found", manifest);
        }
    }
}
