using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ShootPlaneGame.Assets;
using ShootPlaneGame.Utils;
using ShootPlaneGame.Object;
using WpfAnimatedGif;

namespace ShootPlaneGame.UserControl;

public partial class GameView : System.Windows.Controls.UserControl
{
    private GameViewModel viewModel = new();
    private GameTime gameTime = new GameTime();

    private bool moveLeft = false;
    private bool moveRight = false;
    private bool isPaused = false;

    private Random rand = new Random();
    private List<BitmapImage> catImages;
    
    private double bulletCooldown;
    private double enemySpawnCooldown;

    public GameView()
    {
        InitializeComponent();
        DataContext = viewModel;
        catImages = new List<BitmapImage>() { Resource.Cat1, Resource.Cat2, Resource.Cat3 };
        InputMethod.SetIsInputMethodEnabled(this, false); // 禁用输入法
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

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
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
        int height = 32;
        int width = 32;
        
        double playerX = Canvas.GetLeft(Player) + Player.Width / 2 - width / 2;
        double playerY = Canvas.GetTop(Player) - height;
        
        Bullet bullet = new Bullet(playerX, playerY, width, height, Resource.Basketball)
        {
            Tag = "Bullet"
        };
        
        GameCanvas.Children.Add(bullet);
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        gameTime.Update();
        viewModel.FPS = gameTime.FPS;
        double delta = gameTime.DeltaTime;
        MovePlayer(delta);
        MoveBulletsAndEnemies(delta);
        CheckCollisions();
        
        // 子弹发射间隔
        bulletCooldown += delta;
        if (bulletCooldown >= GameSetting.BulletSpawnInterval / 1000.0)
        {
            FireBullet();
            bulletCooldown = 0;
        }
        
        // 敌机生成间隔
        enemySpawnCooldown += delta;
        if (enemySpawnCooldown >= GameSetting.EnemySpawnInterval / 1000.0)
        {
            SpawnEnemy();
            enemySpawnCooldown = 0;
        }
    }

    private void MovePlayer(double delta)
    {
        double left = Canvas.GetLeft(Player);

        if (moveLeft && left > 0)
            Canvas.SetLeft(Player, left - GameSetting.PlayerSpeed * delta);
        else if (moveRight && left < GameCanvas.ActualWidth - Player.Width)
            Canvas.SetLeft(Player, left + GameSetting.PlayerSpeed * delta);
    }

    private void SpawnEnemy()
    {
        double x = rand.Next(0, (int)(GameCanvas.ActualWidth - 40));
        // select random image from catList
        BitmapImage cat = catImages[rand.Next(0, catImages.Count)];
        Enemy enemy = new Enemy(x, -40, 40, 40, cat);
        ImageBehavior.SetAnimatedSource(enemy, cat);

        GameCanvas.Children.Add(enemy);
    }


    private void MoveBulletsAndEnemies(double delta)
    {
        List<UIElement> toRemove = new();
        List<Enemy> enemiesToRemove = new();

        foreach (UIElement el in GameCanvas.Children)
        {
            if (el is Bullet bullet && (string)bullet.Tag == "Bullet")
            {
                double top = Canvas.GetTop(bullet);
                Canvas.SetTop(bullet, top - GameSetting.BulletSpeed * delta);

                if (top < 0)
                    toRemove.Add(bullet);
            }
            else if (el is Enemy enemy && (string?)enemy.Tag == "Enemy")
            {
                double top = Canvas.GetTop(enemy);
                Canvas.SetTop(enemy, top + GameSetting.EnemySpeed * delta);

                // ⛔ 检查是否越过底部
                if (top + enemy.Height >= GameCanvas.ActualHeight)
                {
                    LoseLife();
                    if (viewModel.Lives <= 0)
                    {
                        GameOver(); // 生命耗尽则失败
                    }

                    GameCanvas.Children.Remove(enemy); // 移除敌人
                    return; // 不再继续处理这个敌人
                }


                if (Canvas.GetTop(enemy) > GameCanvas.ActualHeight)
                {
                    enemiesToRemove.Add(enemy);
                }
            }
        }

        foreach (var el in toRemove)
        {
            GameCanvas.Children.Remove(el);
        }
        
        foreach (var enemy in enemiesToRemove)
        {
            GameCanvas.Children.Remove(enemy);
        }
    }

    private void CheckCollisions()
    {
        var bullets = GameCanvas.Children.OfType<Bullet>().Where(r => (string)r.Tag == "Bullet").ToList();
        var enemies = GameCanvas.Children.OfType<Enemy>().Where(r => (string)r.Tag == "Enemy").ToList();

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

    private void KillEnemy(Enemy enemy)
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

        anim.Completed += (s, e) => { DamageOverlay.Visibility = Visibility.Collapsed; };

        DamageOverlay.BeginAnimation(OpacityProperty, anim);
    }

    private async void ShakeWindow(int shakeAmplitude = 10, int shakeCount = 5)
    {
        var parentWindow = Window.GetWindow(this);
        if (parentWindow == null)
            return;

        var originalLeft = parentWindow.Left;
        var originalTop = parentWindow.Top;

        for (int i = 0; i < shakeCount; i++)
        {
            parentWindow.Left += shakeAmplitude;
            await Task.Delay(20);
            parentWindow.Left -= shakeAmplitude * 2;
            await Task.Delay(20);
            parentWindow.Left = originalLeft;
            await Task.Delay(20);
        }

        parentWindow.Left = originalLeft;
        parentWindow.Top = originalTop;
    }

    private void ShowExplosionEffect(double x, double y)
    {
        int particleCount = 20; // 爆炸粒子数量
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

            double offsetX = rand.Next(-20, 20);
            double offsetY = rand.Next(-20, 20);

            Canvas.SetLeft(particle, x + offsetX);
            Canvas.SetTop(particle, y + offsetY);

            GameCanvas.Children.Add(particle);

            // 每个动画单独创建
            var animationScaleX = new DoubleAnimation(1, 4, TimeSpan.FromMilliseconds(150));
            var animationScaleY = new DoubleAnimation(1, 4, TimeSpan.FromMilliseconds(150));
            var animationFade = new DoubleAnimation(0.8, 0, TimeSpan.FromMilliseconds(150));

            // 注册到当前动画实例
            animationFade.Completed += (s, e) => GameCanvas.Children.Remove(particle);

            particle.BeginAnimation(WidthProperty, animationScaleX);
            particle.BeginAnimation(HeightProperty, animationScaleY);
            particle.BeginAnimation(OpacityProperty, animationFade);
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
            gameTime.Stop();
            isPaused = true;
        }
        else
        {
            CompositionTarget.Rendering += GameLoop;
            PauseMenu.Visibility = Visibility.Collapsed;
            gameTime.Start();
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
            if (el is Bullet r && ((string?)r.Tag == "Bullet"))
                toRemove.Add(el);

            else if (el is Enemy enemy && (string?)enemy.Tag == "Enemy")
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
        viewModel.Reset();
        gameTime.Reset();

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