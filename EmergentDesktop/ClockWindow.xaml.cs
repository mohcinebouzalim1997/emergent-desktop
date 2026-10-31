using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace EmergentDesktop;

public partial class ClockWindow : WidgetWindow
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    public ClockWindow()
    {
        InitializeComponent();
        UpdateClock();
        _timer.Tick += (_, _) => UpdateClock();
        _timer.Start();
        Closed += (_, _) => _timer.Stop();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;
        ClockTime.Text = now.ToString("HH:mm");
        ClockDate.Text = now.ToString("dddd, dd MMMM yyyy").ToUpperInvariant();
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => BeginDrag(e);
}
