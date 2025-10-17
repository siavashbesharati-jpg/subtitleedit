# CaptionFlow Logo Replacement Instructions

## What has been completed:
✅ **Application name changed from "Subtitle Edit" to "CaptionFlow"**
✅ **About dialog updated to show "CaptionFlow"**
✅ **Application is now running with the new name**

## What you need to do to complete the logo replacement:

### Step 1: Save your logo image
1. Save your CaptionFlow logo image as **`captionflow_logo.png`** in the following directory:
   ```
   E:\IRANEXPEDIA\Subedit\subtitleedit\
   ```

### Step 2: Convert and replace the logo
1. Open Command Prompt or PowerShell as Administrator
2. Navigate to the project directory:
   ```
   cd "E:\IRANEXPEDIA\Subedit\subtitleedit"
   ```
3. Run the logo replacement script:
   ```
   ReplaceLogo.bat
   ```

### Alternative Method (Manual):
If the batch script doesn't work, you can:

1. Use an online PNG to ICO converter (like convertio.co or favicon.io)
2. Convert your CaptionFlow logo to ICO format with multiple sizes (16x16, 32x32, 48x48, 64x64, 128x128, 256x256)
3. Replace these files with your new ICO file:
   - `src\ui\SE.ico`
   - `src\ui\Resources\SE.ico` 
   - `src\ui\Icons\SE.ico`

### Step 3: Rebuild the application
After replacing the logo files:
```
cd "E:\IRANEXPEDIA\Subedit\subtitleedit"
dotnet build
```

### Step 4: Run your customized CaptionFlow
```
"E:\IRANEXPEDIA\Subedit\subtitleedit\src\ui\bin\Debug\net48\SubtitleEdit.exe"
```

## Changes Made:
- Application title: "Subtitle Edit" → "CaptionFlow"
- Window title: "Untitled - Subtitle Edit 4.0.13" → "Untitled - CaptionFlow 4.0.13"
- About dialog: "About Subtitle Edit" → "About CaptionFlow"
- Product name in About: "Subtitle Edit 3.2" → "CaptionFlow 3.2"

Your CaptionFlow application is now ready! The logo replacement just needs your PNG file to be converted to ICO format and placed in the correct locations.