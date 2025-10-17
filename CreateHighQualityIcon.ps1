# PowerShell script to create a multi-resolution icon from your CaptionFlow logo
# This will create an ICO with multiple sizes: 16x16, 24x24, 32x32, 48x48, 64x64, 128x128, 256x256

param(
    [Parameter(Mandatory=$true)]
    [string]$InputPath,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

try {
    Write-Host "Creating high-quality multi-resolution icon..." -ForegroundColor Green
    
    # Load the source image
    $sourceImage = [System.Drawing.Image]::FromFile($InputPath)
    Write-Host "Source image loaded: $($sourceImage.Width)x$($sourceImage.Height)" -ForegroundColor Cyan
    
    # Define all the sizes Windows expects for optimal display
    $sizes = @(16, 24, 32, 48, 64, 96, 128, 256)
    
    # Create a memory stream for the ICO data
    $icoStream = New-Object System.IO.MemoryStream
    
    # Write ICO header (6 bytes)
    $icoStream.WriteByte(0) # Reserved (must be 0)
    $icoStream.WriteByte(0)
    $icoStream.WriteByte(1) # Type: 1 = ICO
    $icoStream.WriteByte(0)
    $icoStream.WriteByte($sizes.Length) # Number of images
    $icoStream.WriteByte(0)
    
    $imageDataList = @()
    $dataOffset = 6 + ($sizes.Length * 16) # Header + directory entries
    
    Write-Host "Creating $($sizes.Length) different icon sizes..." -ForegroundColor Yellow
    
    # Create each size
    foreach ($size in $sizes) {
        Write-Host "  Creating ${size}x${size} icon..." -ForegroundColor Gray
        
        # Create high-quality resized bitmap
        $resizedBitmap = New-Object System.Drawing.Bitmap $size, $size
        $graphics = [System.Drawing.Graphics]::FromImage($resizedBitmap)
        
        # Use high-quality rendering
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        
        # Draw the image
        $graphics.DrawImage($sourceImage, 0, 0, $size, $size)
        $graphics.Dispose()
        
        # Convert to PNG data (PNG format inside ICO for better quality)
        $pngStream = New-Object System.IO.MemoryStream
        $resizedBitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngData = $pngStream.ToArray()
        $pngStream.Dispose()
        $resizedBitmap.Dispose()
        
        # Store image data
        $imageDataList += $pngData
        
        # Write directory entry (16 bytes each)
        $icoStream.WriteByte([Math]::Min($size, 255)) # Width (0 = 256)
        $icoStream.WriteByte([Math]::Min($size, 255)) # Height (0 = 256)
        $icoStream.WriteByte(0) # Color palette (0 for PNG)
        $icoStream.WriteByte(0) # Reserved
        $icoStream.WriteByte(1) # Color planes
        $icoStream.WriteByte(0)
        $icoStream.WriteByte(32) # Bits per pixel
        $icoStream.WriteByte(0)
        
        # Data size (4 bytes, little-endian)
        $dataSize = $pngData.Length
        $icoStream.WriteByte($dataSize -band 0xFF)
        $icoStream.WriteByte(($dataSize -shr 8) -band 0xFF)
        $icoStream.WriteByte(($dataSize -shr 16) -band 0xFF)
        $icoStream.WriteByte(($dataSize -shr 24) -band 0xFF)
        
        # Data offset (4 bytes, little-endian)
        $icoStream.WriteByte($dataOffset -band 0xFF)
        $icoStream.WriteByte(($dataOffset -shr 8) -band 0xFF)
        $icoStream.WriteByte(($dataOffset -shr 16) -band 0xFF)
        $icoStream.WriteByte(($dataOffset -shr 24) -band 0xFF)
        
        $dataOffset += $dataSize
    }
    
    Write-Host "Writing image data..." -ForegroundColor Yellow
    
    # Write all image data
    foreach ($imageData in $imageDataList) {
        $icoStream.Write($imageData, 0, $imageData.Length)
    }
    
    # Save to file
    [System.IO.File]::WriteAllBytes($OutputPath, $icoStream.ToArray())
    $icoStream.Dispose()
    $sourceImage.Dispose()
    
    $finalSize = (Get-Item $OutputPath).Length
    Write-Host "SUCCESS! High-quality icon created: $OutputPath" -ForegroundColor Green
    Write-Host "File size: $finalSize bytes (was $((Get-Item $InputPath).Length) bytes)" -ForegroundColor Cyan
    Write-Host "Contains $($sizes.Length) different resolutions for optimal display" -ForegroundColor Cyan
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}