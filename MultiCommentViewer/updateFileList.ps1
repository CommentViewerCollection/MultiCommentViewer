param([String]$targetDir)

$filename = [System.String]::Format("{0}\list.txt", "$targetDir")
$Files = Get-ChildItem "$targetDir" -Name -Exclude "settings", "error.txt", "*.vshost.*" -Recurse -File

Set-Content "$filename" $Files
Add-Content "$filename" "list.txt"


$file = New-Object System.IO.StreamWriter($filename, $false, [System.Text.Encoding]::GetEncoding("utf-8"))
foreach($line in $Files) {
	if($line.StartsWith("settings\")) {
		continue
	}
	if($line -eq "error.txt") {
		continue
	}
	if($line.Contains(".vshost.")) {
		continue
	}
	$file.WriteLine($line)
}
$file.Close()
