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
/// </remarks>
public class Bullet : Image
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = 100;
    
    public double AttackPower { get; set; } = 1.0;

    public Bullet(double startX, double startY, double width, double height, ImageSource source)
    {
        Width = width;
        Height = height;

        Source = source;

        X = startX;
        Y = startY;

        Tag = "Bullet";

        Canvas.SetLeft(this, X);
        Canvas.SetTop(this, Y);
    }

    public Bullet()
    {

    }

    public void Update(double deltaTime)
    {
        Y += Speed * deltaTime;
        Canvas.SetTop(this, Y);
    }

    public Rect GetBounds()
    {
        return new Rect(X, Y, RenderSize.Width, RenderSize.Height);
    }

    public bool IsOutOfCanvas(double canvasHeight)
    {
        return Y > canvasHeight;
    }

    public void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
        Canvas.SetLeft(this, X);
        Canvas.SetTop(this, Y);
    }
}