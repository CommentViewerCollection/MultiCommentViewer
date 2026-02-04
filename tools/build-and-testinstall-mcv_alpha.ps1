$workspaceRoot = "C:\Users\ryu\Downloads\mcv"

Set-Location $workspaceRoot/apps/mcv
npm install
npm run build
Set-Location $workspaceRoot/apps/mcv/src-tauri
cargo tauri build --features alpha

Set-Location $workspaceRoot/apps/exe-plugin-sample
npm install
npm run build
Set-Location $workspaceRoot/apps/exe-plugin-sample/src-tauri
cargo tauri build # --features alpha

Set-Location $workspaceRoot/crates/plugin-dummy
cargo build --release --features alpha

Set-Location $workspaceRoot/crates/plugin-exe-manager-v3
cargo build --release # --features alpha



New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer -Force
Copy-Item -Path $workspaceRoot/target/release/MultiCommentViewer.exe -Destination $env:LOCALAPPDATA\MultiCommentViewer\
Copy-Item -Path $workspaceRoot/target/release/MultiCommentViewer.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_dummy.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/release/plugin_dummy.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/release/plugin_exe_manager_v3.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/release/plugin_exe_manager_v3.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample -Force
Copy-Item -Path $workspaceRoot/target/release/exe-plugin-sample.exe -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\
Copy-Item -Path $workspaceRoot/target/release/exe_plugin_sample.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\
$manifest = @{
    path = "exe-plugin-sample.exe"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\manifest.json" -Encoding UTF8

# plugin-youtube-live
Set-Location $workspaceRoot/crates/plugin-youtube-live
cargo build --release --features alpha
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_youtube_live.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live\
Copy-Item -Path $workspaceRoot/target/release/plugin_youtube_live.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live\
$manifest = @{
    path = "plugin_youtube_live.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live\manifest.json" -Encoding UTF8

# plugin-sample
Set-Location $workspaceRoot/crates/plugin-sample
cargo build --release --features alpha
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_sample.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample\
Copy-Item -Path $workspaceRoot/target/release/plugin_sample.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample\
$manifest = @{
    path = "plugin_sample.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample\manifest.json" -Encoding UTF8
