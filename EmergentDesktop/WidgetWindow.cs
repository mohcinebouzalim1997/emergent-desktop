using System;
using System.Windows;
using System.Windows.Input;

namespace EmergentDesktop;

public class WidgetWindow : Window
{
    protected void BeginDrag(MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || e.ClickCount != 1) return;
        try
        {
            DragMove();
            (Application.Current as App)?.SavePosition(this);
        }
        catch (InvalidOperationException)
        {
            // Drag can be interrupted by display changes or input cancellation.
        }
    }
}
