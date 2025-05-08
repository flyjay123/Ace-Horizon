using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShootPlaneGame.UserControl;

public partial class SettingControl:System.Windows.Controls.UserControl
{
    public SettingControl()
    {
        InitializeComponent();
    }
    
    private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Slider slider)
        {
            Point position = e.GetPosition(slider);
            double relative = position.X / slider.ActualWidth;
            double newValue = slider.Minimum + (slider.Maximum - slider.Minimum) * relative;
            slider.Value = newValue;
            e.Handled = true; // 阻止默认行为
        }
    }
}