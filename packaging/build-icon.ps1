param(
  [string]$SourcePath = (Join-Path $PSScriptRoot "..\app.jpg"),
  [Parameter(Mandatory = $true)]
  [string]$OutputPath
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$resolvedSourcePath = (Resolve-Path $SourcePath).Path
if ([System.IO.Path]::IsPathRooted($OutputPath)) {
  $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
} else {
  $resolvedOutputPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputPath))
}

$outputDir = Split-Path -Parent $resolvedOutputPath
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$iconSize = 256
$originalImage = [System.Drawing.Image]::FromFile($resolvedSourcePath)

try {
  $iconBitmap = New-Object System.Drawing.Bitmap $iconSize, $iconSize, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  try {
    $graphics = [System.Drawing.Graphics]::FromImage($iconBitmap)
    try {
      $graphics.Clear([System.Drawing.Color]::Transparent)
      $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
      $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
      $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
      $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

      $scale = [Math]::Min($iconSize / $originalImage.Width, $iconSize / $originalImage.Height)
      $drawWidth = [int][Math]::Round($originalImage.Width * $scale)
      $drawHeight = [int][Math]::Round($originalImage.Height * $scale)
      $drawX = [int][Math]::Round(($iconSize - $drawWidth) / 2)
      $drawY = [int][Math]::Round(($iconSize - $drawHeight) / 2)

      $destinationRect = New-Object System.Drawing.Rectangle $drawX, $drawY, $drawWidth, $drawHeight
      $graphics.DrawImage($originalImage, $destinationRect)
    }
    finally {
      $graphics.Dispose()
    }

    $pngStream = New-Object System.IO.MemoryStream
    try {
      $iconBitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
      $pngBytes = $pngStream.ToArray()
    }
    finally {
      $pngStream.Dispose()
    }
  }
  finally {
    $iconBitmap.Dispose()
  }
}
finally {
  $originalImage.Dispose()
}

$fileStream = [System.IO.File]::Open($resolvedOutputPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
$writer = New-Object System.IO.BinaryWriter $fileStream

try {
  $writer.Write([UInt16]0)
  $writer.Write([UInt16]1)
  $writer.Write([UInt16]1)
  $writer.Write([byte]0)
  $writer.Write([byte]0)
  $writer.Write([byte]0)
  $writer.Write([byte]0)
  $writer.Write([UInt16]1)
  $writer.Write([UInt16]32)
  $writer.Write([UInt32]$pngBytes.Length)
  $writer.Write([UInt32]22)
  $writer.Write($pngBytes)
}
finally {
  $writer.Dispose()
  $fileStream.Dispose()
}
