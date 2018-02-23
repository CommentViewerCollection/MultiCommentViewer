param([String]$targetDir)

$filename = [System.String]::Format("{0}\list.txt", "$targetDir")
$Files = Get-ChildItem "$targetDir" -Name -Exclude "settings", "error.txt", "*.vshost.*" -Recurse -File

Set-Content "$filename" $Files
Add-Content "$filename" "list.txt"
