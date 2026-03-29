@echo off
setlocal

cd /d "%~dp0.."

powershell -NoProfile -ExecutionPolicy Bypass -File ".\packaging\build-host.ps1" -OutputPath ".\build\dev\TodoDesk.exe" -Run

endlocal
