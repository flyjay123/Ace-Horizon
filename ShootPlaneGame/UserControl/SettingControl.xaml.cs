using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ShootPlaneGame.UserControl;

public partial class SettingControl:System.Windows.Controls.UserControl
{
    public SettingControl()
    {
        InitializeComponent();
    }
    
    private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider slider)
            return;

        var originalSource = e.OriginalSource as DependencyObject;

        while (originalSource != null && originalSource is not Thumb)
            originalSource = VisualTreeHelper.GetParent(originalSource);

        if (originalSource is Thumb)
            return;

        if (slider.Template.FindName("PART_Track", slider) is Track track)
        {
            Point position = e.GetPosition(slider);
            double ratio = position.X / slider.ActualWidth;
            double newValue = track.Minimum + (track.Maximum - track.Minimum) * ratio;
            slider.Value = newValue;

            e.Handled = true;
        }
    }
}