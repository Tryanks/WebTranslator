# Export Page Design (Fluent v2) — Diff Preview and Active Save

Date: 2025-10-10
Owner: Junie (JetBrains EAP agent)

## Goals
- Preview change diff for zh_cn.json (or zh_cn.lang) before saving.
- When the project is opened via a local folder, support saving directly back to the original file (“origin folder”).
- Provide a more proactive/active save experience (auto-save toggle + one-click save-to-origin).

## UX Overview
1. Command Bar
   - 复制到剪贴板: Copies the export content.
   - 另存为: Opens the platform Save dialog.
   - 保存到原文件: Writes the export content to the file inside the origin folder (only meaningful when opened from Local Folder).
2. Header Card
   - Shows file name and format, and current character count.
   - Checkbox: 自动保存到原文件夹 (visible only when opened from Local Folder). When enabled, the app will immediately save the generated export to the original zh_cn file.
3. Content Card
   - Read-only editor showing the final export file text.
4. Diff Preview Card
   - Read-only editor showing a compact line-based diff between “原文件” and “导出内容”.
   - Unchanged lines are collapsed. Changed lines are emitted with “- ” (old) and “+ ” (new).
   - If the origin file doesn’t exist, the card shows a message: “(原文件不存在，将创建新文件)”.

## Behaviors
- Open Source Context
  - If the user opened files from GitHub or Manual input, origin folder isn’t available; Auto Save UI is hidden.
  - If the user opened a Local Folder, the app records the folder path as the origin folder.
- Diff Preview
  - Loads the existing zh_cn.json (or zh_cn.lang) from the origin folder if present, and computes a simple line diff vs the exported content.
  - The diff is for preview/awareness only; it isn’t meant to be a full patch tool.
- Active Save
  - 保存到原文件: One-click write to the origin file path.
  - 自动保存到原文件夹: When toggled on, automatically saves to origin immediately (and again whenever export content is regenerated in this session).

## Technical Notes
- Context Bridging
  - Introduced ProjectContextService to store ImportMode and OriginFolderPath.
  - OpenFileViewModel sets this context when opening via Local Folder, GitHub, or Manual.
- ExportViewModel
  - New properties: AutoSaveToOrigin, OriginFolderPath, IsLocalFolder, HasDiff, DiffDocument.
  - Methods: SaveToOriginCommand (writes export to origin); ComputeLineDiff (very lightweight line-based diff for preview).
  - SetParameter: Builds export text and metadata, reads origin file when applicable, builds diff, defaults AutoSaveToOrigin=true when from Local Folder.
- UI (ExportView.axaml)
  - Added 保存到原文件 button in CommandBar.
  - Added Auto Save checkbox in header; visible only when IsLocalFolder.
  - Added a Diff preview card bound to DiffDocument and HasDiff.

## Future Improvements
- Replace simple line diff with a richer diff library (folding, coloring, hunk navigation).
- Provide a non-blocking background auto-save (debounced) tied to content changes.
- Add a confirmation dialog or backup creation before overwriting origin files.
- Persist user preference for Auto Save between sessions.
- Support additional locales and multi-file export/compare.
