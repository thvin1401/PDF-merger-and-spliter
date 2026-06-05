using PdfSharp.Pdf.IO;
using Docnet.Core;
using Docnet.Core.Models;
using System.Runtime.InteropServices;
using PSharpDoc = PdfSharp.Pdf.PdfDocument;

namespace PdfTool;

public class MainForm : Form
{
    private RadioButton   _rdoMerge        = new();
    private RadioButton   _rdoSplit        = new();
    private ListBox       _lstFiles        = new();
    private Button        _btnAddFile      = new();
    private Button        _btnRemoveFile   = new();
    private Button        _btnMoveUp       = new();
    private Button        _btnMoveDown     = new();
    private Label         _lblPageRange    = new();
    private NumericUpDown _nudFrom         = new();
    private NumericUpDown _nudTo           = new();
    private Label         _lblPageInfo     = new();
    private Button        _btnProcess      = new();
    private Button        _btnPreview      = new();
    private Label         _lblPreviewTitle = new();
    private PictureBox    _picPreview      = new();
    private Button        _btnPrevPage     = new();
    private Button        _btnNextPage     = new();
    private Label         _lblCurrentPage  = new();
    private Label         _lblStatus       = new();

    private List<string> _inputFiles   = new();
    private string?      _outputFile;
    private List<Bitmap> _previewPages = new();
    private int          _previewIndex = 0;

    public MainForm()
    {
        // Must be set BEFORE InitializeUI so AutoScale works correctly
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode       = AutoScaleMode.None;

        InitializeUI();
        WireEvents();
        UpdateModeUI();
    }

    private void InitializeUI()
    {
        Text          = "PDF Tool — Merge & Split";
        ClientSize    = new Size(960, 620);
        MinimumSize   = new Size(780, 480);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = Color.FromArgb(245, 245, 248);
        Font          = new Font("Segoe UI", 9f);

        // ── Title bar ─────────────────────────────────────────
        var panelTop = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 44,
            BackColor = Color.FromArgb(30, 30, 60)
        };
        panelTop.Controls.Add(new Label
        {
            Text      = "PDF Tool",
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            AutoSize  = true,
            Location  = new Point(14, 10)
        });

