using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Forms = System.Windows.Forms;

namespace EmergentDesktop;

public partial class App : Application
{
    private const string MutexName = @"Local\EmergentDesktop_v03";
    private Mutex? _singleInstance;
    private Forms.NotifyIcon? _tray;
    private readonly List<Window> _widgets = [];
    private DeckSettings _settings = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _singleInstance = new Mutex(true, MutexName, out bool firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("EMERGENT DESKTOP is already running. Look for its tray icon.",
                "EMERGENT DESKTOP", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _settings = SettingsStore.Load();

        var header = new HeaderWindow();
        var telemetry = new TelemetryWindow();
        var launcher = new LauncherWindow();
        var clock = new ClockWindow();
        _widgets.AddRange([header, telemetry, launcher, clock]);

        var area = SystemParameters.WorkArea;
        header.Width = Math.Max(480, area.Width - 28);
        PutWindow(header, "header", area.Left + 14, area.Top + 12);
        PutWindow(telemetry, "telemetry", area.Left + 16, area.Top + 78);
        PutWindow(launcher, "launcher", area.Right - launcher.Width - 16, area.Top + 78);
        PutWindow(clock, "clock", area.Left + (area.Width - clock.Width) / 2,
            area.Bottom - clock.Height - 25);

        // This is a set of ordinary WPF desktop windows, not an Explorer shell hook.
        // They do not run above other applications and can be hidden at any time.
        foreach (Window window in _widgets) window.Show();

        CreateTray();
    }

    private void PutWindow(Window window, string id, double defaultLeft, double defaultTop)
    {
        if (_settings.Positions.TryGetValue(id, out WidgetPosition? p))
        {
            defaultLeft = p.Left;
            defaultTop = p.Top;
        }

        // Keep at least the panel header reachable if an external monitor disappears.
        var leftEdge = SystemParameters.VirtualScreenLeft;
        var topEdge = SystemParameters.VirtualScreenTop;
        var rightEdge = leftEdge + SystemParameters.VirtualScreenWidth;
        var bottomEdge = topEdge + SystemParameters.VirtualScreenHeight;
        window.Left = Math.Clamp(defaultLeft, leftEdge, Math.Max(leftEdge, rightEdge - 90));
        window.Top = Math.Clamp(defaultTop, topEdge, Math.Max(topEdge, bottomEdge - 45));
        window.Tag = id;
    }

    public void SavePosition(Window window)
    {
        if (window.Tag is not string id) return;
        _settings.Positions[id] = new WidgetPosition(window.Left, window.Top);
        SettingsStore.Save(_settings);
    }

    public IReadOnlyList<LauncherEntry> GetLaunchers() => _settings.Launchers;

    public void ReloadLaunchers()
    {
        _settings = SettingsStore.Load();
        foreach (Window w in _widgets)
            if (w is LauncherWindow launcher) launcher.Rebuild();
    }

    private void CreateTray()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Show widgets", null, (_, _) => Dispatcher.Invoke(ShowWidgets));
        menu.Items.Add("Hide widgets", null, (_, _) => Dispatcher.Invoke(HideWidgets));
        menu.Items.Add("Open settings folder", null, (_, _) => Dispatcher.Invoke(SettingsStore.OpenFolder));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit Emergent Desktop", null, (_, _) => Dispatcher.Invoke(ExitDeck));

        _tray = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "EMERGENT DESKTOP v0.3",
            Visible = true,
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => Dispatcher.Invoke(ShowWidgets);
    }

    public void ShowWidgets()
    {
        foreach (Window w in _widgets)
        {
            if (!w.IsVisible) w.Show();
            if (w.WindowState == WindowState.Minimized) w.WindowState = WindowState.Normal;
        }
    }

    public void HideWidgets()
    {
        foreach (Window w in _widgets) w.Hide();
    }

    public void ExitDeck()
    {
        foreach (Window w in _widgets) SavePosition(w);
        _tray?.Dispose();
        _tray = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
