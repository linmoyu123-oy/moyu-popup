using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public static class App {
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool SetDllDirectory(string lpPathName);

    [DllImport("user32.dll")]
    static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")]
    static extern bool SetProcessDPIAware();

    static string BaseDir;
    static Form MainForm;

    [STAThread]
    public static void Main(string[] args) {
        // 声明 Per-Monitor V2 DPI 感知，否则高 DPI 下界面文字被位图拉伸导致模糊
        try { SetProcessDpiAwarenessContext((IntPtr)(-4)); } catch { }
        if (!SetProcessDpiAwarenessContext((IntPtr)(-4))) { try { SetProcessDPIAware(); } catch { } }

        BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        string wvDir = Path.Combine(BaseDir, "webview2");
        // 托管 DLL 在 webview2\ 子目录，注册程序集解析器（native 的 WebView2Loader 由 SetDllDirectory 处理）
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
            try {
                string name = new System.Reflection.AssemblyName(e.Name).Name + ".dll";
                string p = Path.Combine(wvDir, name);
                return File.Exists(p) ? System.Reflection.Assembly.LoadFrom(p) : null;
            } catch { return null; }
        };
        if (Directory.Exists(wvDir)) SetDllDirectory(wvDir);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        string page = "fish.html";
        if (args.Length > 0 && !string.IsNullOrEmpty(args[0])) page = args[0];
        BuildMainWindow(page);
    }

    // ================= 主窗口（配置台） =================
    static void BuildMainWindow(string pageName) {
        MainForm = new Form();
        MainForm.Text = "\u6478\u9c7c\u5f39\u7a97";
        MainForm.FormBorderStyle = FormBorderStyle.Sizable;
        MainForm.StartPosition = FormStartPosition.CenterScreen;
        MainForm.Size = new Size(940, 780);
        MainForm.MinimumSize = new Size(620, 480);
        MainForm.ShowInTaskbar = true;

        var wv = new WebView2();
        wv.Dock = DockStyle.Fill;
        MainForm.Controls.Add(wv);

        string pagePath = Path.Combine(BaseDir, pageName);
        string fullUrl = new Uri(pagePath).AbsoluteUri;

        wv.CoreWebView2InitializationCompleted += (s, e) => {
            if (e.IsSuccess) {
                wv.CoreWebView2.WebMessageReceived += OnMainMessage;
                wv.CoreWebView2.Navigate(fullUrl);
            } else {
                try { File.WriteAllText(Path.Combine(BaseDir, "_main_err.log"),
                    "Init failed: " + (e.InitializationException != null ? e.InitializationException.ToString() : "unknown")); } catch { }
            }
        };

        MainForm.Load += (s, e) => {
            try { InitWebView(wv, "wv2main-"); }
            catch (Exception ex) { try { File.WriteAllText(Path.Combine(BaseDir, "_main_err.log"), ex.ToString()); } catch { } }
        };

        Application.Run(MainForm);
    }

    // ================= 主窗口收到配置台的"开始摸鱼"消息 =================
    static void OnMainMessage(object s, CoreWebView2WebMessageReceivedEventArgs e) {
        string m = e.WebMessageAsJson;
        try {
            if (m != null && m.Contains("\"action\":\"open\"")) {
                string url = GetStr(m, "url");
                string skin = GetStr(m, "skin");
                string pos = GetStr(m, "pos");
                string size = GetStr(m, "size");
                string type = GetStr(m, "type");
                double opacity = GetNum(m, "opacity");
                int topmost = (int)GetNum(m, "topmost");
                if (string.IsNullOrEmpty(type)) type = "video";
                MainForm.BeginInvoke((Action)(() => OpenPopup(url, skin, pos, size, opacity, type, topmost)));
            }
        } catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.ToString()); }
    }

    static string GetStr(string json, string key) {
        var mm = Regex.Match(json, "\"" + key + "\":\"([^\"]*)\"");
        return mm.Success ? mm.Groups[1].Value : "";
    }
    static double GetNum(string json, string key) {
        var mm = Regex.Match(json, "\"" + key + "\":([0-9.]+)");
        return mm.Success ? double.Parse(mm.Groups[1].Value) : 1.0;
    }

    // ================= 弹窗（伪装广告小窗） =================
    static void OpenPopup(string url, string skin, string pos, string size, double opacity, string type, int topmost) {
        if (string.IsNullOrEmpty(url)) { return; }
        try {
        int W = 400, H = 580;
        if (size == "s") { W = 320; H = 460; }
        if (size == "l") { W = 480; H = 700; }
        var scr = Screen.PrimaryScreen.WorkingArea;
        int x = 0, y = 0;
        switch (pos) {
            case "bl": x = 0; y = Math.Max(0, scr.Bottom - H); break;
            case "tr": x = Math.Max(0, scr.Right - W); y = 0; break;
            case "tl": x = 0; y = 0; break;
            case "center": x = Math.Max(0, (scr.Right - W) / 2); y = Math.Max(0, (scr.Bottom - H) / 2); break;
            default: x = Math.Max(0, scr.Right - W); y = Math.Max(0, scr.Bottom - H); break;
        }

        var form = new Form();
        form.FormBorderStyle = FormBorderStyle.None;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(x, y);
        form.Size = new Size(W, H);
        if (opacity < 0.3) opacity = 0.3;
        if (opacity > 1.0) opacity = 1.0;
        form.Opacity = opacity;
        form.TopMost = topmost != 0;
        form.ShowInTaskbar = false;
        form.Text = "";

        var gp = new System.Drawing.Drawing2D.GraphicsPath();
        int d = 18;
        gp.AddArc(0, 0, d, d, 180, 90);
        gp.AddArc(W - d, 0, d, d, 270, 90);
        gp.AddArc(W - d, H - d, d, d, 0, 90);
        gp.AddArc(0, H - d, d, d, 90, 90);
        gp.CloseFigure();
        form.Region = new Region(gp);

        var wv = new WebView2();
        wv.Dock = DockStyle.Fill;
        form.Controls.Add(wv);

        string playerPath = Path.Combine(BaseDir, "player.html");
        string fullUrl = new Uri(playerPath).AbsoluteUri
            + "?url=" + Uri.EscapeDataString(url)
            + "&skin=" + Uri.EscapeDataString(skin)
            + "&size=" + Uri.EscapeDataString(size)
            + "&type=" + Uri.EscapeDataString(type);

        wv.CoreWebView2InitializationCompleted += (s, e) => {
            if (e.IsSuccess) {
                wv.CoreWebView2.WebMessageReceived += (s3, e3) => {
                    try {
                        string m = e3.WebMessageAsJson;
                        if (m != null && m.Contains("resize")) {
                            var mh = Regex.Match(m, "\"h\":(\\d+)");
                            if (mh.Success) {
                                int nh = int.Parse(mh.Groups[1].Value);
                                if (nh >= 300 && nh <= 900) {
                                    int fx2 = nh;
                                    string fpos = pos;
                                    form.BeginInvoke((Action)(() => {
                                        form.Height = fx2;
                                        if (fpos == "br" || fpos == "bl") {
                                            form.Location = new Point(form.Location.X, Math.Max(0, Screen.PrimaryScreen.WorkingArea.Bottom - fx2));
                                        }
                                    }));
                                }
                            }
                        }
                        if (m != null && m.Contains("move")) {
                            int dx = 0, dy = 0;
                            var mx = Regex.Match(m, "\"dx\":(-?\\d+)");
                            var my = Regex.Match(m, "\"dy\":(-?\\d+)");
                            if (mx.Success) dx = int.Parse(mx.Groups[1].Value);
                            if (my.Success) dy = int.Parse(my.Groups[1].Value);
                            int fx = dx, fy = dy;
                            form.BeginInvoke((Action)(() => form.Location = new Point(form.Location.X + fx, form.Location.Y + fy)));
                        }
                    } catch { }
                };
                wv.CoreWebView2.Navigate(fullUrl);
                wv.CoreWebView2.WindowCloseRequested += (s2, e2) => form.Close();
            } else {
                try { File.WriteAllText(Path.Combine(BaseDir, "_init_err.log"), "Init failed: " + (e.InitializationException != null ? e.InitializationException.ToString() : "unknown")); } catch { }
            }
        };

        form.Load += (s, e) => {
            try { InitWebView(wv, "wv2popup-"); }
            catch (Exception ex) { try { File.WriteAllText(Path.Combine(BaseDir, "_init_err.log"), ex.ToString()); } catch { } }
        };

        form.Show();
        } catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.ToString()); }
    }

    // ================= WebView2 初始化（每实例独立用户目录） =================
    static void InitWebView(WebView2 wv, string prefix) {
        string userDataFolder = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N").Substring(0, 12));
        var envTask = CoreWebView2Environment.CreateAsync(null, userDataFolder);
        var env = envTask.Result;
        wv.EnsureCoreWebView2Async(env);
    }
}
