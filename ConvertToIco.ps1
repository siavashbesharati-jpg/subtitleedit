# PowerShell script to convert PNG to ICO format
# Usage: .\ConvertToIco.ps1 -InputPath "path\to\logo.png" -OutputPath "path\to\output.ico"

param(
    [Parameter(Mandatory=$true)]
    [string]$InputPath,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

try {
    # Load the PNG image
    $image = [System.Drawing.Image]::FromFile($InputPath)
    
    # Create different sizes for the ICO (16x16, 32x32, 48x48, 64x64, 128x128, 256x256)
    $sizes = @(16, 32, 48, 64, 128, 256)
    
    # Create a memory stream for the ICO data
    $icoStream = New-Object System.IO.MemoryStream
    
    # ICO header (6 bytes)
    $icoStream.WriteByte(0) # Reserved
    $icoStream.WriteByte(0)
    $icoStream.WriteByte(1) # Type: ICO
    $icoStream.WriteByte(0)
    $icoStream.WriteByte($sizes.Length) # Number of images
    $icoStream.WriteByte(0)
    
    $imageDataList = @()
    $directorySize = 6 + ($sizes.Length * 16) # Header + directory entries
    
    # Create resized images and prepare directory entries
    foreach ($size in $sizes) {
        # Resize image
        $resizedBitmap = New-Object System.Drawing.Bitmap $size, $size
        $graphics = [System.Drawing.Graphics]::FromImage($resizedBitmap)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($image, 0, 0, $size, $size)
        $graphics.Dispose()
        
        # Convert to PNG data
        $pngStream = New-Object System.IO.MemoryStream
        $resizedBitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngData = $pngStream.ToArray()
        $pngStream.Dispose()
        $resizedBitmap.Dispose()
        
        # Store image data
        $imageDataList += $pngData
        
        # Write directory entry
        $icoStream.WriteByte([Math]::Min($size, 255)) # Width (0 = 256)
        $icoStream.WriteByte([Math]::Min($size, 255)) # Height (0 = 256)
        $icoStream.WriteByte(0) # Color palette
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
        $icoStream.WriteByte($directorySize -band 0xFF)
        $icoStream.WriteByte(($directorySize -shr 8) -band 0xFF)
        $icoStream.WriteByte(($directorySize -shr 16) -band 0xFF)
        $icoStream.WriteByte(($directorySize -shr 24) -band 0xFF)
        
        $directorySize += $dataSize
    }
    
    # Write image data
    foreach ($imageData in $imageDataList) {
        $icoStream.Write($imageData, 0, $imageData.Length)
    }
    
    # Save to file
    [System.IO.File]::WriteAllBytes($OutputPath, $icoStream.ToArray())
    $icoStream.Dispose()
    $image.Dispose()
    
    Write-Host "Successfully converted $InputPath to $OutputPath" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}