$workspaceRoot = "C:\Users\ryu\Downloads\mcv"

Set-Location $workspaceRoot/apps/mcv/src-tauri
cargo tauri build --debug --features alpha

Set-Location $workspaceRoot/apps/exe-plugin-sample/src-tauri
cargo tauri build --debug # --features alpha

Set-Location $workspaceRoot/crates/plugin-dummy
cargo build --features alpha

Set-Location $workspaceRoot/crates/plugin-exe-manager
cargo build # --features alpha

New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer -Force
Copy-Item -Path $workspaceRoot/target/debug/MultiCommentViewer.exe -Destination $env:LOCALAPPDATA\MultiCommentViewer\
Copy-Item -Path $workspaceRoot/target/debug/MultiCommentViewer.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins -Force
Copy-Item -Path $workspaceRoot/target/debug/plugin_dummy.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/debug/plugin_dummy.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/debug/plugin_exe_manager.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/debug/plugin_exe_manager.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample -Force
Copy-Item -Path $workspaceRoot/target/debug/exe-plugin-sample.exe -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\
Copy-Item -Path $workspaceRoot/target/debug/exe_plugin_sample.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\
$manifest = @{
    path = "exe-plugin-sample.exe"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\manifest.json" -Encoding UTF8
