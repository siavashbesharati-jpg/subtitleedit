# Subtitle Editor - Quick Start

## 🎯 What's New?

Added a **professional subtitle editor** that opens after transcription completes, allowing you to edit both original and translated subtitles.

## ✨ Features

✅ **Edit Text** - Modify subtitle content  
✅ **Edit Timestamps** - Adjust start/end times  
✅ **Side-by-Side View** - Original and translated panels  
✅ **Add Subtitles** - Insert new entries  
✅ **Delete Subtitles** - Remove unwanted lines  
✅ **Save Changes** - Download edited SRT files  
✅ **Track Modifications** - See what's been changed  

## 🚀 How to Use

### Step 1: Complete Transcription
1. Go to http://localhost:5000/transcription-test.html
2. Upload a video file
3. Select translation (e.g., Farsi)
4. Wait for completion

### Step 2: Open Editor
1. After transcription completes, you'll see:
   - 📄 Download Original
   - 🌐 Download Translation
   - **✏️ Edit Subtitles** ← Click this!

2. Editor opens in a new window

### Step 3: Edit Subtitles

**Edit Table:**
```
┌────┬──────────────────┬────────────────────────┬─────────┐
│ #  │ ⏰ Timecode       │ 📝 Text                │ Actions │
├────┼──────────────────┼────────────────────────┼─────────┤
│ 1  │ 00:00:01,000     │ [Click to edit text]   │  🗑️    │
│    │ 00:00:03,500     │                        │         │
└────┴──────────────────┴────────────────────────┴─────────┘
```

**To Edit:**
- Click on any text area to modify content
- Click on timestamp to change timing
- Use 🗑️ to delete a subtitle
- Click ➕ Add Subtitle to insert new entry

### Step 4: Save Your Work
1. Click **💾 Save All Changes**
2. Files download automatically:
   - `{jobId}_original_edited.srt`
   - `{jobId}_translated_edited.srt`

## 📋 Toolbar Buttons

| Button | Function |
|--------|----------|
| 💾 Save All Changes | Download edited subtitles as SRT files |
| ➕ Add Subtitle | Insert a new subtitle row |
| 🔄 Reset All | Discard changes and reload originals |
| ❌ Close | Close editor window |

## 🎨 Layout

### With Translation (Dual Panel)
```
┌──────────────────────────────────────────────────────┐
│           📝 Subtitle Editor                         │
├──────────────────────────────────────────────────────┤
│  💾 Save  │  ➕ Add  │  🔄 Reset  │  ❌ Close        │
├─────────────────────────┬────────────────────────────┤
│  📄 Original            │  🌐 Translated             │
│  1. Hello world         │  1. سلام دنیا               │
│  2. How are you?        │  2. حال شما چطور است؟      │
└─────────────────────────┴────────────────────────────┘
```

### Without Translation (Single Panel)
```
┌──────────────────────────────────────────────────────┐
│           📝 Subtitle Editor                         │
├──────────────────────────────────────────────────────┤
│  💾 Save  │  ➕ Add  │  🔄 Reset  │  ❌ Close        │
├──────────────────────────────────────────────────────┤
│  📄 Original                                         │
│  1. Hello world                                      │
│  2. How are you?                                     │
└──────────────────────────────────────────────────────┘
```

## 🔧 Editing Tips

### ⏰ Timestamp Format
Use SRT standard: `HH:MM:SS,mmm`

**Examples:**
- `00:00:01,500` = 1.5 seconds
- `00:01:30,250` = 1 minute 30.25 seconds
- `01:23:45,678` = 1 hour 23 minutes 45.678 seconds

### 📝 Text Editing
- Multi-line text supported
- Press Enter for new line
- Text auto-grows as you type

### ➕ Adding Subtitles
1. Click "➕ Add Subtitle"
2. New row appears at bottom
3. Edit text and timestamps
4. Automatically renumbered

### 🗑️ Deleting Subtitles
1. Click 🗑️ button
2. Confirm deletion
3. Remaining subtitles auto-renumber

## 📊 Tracking Changes

**Info Bar Shows:**
- 📄 Job ID
- 🎬 Total Segments count
- ✏️ Modified count (updates live)

**Modified Counter:**
- Increments when you edit text
- Increments when you change timestamps
- Resets when you save
- Helps track unsaved work

## 💾 Saving Process

### What Happens:
1. Click "💾 Save All Changes"
2. Editor generates SRT format
3. Files download to your browser:
   - Original (if modified)
   - Translated (if modified)
4. Files named: `{jobId}_original_edited.srt`
5. Modified count resets to 0

### File Format:
```srt
1
00:00:01,000 --> 00:00:03,500
First subtitle text

2
00:00:04,000 --> 00:00:06,500
Second subtitle text
```

## 🌐 Browser Support

| Browser | Status |
|---------|--------|
| Chrome 90+ | ✅ Recommended |
| Firefox 88+ | ✅ Full support |
| Edge 90+ | ✅ Full support |
| Safari 14+ | ✅ Works well |

## ⚠️ Important Notes

1. **Changes are client-side** - Editing happens in your browser
2. **Save downloads files** - Changes saved as new SRT files
3. **Original preserved** - API files remain unchanged
4. **No auto-save** - Remember to save before closing
5. **Unsaved warning** - Prompted if closing with changes

## 🔗 Direct Access

You can access the editor directly via URL:
```
http://localhost:5000/subtitle-editor.html?jobId={YOUR_JOB_ID}
```

## 🎯 Common Use Cases

### 1. Fix Transcription Errors
```
Before: "Hell world"
After:  "Hello world"
```

### 2. Improve Translation
```
Before: "سلام دنیا" (literal)
After:  "سلام به همه" (natural)
```

### 3. Adjust Timing
```
Before: 00:00:01,000 → 00:00:02,000 (too short)
After:  00:00:01,000 → 00:00:04,000 (better)
```

### 4. Add Missing Content
```
Video has 15 seconds of speech but only 10 subtitles
→ Click ➕ to add missing subtitles
```

## 🐛 Troubleshooting

**Editor won't open?**
- Check API is running
- Verify job ID exists
- Check browser console (F12)

**Changes not saving?**
- Click "💾 Save All Changes" button
- Check browser downloads folder
- Ensure pop-ups not blocked

**Only one panel showing?**
- Normal if no translation requested
- Only original will display

**Can't edit timestamps?**
- Use correct format: `HH:MM:SS,mmm`
- Comma before milliseconds (not period)

## 📚 Files Created

### New Files
1. **subtitle-editor.html** - Main editor interface
2. **SUBTITLE_EDITOR_GUIDE.md** - Complete documentation

### Modified Files
1. **transcription-test.html** - Added "✏️ Edit Subtitles" button

## 🎉 Try It Now!

1. Start API (already running at http://localhost:5000)
2. Open http://localhost:5000/transcription-test.html
3. Upload a video
4. Wait for transcription
5. Click **"✏️ Edit Subtitles"**
6. Start editing!

Enjoy your new professional subtitle editor! 🚀
