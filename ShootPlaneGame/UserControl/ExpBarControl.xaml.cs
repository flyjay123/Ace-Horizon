using System.Windows;
using ShootPlaneGame.Helper;

namespace ShootPlaneGame.UserControl;

public partial class ExpBarControl
{
    public ExpBarControl()
    {
        InitializeComponent();
        SizeChanged += (s, e) => UpdateSize();
    }
    
    public static readonly DependencyProperty CurrentExpProperty =
        DependencyProperty.Register(nameof(CurrentExp), typeof(double), typeof(ExpBarControl),
            new PropertyMetadata(0.0, OnExpChanged));

    public static readonly DependencyProperty MaxExpProperty =
        DependencyProperty.Register(nameof(MaxExp), typeof(double), typeof(ExpBarControl),
            new PropertyMetadata(100.0, OnExpChanged));

    public double CurrentExp
    {
        get => (double)GetValue(CurrentExpProperty);
        set => SetValue(CurrentExpProperty, value);
    }

    public double MaxExp
    {
        get => (double)GetValue(MaxExpProperty);
        set => SetValue(MaxExpProperty, value);
    }
    
    private static void OnExpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExpBarControl bar)
        {
            bar.UpdateBar();
        }
    }

    private void UpdateBar()
    {
        ExpText.Text = $"{CurrentExp} / {MaxExp}";
    }
    
    private void UpdateSize()
    {
        double ratio = MathHelper.Clamp(MaxExp == 0 ? 0 : CurrentExp / MaxExp, 0, 1);
        ExpFill.Width = ActualWidth * ratio;
    }
}