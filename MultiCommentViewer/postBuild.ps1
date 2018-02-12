param([String]$targetDir, [String]$targetName)
$dllDirName = "dll"
$pluginDirName = "plugins"
$dllDir = "$targetDir" + "\" + $dllDirName

Remove-Item -Recurse -Force $dllDir -ErrorAction Ignore
New-Item "$dllDir" -type directory

$Files = Get-ChildItem -Path $targetDir  -Exclude "$targetName.*" | where {! $_.PSIsContainer}
ForEach ($file in $Files)
{
    Move-Item -Path "$File" -Destination "$dllDir"
}

$dirs = Get-ChildItem -Path $targetDir  -Exclude $dllDirName,$pluginDirName | where { $_.PSIsContainer }
ForEach ($dir in $dirs)
{
    Move-Item -Path "$dir" -Destination $dllDir
}