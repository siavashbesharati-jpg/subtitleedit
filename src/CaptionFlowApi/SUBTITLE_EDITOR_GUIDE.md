# Subtitle Editor Feature

## Overview
The **Subtitle Editor** is a powerful web-based tool that allows users to edit both original and translated subtitles with full control over text content and timestamps.

## Features

### ✨ Key Capabilities
- **📝 Edit Text** - Modify subtitle text in real-time
- **⏰ Edit Timestamps** - Adjust start and end times for each subtitle
- **🌐 Dual Panel View** - Edit original and translated subtitles side-by-side
- **➕ Add Subtitles** - Insert new subtitle entries
- **🗑️ Delete Subtitles** - Remove unwanted subtitle entries
- **💾 Save Changes** - Download edited subtitles as SRT files
- **🔄 Reset** - Discard all changes and reload original
- **📊 Track Modifications** - See how many subtitles have been modified

## How to Access

### From Transcription Page
1. Complete a transcription job
2. Click the **"✏️ Edit Subtitles"** button
3. Editor opens in a new window with your subtitles loaded

### Direct Access
```
http://localhost:5000/subtitle-editor.html?jobId={YOUR_JOB_ID}
```

## User Interface

### Editor Layout

```
┌─────────────────────────────────────────────────────────────┐
│                     📝 Subtitle Editor                      │
├─────────────────────────────────────────────────────────────┤
│  Job ID: abc123  |  Total: 50  |  Modified: 5              │
├─────────────────────────────────────────────────────────────┤
│  💾 Save  |  ➕ Add  |  🔄 Reset  |  ❌ Close                │
├──────────────────────────┬──────────────────────────────────┤
│  📄 Original Subtitle    │  🌐 Translated Subtitle         │
├──────────────────────────┼──────────────────────────────────┤
│  #  | ⏰ Time  | 📝 Text │  #  | ⏰ Time  | 📝 Text         │
├──────────────────────────┼──────────────────────────────────┤
│  1  | 00:00:01 | Hello   │  1  | 00:00:01 | سلام            │
│     | 00:00:03 |         │     | 00:00:03 |                 │
├──────────────────────────┼──────────────────────────────────┤
│  2  | 00:00:04 | World   │  2  | 00:00:04 | دنیا            │
│     | 00:00:06 |         │     | 00:00:06 |                 │
└──────────────────────────┴──────────────────────────────────┘
```

### Table Columns

| Column | Description |
|--------|-------------|
| **#** | Subtitle number (sequential) |
| **⏰ Time** | Start and end timestamps (editable) |
| **📝 Text** | Subtitle text content (editable textarea) |
| **Actions** | Delete button (🗑️) |

## Editing Subtitles

### Edit Text
1. Click on any text area
2. Type your changes
3. Text is automatically marked as modified

### Edit Timestamps
Timestamps use SRT format: `HH:MM:SS,mmm`

**Example:**
- `00:00:01,500` = 1.5 seconds
- `00:01:30,250` = 1 minute 30.25 seconds

**To Edit:**
1. Click on the timestamp input field
2. Type the new time in format `HH:MM:SS,mmm`
3. Change is automatically tracked

### Add New Subtitle
1. Click **"➕ Add Subtitle"** button
2. New row appears at the bottom
3. Edit text and timestamps
4. Save when done

### Delete Subtitle
1. Click the **🗑️** button on any row
2. Confirm deletion
3. Subtitle is removed and remaining entries are renumbered

## Saving Changes

### Save Process
1. Click **"💾 Save All Changes"** button
2. Modified subtitles are downloaded as SRT files:
   - `{jobId}_original_edited.srt` - Original subtitle
   - `{jobId}_translated_edited.srt` - Translated subtitle

### What Gets Saved
- ✅ Text modifications
- ✅ Timestamp changes
- ✅ New subtitles added
- ✅ Deleted subtitles removed
- ✅ Proper SRT formatting

## Technical Details

### SRT Format
The editor reads and writes standard SubRip (SRT) format:

```srt
1
00:00:01,000 --> 00:00:03,500
First subtitle text

2
00:00:04,000 --> 00:00:06,500
Second subtitle text
```

### Data Loading
```javascript
// Editor loads subtitles via API
GET /api/transcription/download/{jobId}/original
GET /api/transcription/download/{jobId}/translated
```

### Parsing Logic
```javascript
function parseSRT(srtContent) {
    // Splits content by double newlines
    // Extracts: number, timecode, text
    // Returns array of subtitle objects
}
```

### Saving Logic
```javascript
function generateSRT(subtitles) {
    // Formats subtitles back to SRT format
    // Downloads as .srt file
}
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl + S` | Save changes (future enhancement) |
| `Tab` | Move to next field |
| `Shift + Tab` | Move to previous field |

## Features Breakdown

### 1. **Dual Panel View**
- Original subtitle on left
- Translated subtitle on right (if available)
- Synchronized scrolling (future enhancement)
- Side-by-side comparison

