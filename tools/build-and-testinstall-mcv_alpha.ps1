$workspaceRoot = "C:\Users\ryu\Downloads\mcv"

# mcv
npm install --prefix "$workspaceRoot/packages/my-dataview"
npm run build --prefix "$workspaceRoot/packages/my-dataview"
npm install --prefix "$workspaceRoot/apps/mcv"
npm run build --prefix "$workspaceRoot/apps/mcv"
cargo build --release --features alpha --manifest-path "$workspaceRoot/apps/mcv/src-tauri/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer -Force
Copy-Item -Path $workspaceRoot/target/release/MultiCommentViewer.exe -Destination $env:LOCALAPPDATA\MultiCommentViewer\
Copy-Item -Path $workspaceRoot/target/release/MultiCommentViewer.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\

# create plugins dir
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins -Force

# plugin-dummy
cargo build --release --features alpha --manifest-path "$workspaceRoot/crates/plugin-dummy/Cargo.toml"
Copy-Item -Path $workspaceRoot/target/release/plugin_dummy.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/release/plugin_dummy.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\

# plugin-exe-manager-v2
# cargo build --release --manifest-path "$workspaceRoot/crates/plugin-exe-manager/Cargo.toml"
# Copy-Item -Path $workspaceRoot/target/release/plugin_exe_manager.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
# Copy-Item -Path $workspaceRoot/target/release/plugin_exe_manager.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\

# plugin-exe-manager-v3
cargo build --release --manifest-path "$workspaceRoot/crates/plugin-exe-manager-v3/Cargo.toml"
Copy-Item -Path $workspaceRoot/target/release/plugin_exe_manager_v3.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\
Copy-Item -Path $workspaceRoot/target/release/plugin_exe_manager_v3.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\

# exe-plugin-sample
npm install --prefix "$workspaceRoot/apps/exe-plugin-sample"
npm run build --prefix "$workspaceRoot/apps/exe-plugin-sample"
cargo build --release --manifest-path "$workspaceRoot/apps/exe-plugin-sample/src-tauri/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample -Force
Copy-Item -Path $workspaceRoot/target/release/exe-plugin-sample.exe -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\
Copy-Item -Path $workspaceRoot/target/release/exe_plugin_sample.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\
$manifest = @{
    path = "exe-plugin-sample.exe"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\exe-plugin-sample\manifest.json" -Encoding UTF8

# plugin-youtube-live
cargo build --release --features alpha --manifest-path "$workspaceRoot/crates/plugin-youtube-live/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_youtube_live.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live\
Copy-Item -Path $workspaceRoot/target/release/plugin_youtube_live.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live\
$manifest = @{
    path = "plugin_youtube_live.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-youtube-live\manifest.json" -Encoding UTF8

# plugin-chrome-cookie
cargo build --release --features alpha --manifest-path "$workspaceRoot/crates/plugin-chrome-cookie/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-chrome-cookie -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_chrome_cookie.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-chrome-cookie\
Copy-Item -Path $workspaceRoot/target/release/plugin_chrome_cookie.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-chrome-cookie\
$manifest = @{
    path = "plugin_chrome_cookie.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-chrome-cookie\manifest.json" -Encoding UTF8

# plugin-bouyomi
cargo build --release --features alpha --manifest-path "$workspaceRoot/crates/plugin-bouyomi/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-bouyomi -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_bouyomi.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-bouyomi\
Copy-Item -Path $workspaceRoot/target/release/plugin_bouyomi.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-bouyomi\
$manifest = @{
    path = "plugin_bouyomi.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-bouyomi\manifest.json" -Encoding UTF8

# plugin-sample-v3
cargo build --release --features alpha --manifest-path "$workspaceRoot/crates/plugin-sample-v3/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample-v3 -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_sample_v3.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample-v3\
Copy-Item -Path $workspaceRoot/target/release/plugin_sample_v3.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample-v3\
$manifest = @{
    path = "plugin_sample_v3.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-sample-v3\manifest.json" -Encoding UTF8

# plugin-twitch
cargo build --release --features alpha --manifest-path "$workspaceRoot/crates/plugin-twitch/Cargo.toml"
New-Item -ItemType Directory -Path $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-twitch -Force
Copy-Item -Path $workspaceRoot/target/release/plugin_twitch.dll -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-twitch\
Copy-Item -Path $workspaceRoot/target/release/plugin_twitch.pdb -Destination $env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-twitch\
$manifest = @{
    path = "plugin_twitch.dll"
}
$manifest | ConvertTo-Json | Out-File -FilePath "$env:LOCALAPPDATA\MultiCommentViewer\plugins\plugin-twitch\manifest.json" -Encoding UTF8
