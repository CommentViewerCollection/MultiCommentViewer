Param([string] $outputDir,[string] $targetDir,[string] $exePath,[string] $zipFileSuffix)

function Get-AssemblyVersion {
  param ([string]$asmPath)
  # LoadFile()を使うとファイルがロックされてあとでzip化する時に読めなくなる
  $Assembly = [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($asmPath))
  $AssemblyName = $Assembly.GetName()
  $Assemblyversion =  $AssemblyName.version
  $version = ""+($Assemblyversion.Major)+"."+($Assemblyversion.Minor)+"."+($Assemblyversion.Build)
  $version
}

function Get-FilenameWithoutExt {
 param([string]$path)
 [io.path]::GetFileNameWithoutExtension($path)
}


$outFileName = (Get-FilenameWithoutExt $exePath) + "_v" + (Get-AssemblyVersion $exePath) + "_" + $zipFileSuffix + ".zip"
$outFilePath = (Join-Path $outputDir $outFileName)

Compress-Archive -Path $targetDir -DestinationPath $outFilePath -Force

Write-Output $targetDir
Write-Output $outFilePath
