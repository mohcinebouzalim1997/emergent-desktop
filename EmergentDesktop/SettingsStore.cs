using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace EmergentDesktop;

public sealed record WidgetPosition(double Left, double Top);

public sealed class LauncherEntry
{
    public string Group { get; set; } = "SYSTEM";
    public string Label { get; set; } = "APP";
    public string Target { get; set; } = "explorer.exe";
    public string Arguments { get; set; } = "";
}

public sealed class DeckSettings
{
    public Dictionary<string, WidgetPosition> Positions { get; set; } = [];
    public List<LauncherEntry> Launchers { get; set; } =
    [
        new() { Group = "WORKSPACE", Label = "FILE EXPLORER", Target = "explorer.exe" },
        new() { Group = "WORKSPACE", Label = "DOCUMENTS", Target = "explorer.exe", Arguments = "shell:Personal" },
        new() { Group = "WORKSPACE", Label = "DOWNLOADS", Target = "explorer.exe", Arguments = "shell:Downloads" },
        new() { Group = "BUILD / ANALYZE", Label = "NOTEPAD", Target = "notepad.exe" },
        new() { Group = "BUILD / ANALYZE", Label = "CALCULATOR", Target = "calc.exe" },
        new() { Group = "BUILD / ANALYZE", Label = "EDGE", Target = "msedge.exe" },
        new() { Group = "SYSTEM", Label = "TASK MANAGER", Target = "taskmgr.exe" },
        new() { Group = "SYSTEM", Label = "SETTINGS", Target = "ms-settings:" }
    ];
}

public static class SettingsStore
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmergentDesktop");
    public static readonly string FilePath = Path.Combine(DirectoryPath, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static DeckSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                var defaults = new DeckSettings();
                Save(defaults);
                return defaults;
            }

            DeckSettings loaded = JsonSerializer.Deserialize<DeckSettings>(File.ReadAllText(FilePath), JsonOptions)
                ?? new DeckSettings();
            loaded.Positions ??= [];
            loaded.Launchers ??= [];
            return loaded;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // A broken or protected settings file must not prevent the UI from starting.
            return new DeckSettings();
        }
    }

    public static void Save(DeckSettings settings)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Managed workstations might block writes; widgets still work in memory.
        }
    }

    public static void OpenFolder()
    {
        Directory.CreateDirectory(DirectoryPath);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{DirectoryPath}\"") { UseShellExecute = true });
    }
}
