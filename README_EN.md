# 🐟 Moyu Popup

> Disguise web videos / novels as a legit-looking "Windows system popup".
> When your boss walks by, you're just looking at a system warning. That's all.

A desktop utility written in C# (WinForms + WebView2). Double-click the exe to open the config panel, hit "Start Moyu", and a window that looks exactly like a Windows system popup appears at the bottom-right corner of your screen — except it's actually playing a Bilibili video or showing a web novel.

---

## ✨ Features

- **10 system-style skins**: Windows Security Center, System Update, Disk Space Low, Lottery Popup, Antivirus Scan, UAC, Network Error, Classic XP, Edge Security Block, Scheduled Task
- **Video / Web novel dual mode**: Bilibili BV IDs, YouTube, direct mp4 links, local video files; leave the URL empty to auto-open the built-in biquge (web novel) site
- **Fits perfectly to screen corners**: bottom-right / bottom-left / top-right / top-left / center, 0-pixel margin, draggable
- **3 size presets**: Small / Medium / Large
- **Adjustable opacity**: 50% ~ 100%
- **Always on top**: the popup stays above every other window
- **Customizable hotkeys**: emergency hide (default Esc, shrinks to a desktop green dot), real quit (default Ctrl+Q)
- **Camouflage details**: fake progress bars, fake log scrolling, fake scan lines
- **Single-file installer**: 0.97MB, one exe bundles everything, double-click to install

## 🚀 Quick Start

1. Download the installer `moyu-popup-setup.exe` (see Releases)
2. Run it → pick an install directory → click "Install"
3. Open the "Moyu Popup" shortcut on your desktop
4. Leave the URL empty and hit "Start Moyu" → a popup window starts playing

> You can also run `摸鱼弹窗.exe` directly from the project directory (it depends on the sibling `fish.html`, `player.html` and `webview2\` folder).

## 🎮 Usage

| Setting | Description |
| --- | --- |
| Content type | Video / Web novel; in novel mode, an empty URL opens the built-in biquge site |
| URL | Bilibili BV ID, YouTube link, direct mp4 link, or a novel webpage URL |
| Local file | Pick a local video file to play |
| Skin | 10 system popup styles (see above) |
| Position / Size | 5 positions × 3 sizes |
| Opacity | Slider, 50%~100% |
| Always on top | Keep the popup above all windows when enabled |
| Emergency hide key | Click the button and press a new combo to rebind; default Esc |
| Quit combo | Default Ctrl+Q |

## 🛠 How It Works

- **Main program**: C# (WinForms) + WebView2, compiled as a single exe targeting .NET Framework 4.0
- **Config panel**: `fish.html` hosted in the WebView2 main window; clicking "Start Moyu" notifies the host C# process via `window.chrome.webview.postMessage`
- **The popup**: C# creates a borderless `Form` (0-pixel edge margin, rounded Region, Opacity, TopMost), and WebView2 loads `player.html` to play the video or embed the novel page
- **Height auto-fit**: the page reports its actual content height via postMessage, and the host resizes the form dynamically so there is no blank space
- **DPI awareness**: Per-Monitor V2 awareness declared, so text stays crisp on high-DPI screens
- **Installer**: embeds the main program / pages / WebView2 runtime as resources, extracts them at install time and creates a desktop shortcut

## 📁 Project Structure

```
摸鱼弹窗/
├── 摸鱼弹窗.cs          # Main program: main window + popup + WebView2 host
├── fish.html            # Config panel UI
├── player.html          # Popup playback page (video / novel / skins)
├── Installer.cs         # Installer source (embedded resources + shortcut)
├── webview2/            # WebView2 managed runtime
└── 摸鱼弹窗.exe         # Build output
```

## ⚠️ Disclaimer

This project is for learning and entertainment only. Do not use it to deceive others or for any inappropriate purposes. Users are solely responsible for their own use.

## 📄 License

[MIT](LICENSE)