        // ── Left panel ─────────────────────────────────────────
        var left = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 320,
            BackColor = Color.White
        };

        int x = 14, y = 10, w = 288;

        // Mode
        Add(left, BoldLabel("Mode"),             x, y,       w,  18); y += 22;
        Add(left, _rdoMerge,                     x, y,       w,  20); y += 22;
        _rdoMerge.Text    = "Merge (combine multiple PDFs)";
        _rdoMerge.Checked = true;
        Add(left, _rdoSplit,                     x, y,       w,  20); y += 22;
        _rdoSplit.Text    = "Split (extract page range)";
        Add(left, Sep(),                         x, y + 4,   w,   1); y += 14;

        // Files
        Add(left, BoldLabel("Input PDF Files"),  x, y,       w,  18); y += 22;
        Add(left, _lstFiles,                     x, y,       w, 110); y += 114;
        _lstFiles.BorderStyle         = BorderStyle.FixedSingle;
        _lstFiles.HorizontalScrollbar = true;

        _btnAddFile.Text    = "+ Add";    Btn(_btnAddFile,    Color.FromArgb(0, 120, 215));
        _btnRemoveFile.Text = "Remove";   Btn(_btnRemoveFile, Color.FromArgb(196, 43, 28));
        _btnMoveUp.Text     = "▲";        Btn(_btnMoveUp,     Color.FromArgb(100, 100, 120));
        _btnMoveDown.Text   = "▼";        Btn(_btnMoveDown,   Color.FromArgb(100, 100, 120));

        Add(left, _btnAddFile,    x,           y, 80, 26);
        Add(left, _btnRemoveFile, x + 84,      y, 80, 26);
        Add(left, _btnMoveUp,     x + 168,     y, 32, 26);
        Add(left, _btnMoveDown,   x + 204,     y, 32, 26);
        y += 34;

        Add(left, Sep(),                        x, y + 2,    w,   1); y += 12;

        // Page range
        Add(left, _lblPageRange,                x, y,        w,  18); y += 22;
        _lblPageRange.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

        Add(left, PlainLabel("From"),           x,      y + 3, 36, 18);
        Add(left, _nudFrom,                     x + 38, y,     68, 24);
        Add(left, PlainLabel("To"),             x + 114, y + 3, 24, 18);
        Add(left, _nudTo,                       x + 140, y,     68, 24);
        _nudFrom.Minimum = 1; _nudFrom.Maximum = 9999; _nudFrom.Value = 1;
        _nudTo.Minimum   = 1; _nudTo.Maximum   = 9999; _nudTo.Value   = 1;
        y += 32;

        Add(left, _lblPageInfo,                 x, y,        w,  18); y += 24;
        _lblPageInfo.Text      = "Add a file to see page count";
        _lblPageInfo.ForeColor = Color.Gray;

        Add(left, Sep(),                        x, y + 2,    w,   1); y += 12;

        // Action buttons
        _btnPreview.Text = "Preview";
        _btnProcess.Text = "Process & Save";
        _btnProcess.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        Btn(_btnPreview, Color.FromArgb(80, 130, 80));
        Btn(_btnProcess, Color.FromArgb(0, 150, 100));

        Add(left, _btnPreview, x,        y, 100, 30);
        Add(left, _btnProcess, x + 108,  y, 148, 30);
        y += 38;

        Add(left, _lblStatus,             x, y,        w,  36);
        _lblStatus.ForeColor = Color.FromArgb(0, 120, 60);

        // ── Divider ────────────────────────────────────────────
        var div = new Panel { Dock = DockStyle.Left, Width = 2, BackColor = Color.FromArgb(200, 200, 210) };

        // ── Right panel (preview) ──────────────────────────────
        var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 240, 245) };

        _lblPreviewTitle.Text      = "Preview";
        _lblPreviewTitle.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
        _lblPreviewTitle.Dock      = DockStyle.Top;
        _lblPreviewTitle.Height    = 28;
        _lblPreviewTitle.TextAlign = ContentAlignment.MiddleLeft;
        _lblPreviewTitle.Padding   = new Padding(8, 0, 0, 0);
        _lblPreviewTitle.BackColor = Color.FromArgb(225, 225, 235);

        _picPreview.Dock      = DockStyle.Fill;
        _picPreview.SizeMode  = PictureBoxSizeMode.Zoom;
        _picPreview.BackColor = Color.FromArgb(210, 210, 220);

        var navPanel = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = Color.FromArgb(225, 225, 235) };

        _btnPrevPage.Text = "< Prev"; Btn(_btnPrevPage, Color.FromArgb(90, 90, 130));
        _btnNextPage.Text = "Next >"; Btn(_btnNextPage, Color.FromArgb(90, 90, 130));
        _btnPrevPage.SetBounds(6,  5, 72, 26);
        _btnNextPage.SetBounds(82, 5, 72, 26);

        _lblCurrentPage.Text     = "No preview";
        _lblCurrentPage.AutoSize = true;
        _lblCurrentPage.Location = new Point(162, 10);

        navPanel.Controls.AddRange(new Control[] { _btnPrevPage, _btnNextPage, _lblCurrentPage });

        right.Controls.Add(_picPreview);
        right.Controls.Add(navPanel);
        right.Controls.Add(_lblPreviewTitle);

        Controls.Add(right);
        Controls.Add(div);
        Controls.Add(left);
        Controls.Add(panelTop);
    }

    // ── Layout helpers ────────────────────────────────────────
    private static void Add(Control parent, Control child, int x, int y, int w, int h)
    {
        child.SetBounds(x, y, w, h);
        parent.Controls.Add(child);
    }

    private static Label BoldLabel(string text) => new Label
    {
        Text     = text,
        Font     = new Font("Segoe UI", 9f, FontStyle.Bold),
        AutoSize = false
    };

    private static Label PlainLabel(string text) => new Label
    {
        Text     = text,
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Panel Sep() => new Panel { BackColor = Color.FromArgb(220, 220, 230) };

    private static void Btn(Button b, Color c)
    {
        b.BackColor = c;
        b.ForeColor = Color.White;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.Cursor    = Cursors.Hand;
        b.Font      = new Font("Segoe UI", 9f);
    }

    // ── Events ────────────────────────────────────────────────
    private void WireEvents()
    {
        _rdoMerge.CheckedChanged       += (_, _) => UpdateModeUI();
        _rdoSplit.CheckedChanged       += (_, _) => UpdateModeUI();
        _btnAddFile.Click              += BtnAddFile_Click;
        _btnRemoveFile.Click           += BtnRemoveFile_Click;
        _btnMoveUp.Click               += BtnMoveUp_Click;
        _btnMoveDown.Click             += BtnMoveDown_Click;
        _lstFiles.SelectedIndexChanged += LstFiles_SelectedIndexChanged;
        _btnProcess.Click              += BtnProcess_Click;
        _btnPreview.Click              += BtnPreview_Click;
        _btnPrevPage.Click             += (_, _) => NavigatePreview(-1);
        _btnNextPage.Click             += (_, _) => NavigatePreview(+1);
    }

    private void UpdateModeUI()
    {
        bool m             = _rdoMerge.Checked;
        _btnMoveUp.Enabled = _btnMoveDown.Enabled = m;
        _lblPageRange.Text = m
            ? "Page Range (applied to each file)"
            : "Page Range (pages to extract)";
    }

    private void BtnAddFile_Click(object? s, EventArgs e)
    {
        using var dlg = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", Multiselect = true };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        foreach (var f in dlg.FileNames)
        {
            if (_inputFiles.Contains(f)) continue;
            _inputFiles.Add(f);
            _lstFiles.Items.Add(Path.GetFileName(f));
        }

        if (_inputFiles.Count > 0)
        {
            try
            {
                using var doc     = PdfReader.Open(_inputFiles[^1], PdfDocumentOpenMode.Import);
                int pages         = doc.PageCount;
                _lblPageInfo.Text = $"Pages: {pages}";
                _nudFrom.Maximum  = pages; _nudFrom.Value = 1;
                _nudTo.Maximum    = pages; _nudTo.Value   = pages;
            }
            catch { _lblPageInfo.Text = "Could not read page count."; }
        }

        SetStatus($"{_inputFiles.Count} file(s) loaded.", Color.FromArgb(0, 100, 60));
    }

    private void BtnRemoveFile_Click(object? s, EventArgs e)
    {
        int i = _lstFiles.SelectedIndex;
        if (i < 0) return;
        _inputFiles.RemoveAt(i);
        _lstFiles.Items.RemoveAt(i);
    }

    private void BtnMoveUp_Click(object? s, EventArgs e)
    {
        int i = _lstFiles.SelectedIndex;
        if (i <= 0) return;
        Swap(i, i - 1);
        _lstFiles.SelectedIndex = i - 1;
    }

    private void BtnMoveDown_Click(object? s, EventArgs e)
    {
        int i = _lstFiles.SelectedIndex;
        if (i < 0 || i >= _lstFiles.Items.Count - 1) return;
        Swap(i, i + 1);
        _lstFiles.SelectedIndex = i + 1;
    }

    private void Swap(int a, int b)
    {
        (_inputFiles[a], _inputFiles[b]) = (_inputFiles[b], _inputFiles[a]);
        var t = _lstFiles.Items[a];
        _lstFiles.Items[a] = _lstFiles.Items[b];
        _lstFiles.Items[b] = t;
    }

    private void LstFiles_SelectedIndexChanged(object? s, EventArgs e)
    {
        int i = _lstFiles.SelectedIndex;
        if (i < 0) return;
        try
        {
            using var doc     = PdfReader.Open(_inputFiles[i], PdfDocumentOpenMode.Import);
            int pages         = doc.PageCount;
            _lblPageInfo.Text = $"Pages: {pages}";
            _nudFrom.Maximum  = pages;
            _nudTo.Maximum    = pages;
        }
        catch { _lblPageInfo.Text = "Could not read page count."; }
    }

    private void BtnProcess_Click(object? s, EventArgs e)
    {
        if (_inputFiles.Count == 0) { SetStatus("Add at least one PDF.", Color.Firebrick); return; }

        int from = (int)_nudFrom.Value, to = (int)_nudTo.Value;
        if (from > to) { SetStatus("'From' must be <= 'To'.", Color.Firebrick); return; }

        using var dlg = new SaveFileDialog
        {
            Filter   = "PDF Files (*.pdf)|*.pdf",
            FileName = _rdoMerge.Checked ? "merged.pdf" : "split.pdf"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        _outputFile = dlg.FileName;
        try
        {
            if (_rdoMerge.Checked) MergePdfs(_inputFiles, _outputFile, from, to);
            else                   SplitPdf(_inputFiles[0], _outputFile, from, to);

            SetStatus($"Saved: {Path.GetFileName(_outputFile)}", Color.FromArgb(0, 120, 60));
            LoadPreview(_outputFile);
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.Firebrick);
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void MergePdfs(List<string> files, string output, int from, int to)
    {
        using var outDoc = new PSharpDoc();
        foreach (var file in files)
        {
            using var inDoc = PdfReader.Open(file, PdfDocumentOpenMode.Import);
            for (int i = Math.Max(1, from) - 1; i <= Math.Min(to, inDoc.PageCount) - 1; i++)
                outDoc.AddPage(inDoc.Pages[i]);
        }
        outDoc.Save(output);
    }

    private static void SplitPdf(string file, string output, int from, int to)
    {
        using var inDoc  = PdfReader.Open(file, PdfDocumentOpenMode.Import);
        using var outDoc = new PSharpDoc();
        for (int i = Math.Max(1, from) - 1; i <= Math.Min(to, inDoc.PageCount) - 1; i++)
            outDoc.AddPage(inDoc.Pages[i]);
        outDoc.Save(output);
    }

    private void BtnPreview_Click(object? s, EventArgs e)
    {
        var target = (_outputFile != null && File.Exists(_outputFile)) ? _outputFile
                   : _lstFiles.SelectedIndex >= 0                      ? _inputFiles[_lstFiles.SelectedIndex]
                   : _inputFiles.Count > 0                             ? _inputFiles[0]
                   : null;

        if (target == null) { SetStatus("No file to preview.", Color.Firebrick); return; }
        try { LoadPreview(target); }
        catch (Exception ex) { SetStatus($"Preview error: {ex.Message}", Color.Firebrick); }
    }

    private void LoadPreview(string pdfPath)
    {
        foreach (var b in _previewPages) b.Dispose();
        _previewPages.Clear();
        _previewIndex = 0;

        using var lib = DocLib.Instance;
        using var doc = lib.GetDocReader(pdfPath, new PageDimensions(1.5));

        for (int i = 0; i < doc.GetPageCount(); i++)
        {
            using var page = doc.GetPageReader(i);
            int w = page.GetPageWidth(), h = page.GetPageHeight();
            var raw = page.GetImage();

            var bmp  = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, w, h),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(raw, 0, data.Scan0, raw.Length);
            bmp.UnlockBits(data);
            _previewPages.Add(bmp);
        }

        _lblPreviewTitle.Text = $"Preview — {Path.GetFileName(pdfPath)}  ({_previewPages.Count} page(s))";
        ShowPreviewPage(0);
    }

    private void ShowPreviewPage(int index)
    {
        if (_previewPages.Count == 0) return;
        _previewIndex        = Math.Clamp(index, 0, _previewPages.Count - 1);
        _picPreview.Image    = _previewPages[_previewIndex];
        _lblCurrentPage.Text = $"Page {_previewIndex + 1} / {_previewPages.Count}";
        _btnPrevPage.Enabled = _previewIndex > 0;
        _btnNextPage.Enabled = _previewIndex < _previewPages.Count - 1;
    }

    private void NavigatePreview(int delta) => ShowPreviewPage(_previewIndex + delta);

    private void SetStatus(string msg, Color color)
    {
        _lblStatus.ForeColor = color;
        _lblStatus.Text      = msg;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (var b in _previewPages) b.Dispose();
        base.OnFormClosed(e);
    }
}
