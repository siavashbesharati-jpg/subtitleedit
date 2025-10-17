# Extract the largest size from ICO and recreate as high-quality multi-size ICO

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

try {
    Write-Host "Improving CaptionFlow icon quality..." -ForegroundColor Green
    
    # Load the current ICO file
    $currentIcon = New-Object System.Drawing.Icon("captionflow_logo.ico")
    
    # Get the largest available size
    $bitmap = $currentIcon.ToBitmap()
    Write-Host "Current icon size: $($bitmap.Width)x$($bitmap.Height)" -ForegroundColor Cyan
    
    # Save as high-quality PNG first
    $bitmap.Save("temp_logo.png", [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "Extracted to temp PNG for processing" -ForegroundColor Yellow
    
    # Now create the multi-size ICO
    $sourceImage = [System.Drawing.Image]::FromFile("temp_logo.png")
    
    # Define comprehensive sizes for Windows
    $sizes = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 256)
    
    # Create ICO stream
    $icoStream = New-Object System.IO.MemoryStream
    
    # ICO header
    $icoStream.WriteByte(0) 
    $icoStream.WriteByte(0)
    $icoStream.WriteByte(1) 
    $icoStream.WriteByte(0)
    $icoStream.WriteByte($sizes.Length)
    $icoStream.WriteByte(0)
    
    $imageDataList = @()
    $dataOffset = 6 + ($sizes.Length * 16)
    
    Write-Host "Creating $($sizes.Length) optimized icon sizes..." -ForegroundColor Yellow
    
    foreach ($size in $sizes) {
        Write-Host "  Processing ${size}x${size}..." -ForegroundColor Gray
        
        $resizedBitmap = New-Object System.Drawing.Bitmap $size, $size
        $graphics = [System.Drawing.Graphics]::FromImage($resizedBitmap)
        
        # Maximum quality settings
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        
        # Clear with transparency
        $graphics.Clear([System.Drawing.Color]::Transparent)
        
        # Draw with high quality
        $graphics.DrawImage($sourceImage, 0, 0, $size, $size)
        $graphics.Dispose()
        
        # Use PNG format for better quality (especially for larger sizes)
        $format = if ($size -le 48) { [System.Drawing.Imaging.ImageFormat]::Png } else { [System.Drawing.Imaging.ImageFormat]::Png }
        
        $imgStream = New-Object System.IO.MemoryStream
        $resizedBitmap.Save($imgStream, $format)
        $imgData = $imgStream.ToArray()
        $imgStream.Dispose()
        $resizedBitmap.Dispose()
        
        $imageDataList += $imgData
        
        # Directory entry
        $width = if ($size -eq 256) { 0 } else { $size }
        $height = if ($size -eq 256) { 0 } else { $size }
        
        $icoStream.WriteByte($width)
        $icoStream.WriteByte($height)
        $icoStream.WriteByte(0) # Colors
        $icoStream.WriteByte(0) # Reserved
        $icoStream.WriteByte(1) # Planes
        $icoStream.WriteByte(0)
        $icoStream.WriteByte(32) # Bits per pixel
        $icoStream.WriteByte(0)
        
        # Size
        $dataSize = $imgData.Length
        $icoStream.WriteByte($dataSize -band 0xFF)
        $icoStream.WriteByte(($dataSize -shr 8) -band 0xFF)
        $icoStream.WriteByte(($dataSize -shr 16) -band 0xFF)
        $icoStream.WriteByte(($dataSize -shr 24) -band 0xFF)
        
        # Offset
        $icoStream.WriteByte($dataOffset -band 0xFF)
        $icoStream.WriteByte(($dataOffset -shr 8) -band 0xFF)
        $icoStream.WriteByte(($dataOffset -shr 16) -band 0xFF)
        $icoStream.WriteByte(($dataOffset -shr 24) -band 0xFF)
        
        $dataOffset += $dataSize
    }
    
    # Write image data
    foreach ($imgData in $imageDataList) {
        $icoStream.Write($imgData, 0, $imgData.Length)
    }
    
    # Save improved icon
    [System.IO.File]::WriteAllBytes("captionflow_logo_hq.ico", $icoStream.ToArray())
    
    # Cleanup
    $icoStream.Dispose()
    $sourceImage.Dispose()
    $currentIcon.Dispose()
    $bitmap.Dispose()
    
    Remove-Item "temp_logo.png" -ErrorAction SilentlyContinue
    
    $oldSize = (Get-Item "captionflow_logo.ico").Length
    $newSize = (Get-Item "captionflow_logo_hq.ico").Length
    
    Write-Host "SUCCESS! High-quality icon created!" -ForegroundColor Green
    Write-Host "Old size: $oldSize bytes -> New size: $newSize bytes" -ForegroundColor Cyan
    Write-Host "Contains $($sizes.Length) different resolutions" -ForegroundColor Cyan
    Write-Host "Ready to replace current icon files!" -ForegroundColor Yellow
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
}