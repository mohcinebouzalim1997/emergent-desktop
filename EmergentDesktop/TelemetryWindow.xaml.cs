using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace EmergentDesktop;

public partial class TelemetryWindow : WidgetWindow
{
    private readonly SystemTelemetry _telemetry = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Queue<double> _cpuHistory = new();

    public TelemetryWindow()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => UpdateTelemetry();
        _timer.Start();
        Closed += (_, _) => _timer.Stop();
    }

    private void UpdateTelemetry()
    {
        TelemetrySnapshot s = _telemetry.Read();
        CpuText.Text = $"{s.CpuPercent:0}%";
        RamText.Text = $"{s.RamPercent:0}%";
        RamBar.Value = s.RamPercent;
        RamAmount.Text = $"{s.RamUsedGiB:0.0} / {s.RamTotalGiB:0.0} GiB USED";
        DownloadText.Text = SystemTelemetry.FormatRate(s.DownBytesPerSecond);
        UploadText.Text = SystemTelemetry.FormatRate(s.UpBytesPerSecond);

        _cpuHistory.Enqueue(s.CpuPercent);
        if (_cpuHistory.Count > 55) _cpuHistory.Dequeue();
        var points = new PointCollection();
        int n = _cpuHistory.Count;
        int i = 0;
        foreach (double value in _cpuHistory)
        {
            // 0% is graph bottom, 100% is graph top.
            points.Add(new Point(n < 2 ? 0 : i * 219.0 / 54.0,
                58.0 * (1.0 - Math.Clamp(value, 0, 100) / 100.0)));
            i++;
        }
        CpuGraph.Points = points;
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => BeginDrag(e);
}
