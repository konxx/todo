param(
  [string]$OutputPath = (Join-Path $PSScriptRoot "..\build\dev\TodoDesk.exe"),
  [switch]$Run
)

$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$sourceFiles = Get-ChildItem (Join-Path $projectRoot "desktop-host") -Filter *.cs | Select-Object -ExpandProperty FullName
if ([System.IO.Path]::IsPathRooted($OutputPath)) {
  $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
} else {
  $resolvedOutputPath = [System.IO.Path]::GetFullPath((Join-Path $projectRoot $OutputPath))
}
$outputDir = Split-Path -Parent $resolvedOutputPath
$iconOutputPath = Join-Path $outputDir "app.ico"
$cscPath = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not $sourceFiles -or $sourceFiles.Count -eq 0) {
  throw "No desktop source files were found under $projectRoot\desktop-host"
}

if (-not (Test-Path $cscPath)) {
  throw "csc.exe was not found at $cscPath"
}

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
& (Join-Path $PSScriptRoot "build-icon.ps1") -OutputPath $iconOutputPath

& $cscPath `
  /nologo `
  /target:winexe `
  /optimize+ `
  /out:$resolvedOutputPath `
  /win32icon:$iconOutputPath `
  /r:System.dll `
  /r:System.Core.dll `
  /r:System.Drawing.dll `
  /r:System.Runtime.Serialization.dll `
  /r:System.Windows.Forms.dll `
  $sourceFiles

if ($LASTEXITCODE -ne 0) {
  throw "Host build failed."
}

if ($Run) {
  Start-Process -FilePath $resolvedOutputPath -WorkingDirectory (Split-Path -Parent $resolvedOutputPath) | Out-Null
}
