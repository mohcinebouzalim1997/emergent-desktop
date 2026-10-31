using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace EmergentDesktop;

public partial class HeaderWindow : WidgetWindow
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(10) };

    public HeaderWindow()
    {
        InitializeComponent();
        UpdateDate();
        _timer.Tick += (_, _) => UpdateDate();
        _timer.Start();
        Closed += (_, _) => _timer.Stop();
    }

    private void UpdateDate() => HeaderDate.Text = DateTime.Now.ToString("ddd  dd MMM yyyy").ToUpperInvariant();

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => BeginDrag(e);

    private void Exit_Click(object sender, RoutedEventArgs e) => (Application.Current as App)?.ExitDeck();
}
