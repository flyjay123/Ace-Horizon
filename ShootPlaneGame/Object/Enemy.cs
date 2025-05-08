using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShootPlaneGame.Object;

/// <summary>
/// 敌机自定义控件，包含血条显示功能
/// </summary>
public class Enemy : System.Windows.Controls.UserControl
{
    #region 依赖属性

    public static readonly DependencyProperty HealthProperty = DependencyProperty.Register(
        "Health", typeof(double), typeof(Enemy),
        new FrameworkPropertyMetadata(1.0,
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnHealthChanged));

    public static readonly DependencyProperty MaxHealthProperty = DependencyProperty.Register(
        "MaxHealth", typeof(double), typeof(Enemy),
        new FrameworkPropertyMetadata(1.0));

    public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(
        "Speed", typeof(double), typeof(Enemy),
        new FrameworkPropertyMetadata(100.0));

    public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
        "Position", typeof(Point), typeof(Enemy),
        new FrameworkPropertyMetadata(new Point(0, 0),
            FrameworkPropertyMetadataOptions.AffectsParentArrange,
            OnPositionChanged));

    #endregion

    public readonly Image Sprite;
    private readonly Rectangle _healthBarBackground;
    private readonly Rectangle _healthBarForeground;
    private const double HealthBarHeight = 5;
    private const double HealthBarOffset = 8;

    private double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));

    public double Health
    {
        get => (double)GetValue(HealthProperty);
        set => SetValue(HealthProperty, Clamp(value, 0, MaxHealth));
    }

    public double MaxHealth
    {
        get => (double)GetValue(MaxHealthProperty);
        set => SetValue(MaxHealthProperty, value);
    }

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public Point Position
    {
        get => (Point)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public Enemy(ImageSource spriteSource)
    {
        // 创建视觉树
        var grid = new Grid();

        // 敌机精灵
        Sprite = new Image
        {
            Source = spriteSource,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // 血条背景
        _healthBarBackground = new Rectangle
        {
            Fill = Brushes.DarkGray,
            Height = HealthBarHeight,
            Margin = new Thickness(0, -HealthBarOffset, 0, 0),
            VerticalAlignment = VerticalAlignment.Top
        };

        // 血条前景
        _healthBarForeground = new Rectangle
        {
            Fill = Brushes.LimeGreen,
            Height = HealthBarHeight,
            Margin = new Thickness(0, -HealthBarOffset, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // 组合控件
        grid.Children.Add(Sprite);
        grid.Children.Add(_healthBarBackground);
        grid.Children.Add(_healthBarForeground);
        Content = grid;

        // 初始设置
        SizeChanged += OnSizeChanged;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        var ratio = Health / MaxHealth;
        _healthBarForeground.Width = ActualWidth * ratio;
        _healthBarForeground.Fill = ratio switch
        {
            < 0.3 => Brushes.Red,
            < 0.6 => Brushes.Orange,
            _ => Brushes.LimeGreen
        };

        // 满血时隐藏血条
        var visibility = Math.Abs(ratio - 1.0) < 0.01 ? Visibility.Collapsed : Visibility.Visible;

        _healthBarBackground.Visibility = visibility;
        _healthBarForeground.Visibility = visibility;
    }

    #region 事件处理

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Sprite.Width = e.NewSize.Width;
        Sprite.Height = e.NewSize.Height;
        UpdateHealthBar();
    }

    private static void OnHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Enemy enemy)
            enemy.UpdateHealthBar();
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Enemy enemy && e.NewValue is Point newPos)
        {
            Canvas.SetLeft(enemy, newPos.X);
            Canvas.SetTop(enemy, newPos.Y);
        }
    }

    #endregion

    public void Update(double deltaTime)
    {
        Position = new Point(
            Position.X,
            Position.Y + Speed * deltaTime);
    }

    public Rect GetCollisionBounds() => new Rect(
        Position.X,
        Position.Y,
        ActualWidth,
        ActualHeight);

    public bool IsOutOfBounds(double canvasHeight) =>
        Position.Y > canvasHeight + ActualHeight;
}
