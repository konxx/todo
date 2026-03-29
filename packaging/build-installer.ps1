param(
  [string]$AppVersion = (Get-Content (Join-Path $PSScriptRoot "..\VERSION") -Raw).Trim(),
  [string]$IsccPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
)

$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

if (-not (Test-Path $IsccPath)) {
  throw "ISCC.exe was not found at $IsccPath"
}

$buildRoot = Join-Path $projectRoot "build"
$stageDir = Join-Path $buildRoot "stage"
$installerDir = Join-Path $buildRoot "installer"

if (Test-Path $stageDir) {
  Remove-Item -LiteralPath $stageDir -Recurse -Force
}

New-Item -ItemType Directory -Path $stageDir -Force | Out-Null
New-Item -ItemType Directory -Path $installerDir -Force | Out-Null

Copy-Item -Path (Join-Path $projectRoot "launchers\start-todo.vbs") -Destination (Join-Path $stageDir "start-todo.vbs") -Force
Copy-Item -Path (Join-Path $projectRoot "README.md") -Destination (Join-Path $stageDir "README.md") -Force
Copy-Item -Path (Join-Path $projectRoot "VERSION") -Destination (Join-Path $stageDir "VERSION") -Force

& (Join-Path $PSScriptRoot "build-host.ps1") -OutputPath (Join-Path $stageDir "TodoDesk.exe")

$issFile = Join-Path $PSScriptRoot "TodoDesk.iss"

& $IsccPath "/DAppVersion=$AppVersion" "/DSourceRoot=$projectRoot" "/DStageDir=$stageDir" $issFile
