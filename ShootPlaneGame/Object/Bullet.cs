using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using ShootPlaneGame.Assets;

namespace ShootPlaneGame.Object;

/// <summary>
/// 子弹类
/// </summary>
/// <remarks>
/// 继承自Image类，表示游戏中的子弹。
/// 
/// 人生就像一段代码，没有注释就容易让人摸不着头脑，及时记录，后人才能读懂你的故事。
/// 人生中总有一些子弹是需要射出去的，哪怕它们的命运是被敌人挡住的。
/// 这个类的设计灵感来源于游戏中的子弹，象征着人生中的努力和追求。
/// 人生就像一段代码，子弹就是我们在这段代码中不断发射的希望和梦想。
/// </remarks>
public class Bullet : Image
{
    public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
        "Position", typeof(Point), typeof(Bullet),
        new FrameworkPropertyMetadata(new Point(0, 0),
            FrameworkPropertyMetadataOptions.AffectsParentArrange,
            OnPositionChanged));
    
    public Point Position
    {
        get => (Point)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }
    
    public double Speed { get; set; } = 100;
    
    public double AttackPower { get; set; } = 1.0;

    public Bullet(double startX, double startY, double width, double height, ImageSource source)
    {
        Width = width;
        Height = height;

        Source = source;

        Position = new Point(startX, startY);
    }

    public Bullet()
    {

    }

    public Rect GetBounds()
    {
        return new Rect(Position.X, Position.Y, RenderSize.Width, RenderSize.Height);
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Bullet bullet && e.NewValue is Point newPos)
        {
            Canvas.SetLeft(bullet, newPos.X);
            Canvas.SetTop(bullet, newPos.Y);
        }
    }
}