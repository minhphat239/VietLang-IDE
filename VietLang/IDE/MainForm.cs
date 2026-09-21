// VietLangIDE.cs — VietLang IDE (WinForms)
// Chạy: dotnet run --project VietLang

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace VietLangIDE;

public class MainForm : Form
{
    private RichTextBox editor;
    private RichTextBox output;
    private Label statusLabel;
    private Label fileLabel;
    private string currentFile = "";

    public MainForm()
    {
        // ── Form ──
        Text = "VietLang IDE";
        Size = new Size(950, 750);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Application;

        // ── Toolbar ──
        var toolbar = new FlowLayoutPanel();
        toolbar.Dock = DockStyle.Top;
        toolbar.Height = 42;
        toolbar.BackColor = Color.FromArgb(240, 240, 240);
        toolbar.Padding = new Padding(4, 4, 4, 4);

        var btnOpen = MakeButton("📂 Mở");
        var btnSave = MakeButton("💾 Lưu");
        var btnSaveAs = MakeButton("📝 Lưu thành");
        var btnRun = MakeButton("▶ Chạy");
        var btnClear = MakeButton("🗑 Xóa");

        btnOpen.Click += (s, e) => OpenFile();
        btnSave.Click += (s, e) => SaveFile();
        btnSaveAs.Click += (s, e) => SaveFileAs();
        btnRun.Click += (s, e) => RunCode();
        btnClear.Click += (s, e) => { output.Clear(); Status("Đã xóa output"); };

        toolbar.Controls.AddRange(new Control[] { btnOpen, btnSave, btnSaveAs, btnRun, btnClear });

        // ── File label ──
        fileLabel = new Label();
        fileLabel.Dock = DockStyle.Top;
        fileLabel.Height = 22;
        fileLabel.BackColor = Color.WhiteSmoke;
        fileLabel.Font = new Font("Segoe UI", 9);
        fileLabel.Text = "  Chưa mở file";
        fileLabel.Padding = new Padding(4, 2, 0, 0);

        // ── Editor ──
        editor = new RichTextBox();
        editor.Dock = DockStyle.Fill;
        editor.Font = new Font("Consolas", 11);
        editor.BackColor = Color.FromArgb(30, 30, 30);
        editor.ForeColor = Color.White;
        editor.WordWrap = false;
        editor.AcceptsTab = true;
        editor.DetectUrls = false;
        editor.ShortcutsEnabled = true;
        editor.KeyDown += Editor_KeyDown;

        // ── Output ──
        output = new RichTextBox();
        output.Dock = DockStyle.Bottom;
        output.Height = 180;
        output.Font = new Font("Consolas", 10);
        output.BackColor = Color.FromArgb(15, 15, 15);
        output.ForeColor = Color.LightGreen;
        output.ReadOnly = true;
        output.WordWrap = true;

        // ── Status ──
        statusLabel = new Label();
        statusLabel.Dock = DockStyle.Bottom;
        statusLabel.Height = 22;
        statusLabel.BackColor = Color.WhiteSmoke;
        statusLabel.Font = new Font("Segoe UI", 9);
        statusLabel.Text = "  Sẵn sàng";
        statusLabel.Padding = new Padding(4, 2, 0, 0);

        // ── Splitter ──
        var splitter = new Splitter();
        splitter.Dock = DockStyle.Bottom;
        splitter.Height = 4;
        splitter.MinSize = 80;

        // ── Add controls ──
        Controls.Add(editor);
        Controls.Add(splitter);
        Controls.Add(output);
        Controls.Add(statusLabel);
        Controls.Add(fileLabel);
        Controls.Add(toolbar);
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

    private void Editor_KeyDown(object sender, KeyEventArgs e)
    {
        // Ctrl+S = Save
        if (e.Control && e.KeyCode == Keys.S)
        {
            e.SuppressKeyPress = true;
            SaveFile();
        }
        // F5 = Run
        if (e.KeyCode == Keys.F5)
        {
            e.SuppressKeyPress = true;
            RunCode();
        }
        // Ctrl+O = Open
        if (e.Control && e.KeyCode == Keys.O)
        {
            e.SuppressKeyPress = true;
            OpenFile();
        }
    }

    private void OpenFile()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "VietLang (*.vl)|*.vl|All (*.*)|*.*";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            currentFile = ofd.FileName;
            editor.Text = File.ReadAllText(currentFile, Encoding.UTF8);
            fileLabel.Text = "  " + currentFile;
            Status("Đã mở: " + currentFile);
        }
    }

    private void SaveFile()
    {
        if (string.IsNullOrEmpty(currentFile))
        {
            SaveFileAs();
            return;
        }
        File.WriteAllText(currentFile, editor.Text, Encoding.UTF8);
        Status("Đã lưu: " + currentFile);
    }

    private void SaveFileAs()
    {
        var sfd = new SaveFileDialog();
        sfd.Filter = "VietLang (*.vl)|*.vl|All (*.*)|*.*";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            currentFile = sfd.FileName;
            File.WriteAllText(currentFile, editor.Text, Encoding.UTF8);
            fileLabel.Text = "  " + currentFile;
            Status("Đã lưu: " + currentFile);
        }
    }

    private void RunCode()
    {
        if (editor.TextLength == 0)
        {
            Status("Không có code để chạy!");
            return;
        }

        output.Clear();
        output.AppendText("Đang chạy...\n");
        Status("Đang chạy...");

        try
        {
            // Write code to temp file
            var tempFile = Path.Combine(Path.GetTempPath(), "_vietlang_ide_temp.vl");
            File.WriteAllText(tempFile, editor.Text, Encoding.UTF8);

            // Find vietlang.cmd
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var vietlangCmd = Path.Combine(exeDir, "..", "..", "..", "..", "vietlang.cmd");
            if (!File.Exists(vietlangCmd))
                vietlangCmd = Path.Combine(exeDir, "vietlang.cmd");

            // Run via Process
            var psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = $"/c \"{vietlangCmd}\" \"{tempFile}\"";
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

            // Cleanup
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

    private void Status(string text)
    {
        statusLabel.Text = "  " + text;
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