### 2. **Real-time Editing**
- Changes tracked instantly
- Modified count updates live
- No page refresh needed
- Auto-save tracking

### 3. **Smart Renumbering**
- Automatic after deletions
- Maintains sequential order
- Updates both panels

### 4. **Validation** (Future Enhancement)
- Timestamp format validation
- Overlapping time detection
- Empty text warnings
- Duration checks

## Use Cases

### 1. **Fix Transcription Errors**
```
Original: "Hell world"
Fixed:    "Hello world"
```

### 2. **Adjust Timing**
```
Before: 00:00:01,000 --> 00:00:02,000 (too short)
After:  00:00:01,000 --> 00:00:03,500 (better duration)
```

### 3. **Improve Translations**
```
Auto-translated: "سلام دنیا" (literal)
Human-edited:    "سلام به همه" (natural)
```

### 4. **Merge Subtitles**
```
Before:
1. Hello
2. World

After:
1. Hello World
```

### 5. **Split Long Subtitles**
```
Before:
1. This is a very long subtitle that spans multiple lines

After:
1. This is a very long subtitle
2. that spans multiple lines
```

## Best Practices

### ⚡ Performance Tips
- Save frequently to avoid data loss
- Work on smaller subtitle files first
- Use modern browsers (Chrome, Firefox, Edge)

### ✅ Quality Tips
- Check timestamp overlaps
- Ensure proper duration (min 1 second)
- Keep text concise and readable
- Sync with video playback

### 🎯 Workflow Tips
1. Load subtitles
2. Review full transcript
3. Edit text first
4. Adjust timing second
5. Save changes
6. Test with video player

## Error Handling

### Common Errors

**"Failed to load subtitles"**
- Check job ID is valid
- Ensure transcription completed
- Verify API is running

**"No job ID provided"**
- Access editor from transcription page
- Or provide valid `?jobId=` parameter

**"Translated file not found"**
- Translation was not requested
- Only original panel will show
- This is normal behavior

## Browser Compatibility

| Browser | Supported | Notes |
|---------|-----------|-------|
| Chrome 90+ | ✅ | Recommended |
| Firefox 88+ | ✅ | Full support |
| Edge 90+ | ✅ | Full support |
| Safari 14+ | ✅ | May have minor UI differences |
| IE 11 | ❌ | Not supported |

## Future Enhancements

### Planned Features
- [ ] **Video Preview** - Play video alongside subtitles
- [ ] **Undo/Redo** - Multi-level change history
- [ ] **Find & Replace** - Batch text modifications
- [ ] **Spell Check** - Integrated spell checker
- [ ] **Export Formats** - VTT, ASS, SSA formats
- [ ] **Collaboration** - Multi-user editing
- [ ] **Auto-save** - Periodic background saves
- [ ] **Keyboard Shortcuts** - Power user features
- [ ] **Timestamp Calculator** - Duration helpers
- [ ] **Subtitle Merging** - Combine multiple files

## API Integration

### Endpoints Used

```javascript
// Load subtitles
GET /api/transcription/result/{jobId}
GET /api/transcription/download/{jobId}/original
GET /api/transcription/download/{jobId}/translated
```

### Response Format
```json
{
  "jobId": "abc123",
  "segments": [
    {
      "number": 1,
      "start": "00:00:01,000",
      "end": "00:00:03,500",
      "text": "Hello world"
    }
  ]
}
```

## Troubleshooting

### Problem: Changes not saving
**Solution:** Click "💾 Save All Changes" button - changes are downloaded as new files

### Problem: Can't edit timestamps
**Solution:** Use format `HH:MM:SS,mmm` (comma for milliseconds, not period)

### Problem: Only one panel showing
**Solution:** Normal if no translation was requested - only original exists

### Problem: Editor won't open
**Solution:** 
- Check if API is running
- Verify job ID is valid
- Check browser console for errors

## Example Workflow

### Complete Editing Session

1. **Start Transcription**
   - Upload video
   - Select Farsi translation
   - Wait for completion

2. **Open Editor**
   - Click "✏️ Edit Subtitles"
   - New window opens

3. **Review Subtitles**
   - Scroll through both panels
   - Identify errors

4. **Make Edits**
   - Fix spelling: "recieve" → "receive"
   - Adjust timing: 00:00:01,000 → 00:00:01,500
   - Improve translation: Auto → Natural

5. **Add Missing Subtitle**
   - Click "➕ Add Subtitle"
   - Fill in text and timestamps

6. **Delete Duplicate**
   - Find duplicate entry
   - Click 🗑️ button

7. **Save Work**
   - Click "💾 Save All Changes"
   - Download both edited files

8. **Test Result**
   - Play video with edited subtitles
   - Verify improvements

## Summary

The Subtitle Editor provides:
- ✅ Full editing control
- ✅ Dual-language support
- ✅ Easy-to-use interface
- ✅ Professional SRT output
- ✅ No data loss
- ✅ Quick access from transcription page

Perfect for:
- 🎬 Video producers
- 🌐 Translators
- 📚 Content creators
- 🎓 Educators
- 🎥 YouTubers
