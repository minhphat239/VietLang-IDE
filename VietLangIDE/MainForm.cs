// VietLangIDE.cs — VietLang IDE (WinForms)
// Chạy: dotnet run --project VietLang

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VietLangIDE;

public class MainForm : Form
{
    private MenuStrip menuStrip;
    private TabControl tabControl;
    private RichTextBox output;
    private Label statusLabel;
    private Label fileLabel;
    private Label zoomLabel;
    private Splitter outputSplitter;
    private System.Windows.Forms.Timer syntaxTimer;
    private System.Windows.Forms.Timer scrollSyncTimer;
    private float zoomLevel = 1.0f;
    private const float DefaultFontSize = 11f;
    private const string DefaultFontName = "Consolas";
    private List<TabData> tabs = new List<TabData>();
    private int tabIndexCounter = 0;

    private class TabData
    {
        public string FilePath = "";
        public RichTextBox Editor;
        public Panel LineNumberPanel;
        public int TabIndex;
    }

    public MainForm()
    {
        Text = "VietLang IDE";
        Size = new Size(950, 750);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Application;

        // ── Menu ──
        menuStrip = new MenuStrip();
        menuStrip.BackColor = Color.FromArgb(240, 240, 240);
        CreateMenus();

        // ── Toolbar ──
        var toolbar = new FlowLayoutPanel();
        toolbar.Dock = DockStyle.Top;
        toolbar.Height = 42;
        toolbar.BackColor = Color.FromArgb(240, 240, 240);
        toolbar.Padding = new Padding(4, 4, 4, 4);

        var btnOpen = MakeButton("📂 Mở");
        var btnSave = MakeButton("💾 Lưu");
        var btnRun = MakeButton("▶ Chạy");
        var btnClear = MakeButton("🗑 Xóa");

        btnOpen.Click += (s, e) => OpenFile();
        btnSave.Click += (s, e) => SaveFile();
        btnRun.Click += (s, e) => RunCode();
        btnClear.Click += (s, e) => { output.Clear(); Status("Đã xóa output"); };

        toolbar.Controls.AddRange(new Control[] { btnOpen, btnSave, btnRun, btnClear });

        // ── File label ──
        fileLabel = new Label();
        fileLabel.Dock = DockStyle.Top;
        fileLabel.Height = 22;
        fileLabel.BackColor = Color.WhiteSmoke;
        fileLabel.Font = new Font("Segoe UI", 9);
        fileLabel.Text = "  Chưa mở file";
        fileLabel.Padding = new Padding(4, 2, 0, 0);

        // ── Tab Control ──
        tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font("Segoe UI", 9);
        tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        tabControl.MouseClick += TabControl_MouseClick;
        tabControl.KeyDown += TabControl_KeyDown;
        var ctxMenu = new ContextMenuStrip();
        ctxMenu.Items.Add("Đóng tab", null, (s, e) => CloseCurrentTab());
        tabControl.ContextMenuStrip = ctxMenu;

        // ── Output ──
        output = new RichTextBox();
        output.Dock = DockStyle.Bottom;
        output.Height = 180;
        output.Font = new Font("Consolas", 10);
        output.BackColor = Color.FromArgb(15, 15, 15);
        output.ForeColor = Color.LightGreen;
        output.ReadOnly = true;
        output.WordWrap = true;

        // ── Output Splitter ──
        outputSplitter = new Splitter();
        outputSplitter.Dock = DockStyle.Bottom;
        outputSplitter.Height = 4;
        outputSplitter.MinSize = 80;

        // ── Status ──
        statusLabel = new Label();
        statusLabel.Dock = DockStyle.Bottom;
        statusLabel.Height = 22;
        statusLabel.BackColor = Color.WhiteSmoke;
        statusLabel.Font = new Font("Segoe UI", 9);
        statusLabel.Text = "  Sẵn sàng";

        zoomLabel = new Label();
        zoomLabel.Dock = DockStyle.Right;
        zoomLabel.Width = 70;
        zoomLabel.BackColor = Color.WhiteSmoke;
        zoomLabel.Font = new Font("Segoe UI", 9);
        zoomLabel.TextAlign = ContentAlignment.MiddleRight;
        zoomLabel.Padding = new Padding(0, 2, 8, 0);
        zoomLabel.Text = "100%";

        var statusPanel = new Panel();
        statusPanel.Dock = DockStyle.Bottom;
        statusPanel.Height = 22;
        statusPanel.BackColor = Color.WhiteSmoke;
        statusPanel.Controls.Add(zoomLabel);
        statusPanel.Controls.Add(statusLabel);

        // ── Controls ──
        Controls.Add(tabControl);
        Controls.Add(outputSplitter);
        Controls.Add(output);
        Controls.Add(statusPanel);
        Controls.Add(fileLabel);
        Controls.Add(toolbar);
        Controls.Add(menuStrip);

        MainMenuStrip = menuStrip;

        // ── Initial tab ──
        NewTab();

        // ── Timers ──
        syntaxTimer = new System.Windows.Forms.Timer();
        syntaxTimer.Interval = 300;
        syntaxTimer.Tick += SyntaxTimer_Tick;

        scrollSyncTimer = new System.Windows.Forms.Timer();
        scrollSyncTimer.Interval = 50;
        scrollSyncTimer.Tick += ScrollSyncTimer_Tick;
        scrollSyncTimer.Start();

        Load += MainForm_Load;
        Resize += (s, e) => SyncLineNumbers();
        FormClosing += MainForm_FormClosing;
    }

