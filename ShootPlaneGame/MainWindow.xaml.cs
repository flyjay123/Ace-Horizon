using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShootPlaneGame;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ScoreViewModel viewModel = new();
    
    private double playerSpeed = 5;
    private bool moveLeft = false;
    private bool moveRight = false;
    private bool isPaused = false;
    
    private Random rand = new Random();
    private int frameCount = 0;
    
    
    public MainWindow()
    {
        InitializeComponent();
        InputMethod.SetIsInputMethodEnabled(this, false); // 禁用输入法

        DataContext = viewModel;
        Loaded += MainWindow_Loaded;
    }

    #region 事件处理

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Left or Key.A)
            moveLeft = true;
        else if (e.Key is Key.Right or Key.D)
            moveRight = true;
        else if (e.Key == Key.Escape)
            TogglePause();
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Left or Key.A)
            moveLeft = false;
        else if (e.Key is Key.Right or Key.D)
            moveRight = false;
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        RestartGame();
    }
    
    private void ResumeButton_Click(object sender, RoutedEventArgs e)
    {
        TogglePause(); // 直接切换回游戏
    }
    
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始放置飞机在底部中间
        Canvas.SetLeft(Player, (GameCanvas.ActualWidth - Player.Width) / 2);
        Canvas.SetTop(Player, GameCanvas.ActualHeight - Player.Height - 20);
        Keyboard.Focus(this);
        
        DamageOverlay.Width = ActualWidth;
        DamageOverlay.Height = ActualHeight;
        
        CompositionTarget.Rendering += GameLoop;
    }
    
    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DamageOverlay.Width = ActualWidth;
        DamageOverlay.Height = ActualHeight;
    }
    
    #endregion

    #region 游戏逻辑

    private void FireBullet()
    {
        Rectangle bullet = new Rectangle
        {
            Width = 5,
            Height = 20,
            Fill = Brushes.Yellow,
            Tag = "Bullet"
        };

        double playerX = Canvas.GetLeft(Player) + Player.Width / 2 - bullet.Width / 2;
        double playerY = Canvas.GetTop(Player) - bullet.Height;

        Canvas.SetLeft(bullet, playerX);
        Canvas.SetTop(bullet, playerY);
        GameCanvas.Children.Add(bullet);
    }
    
    private void GameLoop(object? sender, EventArgs e)
    {
        frameCount++;
        MovePlayer();
        MoveBullets();
        MoveEnemies();
        CheckCollisions();
        if(frameCount % 10 == 0)
            FireBullet();

        // 每隔 50 帧生成一个敌机
        if (frameCount % 50 == 0)
            SpawnEnemy();
    }
    
    private void MovePlayer()
    {
        double left = Canvas.GetLeft(Player);

        if (moveLeft && left > 0)
            Canvas.SetLeft(Player, left - playerSpeed);
        else if (moveRight && left < GameCanvas.ActualWidth - Player.Width)
            Canvas.SetLeft(Player, left + playerSpeed);
    }
    
    private void SpawnEnemy()
    {
        Rectangle enemy = new Rectangle
        {
            Width = 40,
            Height = 40,
            Fill = Brushes.Red,
            Tag = "Enemy"
        };

        double x = rand.Next(0, (int)(GameCanvas.ActualWidth - enemy.Width));
        Canvas.SetLeft(enemy, x);
        Canvas.SetTop(enemy, -enemy.Height);
        GameCanvas.Children.Add(enemy);
    }

    private void MoveEnemies()
    {
        List<Rectangle> enemiesToRemove = new();

        foreach (UIElement el in GameCanvas.Children)
        {
            if (el is Rectangle r && (string?)r.Tag == "Enemy")
            {
                double top = Canvas.GetTop(r);
                Canvas.SetTop(r, top + 4);

                // ⛔ 检查是否越过底部
                if (top + r.Height >= GameCanvas.ActualHeight)
                {
                    LoseLife();
                    if (viewModel.Lives <= 0)
                    {
                        GameOver(); // 生命耗尽则失败
                    }
                    GameCanvas.Children.Remove(r); // 移除敌人
                    return; // 不再继续处理这个敌人
                }


                if (Canvas.GetTop(r) > GameCanvas.ActualHeight)
                {
                    enemiesToRemove.Add(r);
                }
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            GameCanvas.Children.Remove(enemy);
        }
    }

    
    private void MoveBullets()
    {
        List<UIElement> toRemove = new();

        foreach (UIElement el in GameCanvas.Children)
        {
            if (el is Rectangle r && (string)r.Tag == "Bullet")
            {
                double top = Canvas.GetTop(r);
                Canvas.SetTop(r, top - 10);

                if (top < 0)
                    toRemove.Add(r);
            }
        }

        foreach (var el in toRemove)
            GameCanvas.Children.Remove(el);
    }

    private void CheckCollisions()
    {
        var bullets = GameCanvas.Children.OfType<Rectangle>().Where(r => (string)r.Tag == "Bullet").ToList();
        var enemies = GameCanvas.Children.OfType<Rectangle>().Where(r => (string)r.Tag == "Enemy").ToList();

        foreach (var bullet in bullets)
        {
            Rect bulletRect = new(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.RenderSize.Width, bullet.RenderSize.Height);

            foreach (var enemy in enemies)
            {
                Rect enemyRect = new(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), enemy.RenderSize.Width, enemy.RenderSize.Height);

                if (bulletRect.IntersectsWith(enemyRect))
                {
                    GameCanvas.Children.Remove(bullet);
                    KillEnemy(enemy);
                    return; // 避免多次修改集合
                }
            }
        }
    }
    
    private void LoseLife()
    {
        viewModel.Lives -= 1;
        if (viewModel.Lives <= 0)
        {
            GameOver(); // 生命耗尽则失败
        }
        
        // var player = new System.Media.SoundPlayer("Resources/alert.wav");
        // player.Play();
        System.Media.SystemSounds.Hand.Play();
        ShowDamageFlash();
        ShakeWindow();
    }
    
    private void KillEnemy(Rectangle enemy)
    {
        GameCanvas.Children.Remove(enemy);
        viewModel.Score += 1;
        
        // System.Media.SystemSounds.Beep.Play();
        ShowExplosionEffect(Canvas.GetLeft(enemy), Canvas.GetTop(enemy));
    }
    
    #endregion

    #region 特效

    private void ShowDamageFlash()
    {
        DamageOverlay.Opacity = 0;
        DamageOverlay.Visibility = Visibility.Visible;

        var anim = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = 0,
            To = 0.6,
            AutoReverse = true,
            Duration = TimeSpan.FromMilliseconds(150),
        };

        anim.Completed += (s, e) =>
        {
            DamageOverlay.Visibility = Visibility.Collapsed;
        };

        DamageOverlay.BeginAnimation(OpacityProperty, anim);
    }
    
    private async void ShakeWindow(int shakeAmplitude = 10, int shakeCount = 5)
    {
        var originalLeft = this.Left;
        var originalTop = this.Top;

        for (int i = 0; i < shakeCount; i++)
        {
            this.Left += shakeAmplitude;
            await Task.Delay(20);
            this.Left -= shakeAmplitude * 2;
            await Task.Delay(20);
            this.Left = originalLeft;
            await Task.Delay(20);
        }

        this.Left = originalLeft;
        this.Top = originalTop;
    }
    
    private void ShowExplosionEffect(double x, double y)
    {
        int particleCount = 20;  // 爆炸粒子数量
        Random rand = new Random();

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorBrush(Colors.OrangeRed),
                Opacity = 0.8
            };

            // 计算粒子随机位置和速度
            double offsetX = rand.Next(-20, 20);
            double offsetY = rand.Next(-20, 20);

            Canvas.SetLeft(particle, x + offsetX);
            Canvas.SetTop(particle, y + offsetY);

            GameCanvas.Children.Add(particle);

            // 动画效果：粒子放大并淡出
            var animationScaleX = new DoubleAnimation(1, 4, TimeSpan.FromMilliseconds(150));
            var animationScaleY = new DoubleAnimation(1, 4, TimeSpan.FromMilliseconds(150));
            var animationFade = new DoubleAnimation(0.8, 0, TimeSpan.FromMilliseconds(150));

            particle.BeginAnimation(WidthProperty, animationScaleX);
            particle.BeginAnimation(HeightProperty, animationScaleY);
            particle.BeginAnimation(OpacityProperty, animationFade);

            // 动画结束后移除粒子
            animationFade.Completed += (s, e) => GameCanvas.Children.Remove(particle);
        }
    }

    #endregion
    
    # region 菜单
    private void TogglePause()
    {
        if (!isPaused)
        {
            CompositionTarget.Rendering -= GameLoop;
            PauseMenu.Visibility = Visibility.Visible;
            isPaused = true;
        }
        else
        {
            CompositionTarget.Rendering += GameLoop;
            PauseMenu.Visibility = Visibility.Collapsed;
            isPaused = false;
        }
    }
    
    private void RestartGame()
    {
        PauseMenu.Visibility = Visibility.Collapsed;
        RestartButton.Visibility = Visibility.Collapsed;
        isPaused = false;

        // 清理敌人和子弹
        List<UIElement> toRemove = new();

        foreach (UIElement el in GameCanvas.Children)
        {
            if (el is Rectangle r && ((string?)r.Tag == "Bullet" || (string?)r.Tag == "Enemy"))
                toRemove.Add(el);
        }

        foreach (var el in toRemove)
            GameCanvas.Children.Remove(el);

        // 重置玩家位置
        Canvas.SetLeft(Player, (GameCanvas.ActualWidth - Player.Width) / 2);
        Canvas.SetTop(Player, GameCanvas.ActualHeight - Player.Height - 20);

        // 重置变量
        moveLeft = false;
        moveRight = false;
        frameCount = 0;
        viewModel.Reset();

        // 重新开始循环
        CompositionTarget.Rendering += GameLoop;
    }
    
    private void GameOver()
    {
        CompositionTarget.Rendering -= GameLoop;
        PauseMenu.Visibility = Visibility.Collapsed;
        RestartButton.Visibility = Visibility.Visible;
    }

    #endregion
}