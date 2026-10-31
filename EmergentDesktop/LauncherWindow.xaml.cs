using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EmergentDesktop;

public partial class LauncherWindow : WidgetWindow
{
    public LauncherWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => Rebuild();
    }

    public void Rebuild()
    {
        LauncherGroups.Children.Clear();
        var launchers = (Application.Current as App)?.GetLaunchers();
        if (launchers is null) return;

        // Preserve the configured order. Each heading appears exactly once.
        foreach (var group in launchers.GroupBy(x => x.Group ?? "OTHER"))
        {
            var heading = new TextBlock
            {
                Text = group.Key.ToUpperInvariant(),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10,
                Foreground = (Brush)FindResource("Amber"),
                Margin = new Thickness(0, 16, 0, 9)
            };
            LauncherGroups.Children.Add(heading);

            foreach (LauncherEntry item in group)
            {
                var button = new Button
                {
                    Content = "›    " + item.Label,
                    Style = (Style)FindResource("ActionButton"),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 7),
                    ToolTip = item.Target
                };
                button.Click += (_, _) => OpenShortcut(item);
                LauncherGroups.Children.Add(button);
            }
        }
    }

    private static void OpenShortcut(LauncherEntry entry)
    {
        try
        {
            // No command-shell interpretation: executable/URI and arguments are separate.
            Process.Start(new ProcessStartInfo
            {
                FileName = entry.Target,
                Arguments = entry.Arguments ?? string.Empty,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception
                                     or InvalidOperationException or System.IO.FileNotFoundException
                                     or UnauthorizedAccessException or System.Security.SecurityException)
        {
            MessageBox.Show($"Windows couldn't open '{entry.Label}'.\n\n{ex.Message}",
                "EMERGENT DESKTOP", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void EditLinks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{SettingsStore.FilePath}\"")
            { UseShellExecute = true });
        }
        catch (Exception)
        {
            SettingsStore.OpenFolder();
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e) => (Application.Current as App)?.ReloadLaunchers();

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => BeginDrag(e);
}
