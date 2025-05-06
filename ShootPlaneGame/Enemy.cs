using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ShootPlaneGame;

/// <summary>
/// 敌机类
/// </summary>
/// <remarks>
/// 继承自Image类，表示游戏中的敌机。
/// </remarks>
public class Enemy : Image
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = 100; // pixels per second

    public double Width { get; set; } = 50;
    public double Height { get; set; } = 50;

    public Enemy(double startX, double startY, double width, double height, ImageSource source)
    {
        Width = width;
        Height = height;
        Stretch = Stretch.Fill;

        Source = source;

        X = startX;
        Y = startY;

        Tag = "Enemy";

        Canvas.SetLeft(this, X);
        Canvas.SetTop(this, Y);
    }

    public void Update(double deltaTime)
    {
        Y += Speed * deltaTime;
        Canvas.SetTop(this, Y);
    }

    public Rect GetBounds()
    {
        return new Rect(X, Y, Width, Height);
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