    // ── MENU CREATION ──
    private void CreateMenus()
    {
        // File
        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("Mới (Ctrl+N)", null, (s, e) => NewTab());
        fileMenu.DropDownItems.Add("Mở (Ctrl+O)", null, (s, e) => OpenFile());
        fileMenu.DropDownItems.Add("Lưu (Ctrl+S)", null, (s, e) => SaveFile());
        fileMenu.DropDownItems.Add("Lưu thành... (Ctrl+Shift+S)", null, (s, e) => SaveFileAs());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Thoát", null, (s, e) => Close());

        // Edit
        var editMenu = new ToolStripMenuItem("Edit");
        editMenu.DropDownItems.Add("Tìm (Ctrl+F)", null, (s, e) => ShowFindDialog());
        editMenu.DropDownItems.Add("Thay thế (Ctrl+H)", null, (s, e) => ShowReplaceDialog());
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add("Chọn tất cả (Ctrl+A)", null, (s, e) => { GetCurrentEditor()?.SelectAll(); });

        // View
        var viewMenu = new ToolStripMenuItem("View");
        viewMenu.DropDownItems.Add("Phóng to (Ctrl+Plus)", null, (s, e) => ZoomIn());
        viewMenu.DropDownItems.Add("Thu nhỏ (Ctrl+Minus)", null, (s, e) => ZoomOut());
        viewMenu.DropDownItems.Add("Đặt lại zoom (Ctrl+0)", null, (s, e) => ResetZoom());

        // Run
        var runMenu = new ToolStripMenuItem("Run");
        runMenu.DropDownItems.Add("Chạy (F5)", null, (s, e) => RunCode());
        runMenu.DropDownItems.Add("Xóa output", null, (s, e) => { output.Clear(); Status("Đã xóa output"); });

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, runMenu });

        // Shortcuts
        fileMenu.ShortcutKeyDisplayString = "";
        var shortcuts = new Dictionary<Keys, Action>
        {
            { Keys.Control | Keys.N, () => NewTab() },
            { Keys.Control | Keys.O, () => OpenFile() },
            { Keys.Control | Keys.S, () => SaveFile() },
            { Keys.Control | Keys.Shift | Keys.S, () => SaveFileAs() },
            { Keys.Control | Keys.F, () => ShowFindDialog() },
            { Keys.Control | Keys.H, () => ShowReplaceDialog() },
            { Keys.Control | Keys.A, () => GetCurrentEditor()?.SelectAll() },
            { Keys.Control | Keys.Oemplus, () => ZoomIn() },
            { Keys.Control | Keys.OemMinus, () => ZoomOut() },
            { Keys.Control | Keys.D0, () => ResetZoom() },
            { Keys.F5, () => RunCode() },
            { Keys.Control | Keys.T, () => NewTab() },
            { Keys.Control | Keys.W, () => CloseCurrentTab() },
        };
        KeyPreview = true;
        KeyDown += (s, e) =>
        {
            if (shortcuts.TryGetValue(e.KeyCode | (e.Control ? Keys.Control : 0) | (e.Shift ? Keys.Shift : 0), out var action))
            {
                e.SuppressKeyPress = true;
                action();
            }
        };
    }

    private Button MakeButton(string text)
    {
        var btn = new Button();
        btn.Text = text;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Color.LightGray;
        btn.Size = new Size(90, 30);
        btn.Margin = new Padding(2);
        btn.Font = new Font("Segoe UI", 9);
        return btn;
    }

    // ── TAB MANAGEMENT ──
    private void NewTab()
    {
        var tabData = CreateTabData();
        tabData.FilePath = "";
        tabData.TabIndex = tabIndexCounter++;
        tabs.Add(tabData);

        var page = new TabPage("Chưa đặt");
        page.Controls.Add(tabData.Editor);
        tabControl.TabPages.Add(page);
        tabControl.SelectedTab = page;

        tabData.Editor.Focus();
        UpdateFileLabel();
        Status("Tab mới");
    }

    private TabData CreateTabData()
    {
        var container = new Panel();
        container.Dock = DockStyle.Fill;
        container.BackColor = Color.FromArgb(30, 30, 30);

        var rtb = new RichTextBox();
        rtb.Dock = DockStyle.Fill;
        rtb.Font = new Font(DefaultFontName, DefaultFontSize);
        rtb.BackColor = Color.FromArgb(30, 30, 30);
        rtb.ForeColor = Color.White;
        rtb.WordWrap = false;
        rtb.AcceptsTab = true;
        rtb.DetectUrls = false;
        rtb.ShortcutsEnabled = true;
        rtb.ScrollBars = RichTextBoxScrollBars.Both;
        rtb.TextChanged += Editor_TextChanged;
        rtb.VScroll += Editor_VScroll;
        rtb.KeyDown += (sender, e) =>
        {
            if (e.Control && e.KeyCode == Keys.S) { e.SuppressKeyPress = true; SaveFile(); }
            if (e.KeyCode == Keys.F5) { e.SuppressKeyPress = true; RunCode(); }
            if (e.Control && e.KeyCode == Keys.O) { e.SuppressKeyPress = true; OpenFile(); }
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                var rtbSender = (RichTextBox)sender;
                int pos = rtbSender.SelectionStart;
                string text = rtbSender.Text;
                string before = pos > 0 ? text.Substring(0, pos) : "";
                string after = pos < text.Length ? text.Substring(pos) : "";
                int lastNewLine = before.LastIndexOf('\n');
                string currentLine = lastNewLine >= 0 ? before.Substring(lastNewLine + 1) : before;
                string indent = "";
                foreach (char c in currentLine) { if (c == ' ' || c == '\t') indent += c; else break; }
                if (currentLine.TrimEnd().EndsWith(":")) indent += "    ";
                rtbSender.Text = before + "\n" + indent + after;
                rtbSender.SelectionStart = pos + 1 + indent.Length;
            }
        };
        rtb.MouseClick += (s, e) => SyncLineNumbers();

        var lineNumPanel = new Panel();
        lineNumPanel.Dock = DockStyle.Left;
        lineNumPanel.Width = 40;
        lineNumPanel.BackColor = Color.FromArgb(25, 25, 25);
        lineNumPanel.Paint += LineNumPanel_Paint;

        container.Controls.Add(rtb);
        container.Controls.Add(lineNumPanel);

        return new TabData
        {
            Editor = rtb,
            LineNumberPanel = lineNumPanel,
            FilePath = ""
        };
    }

    private void CloseTab(int index)
    {
        if (index < 0 || index >= tabs.Count) return;

        var tab = tabs[index];
        tab.Editor.Dispose();
        tab.LineNumberPanel.Dispose();
        tabControl.TabPages.RemoveAt(index);
        tabs.RemoveAt(index);

        if (tabs.Count == 0)
        {
            scrollSyncTimer.Stop();
            NewTab();
            return;
        }

        int newIndex = Math.Min(index, tabs.Count - 1);
        tabControl.SelectedIndex = newIndex;
        UpdateFileLabel();
    }

    private void CloseCurrentTab()
    {
        if (tabControl.SelectedIndex >= 0)
            CloseTab(tabControl.SelectedIndex);
    }

    private TabData GetCurrentTab()
    {
        if (tabControl.SelectedIndex >= 0 && tabControl.SelectedIndex < tabs.Count)
            return tabs[tabControl.SelectedIndex];
        return null;
    }

    private RichTextBox GetCurrentEditor()
    {
        return GetCurrentTab()?.Editor;
    }

    private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateFileLabel();
        SyncLineNumbers();
        var editor = GetCurrentEditor();
        if (editor != null) editor.Focus();
    }

    private void TabControl_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Middle)
        {
            for (int i = 0; i < tabControl.TabCount; i++)
            {
                if (tabControl.GetTabRect(i).Contains(e.Location))
                {
                    CloseTab(i);
                    break;
                }
            }
        }
    }

    private void TabControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.T) { e.SuppressKeyPress = true; NewTab(); }
        if (e.Control && e.KeyCode == Keys.W) { e.SuppressKeyPress = true; CloseCurrentTab(); }
    }

    private void UpdateTabLayout()
    {
        foreach (var tab in tabs)
        {
            if (tab.Editor.Font.Size != DefaultFontSize * zoomLevel)
            {
                tab.Editor.Font = new Font(DefaultFontName, DefaultFontSize * zoomLevel);
                tab.Editor.Invalidate();
            }
        }
    }

    // ── FILE OPERATIONS ──
    private void OpenFile()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "VietLang (*.vl)|*.vl|All (*.*)|*.*";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var tab = GetCurrentTab();
            if (tab == null) NewTab();
            tab = GetCurrentTab();
            tab.FilePath = ofd.FileName;
            tab.Editor.Text = File.ReadAllText(tab.FilePath, Encoding.UTF8);
            int idx = tabs.IndexOf(tab);
            if (idx >= 0) tabControl.TabPages[idx].Text = Path.GetFileName(tab.FilePath);
            UpdateFileLabel();
            Status("Đã mở: " + tab.FilePath);
        }
    }

    private void SaveFile()
    {
        var tab = GetCurrentTab();
        if (tab == null) return;
        if (string.IsNullOrEmpty(tab.FilePath))
        {
            SaveFileAs();
            return;
        }
        File.WriteAllText(tab.FilePath, tab.Editor.Text, Encoding.UTF8);
        Status("Đã lưu: " + tab.FilePath);
    }

    private void SaveFileAs()
    {
        var tab = GetCurrentTab();
        if (tab == null) return;
        var sfd = new SaveFileDialog();
        sfd.Filter = "VietLang (*.vl)|*.vl|All (*.*)|*.*";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            tab.FilePath = sfd.FileName;
            File.WriteAllText(tab.FilePath, tab.Editor.Text, Encoding.UTF8);
            int idx = tabs.IndexOf(tab);
            if (idx >= 0) tabControl.TabPages[idx].Text = Path.GetFileName(tab.FilePath);
            UpdateFileLabel();
            Status("Đã lưu: " + tab.FilePath);
        }
    }

    // ── RUN ──
    private void RunCode()
    {
        var editor = GetCurrentEditor();
        if (editor == null || editor.TextLength == 0)
        {
            Status("Không có code để chạy!");
            return;
        }

        output.Clear();
        output.AppendText("Đang chạy...\n");
        Status("Đang chạy...");

        try
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "_vietlang_ide_temp.vl");
            File.WriteAllText(tempFile, editor.Text, new UTF8Encoding(false));

            // Find project root (where VietLang.csproj lives)
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

            var psi = new ProcessStartInfo();
            psi.FileName = "dotnet";
            psi.Arguments = $"run --project \"{Path.Combine(projectRoot, "VietLang")}\" -- \"{tempFile}\"";
            psi.WorkingDirectory = projectRoot;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;

            var proc = Process.Start(psi);
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(stdout))
                output.AppendText(stdout);
            if (!string.IsNullOrEmpty(stderr))
            {
                output.SelectionColor = Color.Red;
                output.AppendText(stderr);
                output.SelectionColor = Color.LightGreen;
            }

            output.AppendText($"\n── Exit code: {proc.ExitCode} ──\n");
            Status(proc.ExitCode == 0 ? "Chạy xong!" : $"Lỗi (exit {proc.ExitCode})");

            try { File.Delete(tempFile); } catch { }
        }
        catch (Exception ex)
        {
            output.SelectionColor = Color.Red;
            output.AppendText("Lỗi: " + ex.Message + "\n");
            output.SelectionColor = Color.LightGreen;
            Status("Có lỗi!");
        }
    }

    // ── STATUS ──
    private void Status(string text)
    {
        var tab = GetCurrentTab();
        int lineCount = tab?.Editor?.Lines?.Length ?? 0;
        string file = tab?.FilePath;
        string filePart = string.IsNullOrEmpty(file) ? "Chưa mở file" : Path.GetFileName(file);
        statusLabel.Text = $"  {text}  |  {filePart}  |  {lineCount} dòng";
    }

    private void UpdateFileLabel()
    {
        var tab = GetCurrentTab();
        if (tab == null) return;
        fileLabel.Text = string.IsNullOrEmpty(tab.FilePath) ? "  Chưa mở file" : "  " + tab.FilePath;
    }

    // ── LINE NUMBERS ──
    private void SyncLineNumbers()
    {
        var tab = GetCurrentTab();
        if (tab?.LineNumberPanel == null) return;
        tab.LineNumberPanel.Invalidate();
    }

    private void LineNumPanel_Paint(object sender, PaintEventArgs e)
    {
        var tab = GetCurrentTab();
        if (tab == null) return;
        var editor = tab.Editor;
        if (editor == null || editor.IsDisposed) return;
        if (sender != tab.LineNumberPanel) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.None;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

        var font = new Font("Consolas", editor.Font.Size);
        var bg = Color.FromArgb(25, 25, 25);
        g.Clear(bg);

        int firstLine = GetFirstVisibleLine(editor);
        int totalLines = editor.Lines.Length;
        int lineHeight = (int)g.MeasureString("W", font).Height + 1;
        int visibleLines = editor.ClientSize.Height / lineHeight + 1;

        var textBrush = new SolidBrush(Color.FromArgb(120, 120, 120));
        var format = new StringFormat();
        format.Alignment = StringAlignment.Far;

        for (int i = 0; i < visibleLines && (firstLine + i) < totalLines; i++)
        {
            int lineNum = firstLine + i + 1;
            float y = i * lineHeight + 2;
            g.DrawString(lineNum.ToString(), font, textBrush, tab.LineNumberPanel.Width - 4, y, format);
        }

        textBrush.Dispose();
        font.Dispose();
        format.Dispose();
    }

    private int GetFirstVisibleLine(RichTextBox rtb)
    {
        int firstCharIndex = rtb.GetCharIndexFromPosition(new Point(1, 1));
        return rtb.GetLineFromCharIndex(firstCharIndex);
    }

    private void Editor_VScroll(object sender, EventArgs e)
    {
        var tab = GetCurrentTab();
        if (tab?.LineNumberPanel == null) return;
        tab.LineNumberPanel.Invalidate();
    }

    // ── SYNTAX HIGHLIGHTING ──
    private void Editor_TextChanged(object sender, EventArgs e)
    {
        syntaxTimer.Stop();
        syntaxTimer.Start();
    }

    private void SyntaxTimer_Tick(object sender, EventArgs e)
    {
        syntaxTimer.Stop();
        var editor = GetCurrentEditor();
        if (editor == null || editor.TextLength == 0) return;
        ApplySyntaxHighlight(editor);
    }

    private void ApplySyntaxHighlight(RichTextBox rtb)
    {
        int savedSelStart = rtb.SelectionStart;
        int savedSelLen = rtb.SelectionLength;
        int savedScroll = Win32API.GetScrollPos(rtb.Handle, Win32API.SB_VERT);

        rtb.SelectAll();
        rtb.SelectionColor = rtb.ForeColor;
        rtb.SelectionStart = 0;
        rtb.SelectionLength = 0;

        string text = rtb.Text;

        var keywords = new HashSet<string> {
            "hàm", "lớp", "nếu", "còn_nếu", "không_thì", "trong_lúc",
            "với", "trả_về", "dừng", "tiếp", "và", "hoặc", "không",
            "đúng", "sai", "rỗng", "thử", "ngoại_lệ", "cuối_cùng",
            "ném", "khai_báo"
        };

        var builtins = new HashSet<string> {
            "tìm", "thay", "cắt", "chứa", "phân_tách", "lọc",
            "gộp", "chuẩn_hóa"
        };

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '#')
            {
                int end = text.IndexOf('\n', i);
                if (end == -1) end = text.Length;
                rtb.SelectionStart = i;
                rtb.SelectionLength = end - i;
                rtb.SelectionColor = Color.Green;
                i = end;
                continue;
            }

            if (text[i] == '"' || text[i] == '\'')
            {
                char quote = text[i];
                int start = i;
                i++;
                while (i < text.Length && text[i] != quote)
                {
                    if (text[i] == '\\') i++;
                    i++;
                }
                if (i < text.Length) i++;
                rtb.SelectionStart = start;
                rtb.SelectionLength = i - start;
                rtb.SelectionColor = Color.Orange;
                continue;
            }

            if (char.IsDigit(text[i]))
            {
                int start = i;
                while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.')) i++;
                rtb.SelectionStart = start;
                rtb.SelectionLength = i - start;
                rtb.SelectionColor = Color.Yellow;
                continue;
            }

            if (char.IsLetter(text[i]) || text[i] == '_')
            {
                int start = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                string word = text.Substring(start, i - start);

                Color? color = null;
                if (keywords.Contains(word)) color = Color.Cyan;
                else if (builtins.Contains(word)) color = Color.Magenta;

                if (color.HasValue)
                {
                    rtb.SelectionStart = start;
                    rtb.SelectionLength = i - start;
                    rtb.SelectionColor = color.Value;
                }
                continue;
            }

            i++;
        }

        rtb.SelectionStart = savedSelStart;
        rtb.SelectionLength = savedSelLen;
        Win32API.SetScrollPos(rtb.Handle, Win32API.SB_VERT, savedScroll, true);
    }

    // ── FONT SIZE / ZOOM ──
    private void ZoomIn()
    {
        if (zoomLevel < 4.0f)
        {
            zoomLevel += 0.1f;
            UpdateFontSize();
        }
    }

    private void ZoomOut()
    {
        if (zoomLevel > 0.1f)
        {
            zoomLevel -= 0.1f;
            UpdateFontSize();
        }
    }

    private void ResetZoom()
    {
        zoomLevel = 1.0f;
        UpdateFontSize();
    }

    private void UpdateFontSize()
    {
        UpdateTabLayout();
        zoomLabel.Text = $"{(int)(zoomLevel * 100)}%";
        SyncLineNumbers();
    }

    // ── FIND / REPLACE ──
    private void ShowFindDialog()
    {
        var editor = GetCurrentEditor();
        if (editor == null) return;
        var dlg = new FindReplaceDialog(editor, false, this);
        dlg.Show(this);
    }

    private void ShowReplaceDialog()
    {
        var editor = GetCurrentEditor();
        if (editor == null) return;
        var dlg = new FindReplaceDialog(editor, true, this);
        dlg.Show(this);
    }

    // ── EVENTS ──
    private void MainForm_Load(object sender, EventArgs e)
    {
        SyncLineNumbers();
        var editor = GetCurrentEditor();
        if (editor != null) editor.Focus();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        syntaxTimer?.Dispose();
        scrollSyncTimer?.Dispose();
    }

    private void ScrollSyncTimer_Tick(object sender, EventArgs e)
    {
        SyncLineNumbers();
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

// ── FIND / REPLACE DIALOG ──
public class FindReplaceDialog : Form
{
    private TextBox txtFind;
    private TextBox txtReplace;
    private CheckBox chkMatchCase;
    private Button btnFindNext;
    private Button btnFindPrev;
    private Button btnReplace;
    private Button btnReplaceAll;
    private Button btnClose;
    private RichTextBox targetEditor;
    private MainForm mainForm;

    public FindReplaceDialog(RichTextBox editor, bool showReplace, MainForm form)
    {
        targetEditor = editor;
        mainForm = form;

        Text = showReplace ? "Thay thế" : "Tìm kiếm";
        Size = new Size(400, showReplace ? 200 : 160);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        BackColor = Color.FromArgb(40, 40, 40);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9);

        var findLabel = new Label { Text = "Tìm:", Left = 10, Top = 14, Width = 50, ForeColor = Color.White };
        txtFind = new TextBox { Left = 65, Top = 11, Width = 210, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };

        btnFindNext = new Button { Text = "Tìm tiếp", Left = 285, Top = 9, Width = 85 };
        btnFindNext.Click += (s, e) => FindNext();
        btnFindPrev = new Button { Text = "Tìm trước", Left = 285, Top = 40, Width = 85 };
        btnFindPrev.Click += (s, e) => FindPrevious();

        chkMatchCase = new CheckBox { Text = "Phân biệt HOA/thường", Left = 65, Top = 85, Width = 180, ForeColor = Color.LightGray };

        Controls.AddRange(new Control[] { findLabel, txtFind, btnFindNext, btnFindPrev, chkMatchCase });

        if (showReplace)
        {
            var replaceLabel = new Label { Text = "Thay:", Left = 10, Top = 47, Width = 50, ForeColor = Color.White };
            txtReplace = new TextBox { Left = 65, Top = 44, Width = 210, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };

            btnReplace = new Button { Text = "Thay", Left = 285, Top = 71, Width = 85 };
            btnReplace.Click += (s, e) => Replace();
            btnReplaceAll = new Button { Text = "Thay tất cả", Left = 285, Top = 102, Width = 85 };
            btnReplaceAll.Click += (s, e) => ReplaceAll();

            Controls.AddRange(new Control[] { replaceLabel, txtReplace, btnReplace, btnReplaceAll });
        }

        btnClose = new Button { Text = "Đóng", Left = showReplace ? 285 : 285, Top = showReplace ? 130 : 110, Width = 85 };
        btnClose.Click += (s, e) => Close();
        Controls.Add(btnClose);

        AcceptButton = btnFindNext;
        CancelButton = btnClose;
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) FindNext();
            if (e.KeyCode == Keys.Escape) Close();
        };
    }

    private void FindNext()
    {
        string search = txtFind?.Text;
        if (string.IsNullOrEmpty(search)) return;

        bool matchCase = chkMatchCase?.Checked ?? false;
        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

        int start = targetEditor.SelectionStart + targetEditor.SelectionLength;
        var match = Regex.Match(targetEditor.Text, Regex.Escape(search), options, TimeSpan.FromMilliseconds(500));

        if (start > 0 && start <= targetEditor.Text.Length)
        {
            string after = targetEditor.Text.Substring(start);
            var matchAfter = Regex.Match(after, Regex.Escape(search), options, TimeSpan.FromMilliseconds(500));
            if (matchAfter.Success)
            {
                targetEditor.Select(start + matchAfter.Index, matchAfter.Length);
                targetEditor.ScrollToCaret();
                return;
            }
        }

        if (match.Success)
        {
            targetEditor.Select(match.Index, match.Length);
            targetEditor.ScrollToCaret();
        }
    }

    private void FindPrevious()
    {
        string search = txtFind?.Text;
        if (string.IsNullOrEmpty(search)) return;

        bool matchCase = chkMatchCase?.Checked ?? false;
        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

        int end = targetEditor.SelectionStart;
        if (end > 0)
        {
            string before = targetEditor.Text.Substring(0, end);
            var matches = Regex.Matches(before, Regex.Escape(search), options, TimeSpan.FromMilliseconds(500));
            if (matches.Count > 0)
            {
                var last = matches[matches.Count - 1];
                targetEditor.Select(last.Index, last.Length);
                targetEditor.ScrollToCaret();
            }
        }
    }

    private void Replace()
    {
        string search = txtFind?.Text;
        string replace = txtReplace?.Text ?? "";
        if (string.IsNullOrEmpty(search)) return;

        bool matchCase = chkMatchCase?.Checked ?? false;
        var cmp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (targetEditor.SelectedText.Equals(search, cmp))
        {
            targetEditor.SelectedText = replace;
        }
        FindNext();
    }

    private void ReplaceAll()
    {
        string search = txtFind?.Text;
        string replace = txtReplace?.Text ?? "";
        if (string.IsNullOrEmpty(search)) return;

        bool matchCase = chkMatchCase?.Checked ?? false;
        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

        string text = targetEditor.Text;
        string result = Regex.Replace(text, Regex.Escape(search), replace, options, TimeSpan.FromMilliseconds(500));
        int count = Regex.Matches(text, Regex.Escape(search), options, TimeSpan.FromMilliseconds(500)).Count;

        if (count > 0)
        {
            int savedSelStart = targetEditor.SelectionStart;
            targetEditor.Text = result;
            targetEditor.SelectionStart = 0;
            targetEditor.SelectionLength = 0;
            Status($"Đã thay thế {count} kết quả");
        }
    }

    private void Status(string text)
    {
        mainForm?.GetType().GetMethod("Status",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(mainForm, new object[] { text });
    }
}

// ── WIN32 API ──
internal static class Win32API
{
    public const int SB_VERT = 1;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int GetScrollPos(IntPtr hWnd, int nBar);
}
