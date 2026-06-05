# PDF Tool — Merge & Split

A simple WinForms app to merge and split PDF files, with page preview.

## Features
- **Merge**: Combine multiple PDFs into one (optionally extract specific page range from each)
- **Split**: Extract a page range from a single PDF
- **Preview**: View any PDF page-by-page after processing
- **Single .exe**: Ships as one self-contained executable, no install needed

---

## Requirements
- Visual Studio 2022 (or `dotnet` CLI)
- .NET 8 SDK

---

## Build & Run (Visual Studio)

1. Open `PdfTool.sln` (or open the `PdfTool` folder)
2. Restore NuGet packages (auto on first build)
3. Press **F5** to run in debug mode

---

## Publish as a Single .exe

Run in the project folder:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Output will be at:
```
bin\Release\net8.0-windows\win-x64\publish\PdfTool.exe
```

This single `.exe` runs on any Windows PC with no install required.

---

## Usage

1. **Select mode**: Merge or Split
2. **Add PDF files** using the "+ Add File(s)" button
   - For Merge: add multiple files; reorder with ▲ ▼
   - For Split: only the first file is used
3. **Set page range** (From / To) — click a file to see its page count
4. Click **▶ Process & Save** → choose output location
5. Preview auto-loads after saving; or click **🔍 Preview** anytime

---

## NuGet Packages Used

| Package | Purpose |
|---|---|
| `PdfSharp` | Merge / split PDF pages |
| `PdfiumViewer` | Render PDF pages as images for preview |
| `PdfiumViewer.Native.x86_64.v8-xfa` | Native Pdfium DLL (bundled into exe) |
