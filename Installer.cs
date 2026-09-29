using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

public static class Installer {
    // 内置资源名 -> 释放路径
    static readonly string[][] FILES = new string[][] {
        new[] { "app_exe",     "摸鱼弹窗.exe" },
        new[] { "fish_html",   "fish.html" },
        new[] { "player_html", "player.html" },
        new[] { "wv2_core",    @"webview2\Microsoft.Web.WebView2.Core.dll" },
        new[] { "wv2_winforms",@"webview2\Microsoft.Web.WebView2.WinForms.dll" },
        new[] { "wv2_loader",  @"webview2\WebView2Loader.dll" }
    };

    [STAThread]
    public static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new InstallForm());
    }

    class InstallForm : Form {
        TextBox dirBox;
        Label status;
        Button installBtn;

        public InstallForm() {
            Text = "摸鱼弹窗 · 安装向导";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(470, 210);
            Font = new Font("Microsoft YaHei", 9F);

            var l1 = new Label { Text = "安装到目录：", Location = new Point(22, 30), AutoSize = true };
            dirBox = new TextBox { Text = "F:\\摸鱼弹窗", Location = new Point(118, 27), Width = 232 };
            var browse = new Button { Text = "浏览...", Location = new Point(358, 26), Width = 90 };
            browse.Click += (s, e) => {
                using (var f = new FolderBrowserDialog()) {
                    f.Description = "选择安装目录";
                    if (Directory.Exists(dirBox.Text)) f.SelectedPath = dirBox.Text;
                    if (f.ShowDialog() == DialogResult.OK) dirBox.Text = f.SelectedPath;
                }
            };
            installBtn = new Button { Text = "开始安装", Location = new Point(118, 74), Width = 110, Height = 32 };
            installBtn.Click += DoInstall;
            status = new Label { Text = "", Location = new Point(22, 118), AutoSize = true, ForeColor = Color.DimGray };
            var tip = new Label { Text = "将安装运行文件并创建桌面快捷方式「摸鱼弹窗」。", Location = new Point(22, 160), AutoSize = true, ForeColor = Color.Gray };

            Controls.Add(l1); Controls.Add(dirBox); Controls.Add(browse);
            Controls.Add(installBtn); Controls.Add(status); Controls.Add(tip);
        }

        void DoInstall(object s, EventArgs e) {
            installBtn.Enabled = false;
            status.ForeColor = Color.DimGray;
            try {
                string dir = Path.GetFullPath(dirBox.Text.Trim());
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(Path.Combine(dir, "webview2"));
                foreach (var f in FILES) {
                    string outPath = Path.Combine(dir, f[1]);
                    using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(f[0])) {
                        if (rs == null) throw new Exception("安装包缺少内置文件: " + f[0]);
                        using (var os = File.Create(outPath)) rs.CopyTo(os);
                    }
                    status.Text = "已释放 " + Path.GetFileName(f[1]);
                    Application.DoEvents();
                }
                // 桌面快捷方式
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string lnk = Path.Combine(desktop, "摸鱼弹窗.lnk");
                try {
                    Type t = Type.GetTypeFromProgID("WScript.Shell");
                    if (t != null) {
                        dynamic shell = Activator.CreateInstance(t);
                        dynamic sc = shell.CreateShortcut(lnk);
                        sc.TargetPath = Path.Combine(dir, "摸鱼弹窗.exe");
                        sc.WorkingDirectory = dir;
                        sc.Save();
                    }
                } catch { }
                status.Text = "安装完成，已创建桌面快捷方式。";
                MessageBox.Show("安装完成！\n\n已创建桌面快捷方式「摸鱼弹窗」。", "安装成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                try { System.Diagnostics.Process.Start(Path.Combine(dir, "摸鱼弹窗.exe")); } catch { }
                Close();
            } catch (Exception ex) {
                status.Text = "安装失败: " + ex.Message;
                status.ForeColor = Color.Firebrick;
                installBtn.Enabled = true;
            }
        }
    }
}
