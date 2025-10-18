# CaptionFlow API - cURL Examples
# These examples demonstrate how to use the API with cURL

# Base URL
BASE_URL="https://localhost:5001"

# ============================================
# 1. Health Check
# ============================================
echo "1. Testing Health Check..."
curl -k -X GET "$BASE_URL/api/subtitle/health"
echo -e "\n"

# ============================================
# 2. Upload Video and Extract Subtitles
# ============================================
echo "2. Uploading video file..."
VIDEO_FILE="C:/Path/To/Your/Video.mkv"  # Change this path

UPLOAD_RESPONSE=$(curl -k -X POST "$BASE_URL/api/subtitle/extract" \
  -F "videoFile=@$VIDEO_FILE" \
  -F "sourceLanguage=en" \
  -F "useOcr=false")

echo $UPLOAD_RESPONSE
JOB_ID=$(echo $UPLOAD_RESPONSE | jq -r '.jobId')
echo "Job ID: $JOB_ID"
echo -e "\n"

# ============================================
# 3. Check Job Status
# ============================================
echo "3. Checking job status..."
sleep 2

curl -k -X GET "$BASE_URL/api/subtitle/status/$JOB_ID"
echo -e "\n"

# ============================================
# 4. Download SRT File (when completed)
# ============================================
echo "4. Downloading SRT file..."
curl -k -X GET "$BASE_URL/api/subtitle/download/$JOB_ID/srt" \
  -o "extracted_subtitles.srt"

echo "Downloaded to: extracted_subtitles.srt"
echo -e "\n"

# ============================================
# 5. List All Jobs
# ============================================
echo "5. Listing all jobs..."
curl -k -X GET "$BASE_URL/api/subtitle/jobs"
echo -e "\n"

# ============================================
# Alternative: Using curl for Windows PowerShell
# ============================================
# For Windows users without WSL/Git Bash, use PowerShell:
#
# # Upload video
# Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract" `
#   -Method Post `
#   -Form @{ videoFile = Get-Item "C:\Video.mkv" } `
#   -SkipCertificateCheck
#
# # Check status
# Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/status/JOB_ID" `
#   -Method Get `
#   -SkipCertificateCheck
#
# # Download SRT
# Invoke-WebRequest -Uri "https://localhost:5001/api/subtitle/download/JOB_ID/srt" `
#   -OutFile "subtitles.srt" `
#   -SkipCertificateCheck
