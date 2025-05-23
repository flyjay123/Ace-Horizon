﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ShootPlaneGame.Assets;
using ShootPlaneGame.Utils;
using ShootPlaneGame.Object;
using ShootPlaneGame.ViewModels;
using WpfAnimatedGif;

namespace ShootPlaneGame.UserControl;

public static class CompositionTargetEx
{
    private static TimeSpan _last = TimeSpan.Zero;
    private static event EventHandler<RenderingEventArgs> _FrameUpdating;

    public static event EventHandler<RenderingEventArgs> FrameUpdating
    {
        add
        {
            if (_FrameUpdating == null) CompositionTarget.Rendering += CompositionTarget_Rendering;
            _FrameUpdating += value;
        }
        remove
        {
            _FrameUpdating -= value;
            if (_FrameUpdating == null) CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }
    }

    static void CompositionTarget_Rendering(object sender, EventArgs e)
    {
        RenderingEventArgs args = (RenderingEventArgs)e;
        if (args.RenderingTime == _last) return;
        _last = args.RenderingTime;
        _FrameUpdating(sender, args);
    }
}

public partial class GameView : System.Windows.Controls.UserControl
{
    private GameViewModel ViewModel;
    private GameTime gameTime = new GameTime();

    private bool moveLeft = false;
    private bool moveRight = false;
    private bool isPaused = false;

    private Random rand = new Random();
    private List<BitmapImage> catImages;
    
    private double bulletCooldown;
    private double enemySpawnCooldown;
    private double bossSpawnCooldown;
    private const double bossSize = 100;
    private SettingsViewModel settingsViewModel = new ();

    public GameView()
    {
        InitializeComponent();
        ViewModel = new GameViewModel(settingsViewModel);
        DataContext = ViewModel;
        SettingControl.DataContext = settingsViewModel;
        catImages = new List<BitmapImage>() { Resource.Cat1, Resource.Cat2, Resource.Cat3 };
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        InputMethod.SetIsInputMethodEnabled(this, false); // 禁用输入法
    }

    #region 事件处理

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            TogglePause();
        
        if(e.Key == Key.Tab)
            SettingControl.Visibility = SettingControl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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

        settingsViewModel.MusicVolume = SoundPlayer.MusicVolume * 100;

        CompositionTargetEx.FrameUpdating += GameLoop;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DamageOverlay.Width = ActualWidth;
        DamageOverlay.Height = ActualHeight;
    }
    
    private void GameView_OnMouseMove(object sender, MouseEventArgs e)
    {
        var mousePos = e.GetPosition(GameCanvas);
        var playerX = mousePos.X - Player.Width / 2;
        if (playerX < 0)
            playerX = 0;
        else if (playerX > GameCanvas.ActualWidth - Player.Width)
            playerX = GameCanvas.ActualWidth - Player.Width;
        Canvas.SetLeft(Player, playerX);
    }

    #endregion

    #region 游戏逻辑

    private void FireBullet()
    {
        int bulletWidth = 32;
        int bulletHeight = 32;
        int bulletCount = Math.Max(1, settingsViewModel.BulletCount); // 至少发一颗
        double spacing = 1;

        double playerXCenter = Canvas.GetLeft(Player) + Player.Width / 2;
        double playerY = Canvas.GetTop(Player) - bulletHeight;

        double totalWidth = bulletCount * bulletWidth + (bulletCount - 1) * spacing;
        double startX = playerXCenter - totalWidth / 2;

        for (int i = 0; i < bulletCount; i++)
        {
            double bulletX = startX + i * (bulletWidth + spacing);

            // 检查是否超出画布边界
            if (bulletX < 0 || bulletX + bulletWidth > GameCanvas.ActualWidth)
                continue;

            Bullet bullet = new Bullet(bulletX, playerY, bulletWidth, bulletHeight, Resource.Basketball)
            {
                Speed = settingsViewModel.BulletSpeed
            };

            GameCanvas.Children.Add(bullet);
        }
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        gameTime.Update();
        ViewModel.FPS = gameTime.FPS;
        double delta = gameTime.DeltaTime;
        MoveBulletsAndEnemies(delta);
        CheckCollisions();
        
        // 子弹发射间隔
        bulletCooldown += delta;
        if (bulletCooldown >= settingsViewModel.BulletSpawnInterval / 1000.0)
        {
            FireBullet();
            bulletCooldown = 0;
        }
        
        // 敌机生成间隔
        enemySpawnCooldown += delta;
        if (enemySpawnCooldown >= settingsViewModel.EnemySpawnInterval / 1000.0)
        {
            SpawnEnemy();
            enemySpawnCooldown = 0;
        }
        
        // Boss生成间隔
        bossSpawnCooldown += delta;
        if (bossSpawnCooldown >= settingsViewModel.BossSpawnInterval / 1000.0)
        {
            SpawnBoss();
            bossSpawnCooldown = 0;
        }
    }

    private void SpawnEnemy()
    {
        double x = rand.Next(0, (int)(GameCanvas.ActualWidth - 40));
        // select random image from catList
        BitmapImage cat = catImages[rand.Next(0, catImages.Count)];
        Enemy enemy = new Enemy(cat)
        {
            Position = new Point(x, 0),
            Speed = settingsViewModel.EnemySpeed,
            Width = 40,
            Height = 40,
            MaxHealth = 3,
            Health = 3,
        };
        ImageBehavior.SetAnimatedSource(enemy.Sprite, cat);

        GameCanvas.Children.Add(enemy);
    }

    private void SpawnBoss()
    {
        double x = rand.Next(0, (int)(GameCanvas.ActualWidth - bossSize));
        Enemy boss = new Enemy(Resource.KunKun1)
        {
            Position = new Point(x, 0),
            Width = bossSize,
            Height = bossSize,
            MaxHealth = 6,
            Health = 6,
            ScoreValue = 25,
            ExpValue = 2.5
        };
        ImageBehavior.SetAnimatedSource(boss.Sprite, Resource.KunKun1);
        GameCanvas.Children.Add(boss);
    }


    private void MoveBulletsAndEnemies(double delta)
    {
        List<UIElement> toRemove = new();
        List<Enemy> enemiesToRemove = new();

        foreach (UIElement el in GameCanvas.Children)
        {
            if (el is Bullet bullet)
            {
                if (bullet.Position.Y < 0)
                    toRemove.Add(bullet);
                bullet.Position = bullet.Position with { Y = bullet.Position.Y - bullet.Speed * delta };
            }
            else if (el is Enemy enemy)
            {
                enemy.Position = enemy.Position with { Y = enemy.Position.Y + enemy.Speed * delta };
                // 让敌人从上到下移动

                // ⛔ 检查是否越过底部
                if (enemy.Position.Y + enemy.Height >= GameCanvas.ActualHeight)
                {
                    LoseLife();
                    if (ViewModel.Lives <= 0)
                    {
                        GameOver(); // 生命耗尽则失败
                    }

                    GameCanvas.Children.Remove(enemy); // 移除敌人
                    return; // 不再继续处理这个敌人
                }


                if (enemy.Position.Y > GameCanvas.ActualHeight)
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
        var bullets = GameCanvas.Children.OfType<Bullet>().ToList();
        var enemies = GameCanvas.Children.OfType<Enemy>().ToList();

        foreach (var bullet in bullets)
        {
            Rect bulletRect = bullet.GetBounds();
            foreach (var enemy in enemies)
            {
                Rect enemyRect = enemy.GetCollisionBounds();

                if (bulletRect.IntersectsWith(enemyRect))
                {
                    // 击中敌人
                    enemy.Health -= bullet.AttackPower;
                    if (enemy.Health <= 0)
                    {
                        // 击毁敌人
                        GameCanvas.Children.Remove(bullet);
                        GameCanvas.Children.Remove(enemy);
                        KillEnemy(enemy);
                    }
                    else
                    {
                        // 击中但未击毁敌人
                        GameCanvas.Children.Remove(bullet);
                    }
                    
                    // 伤害数值动画
                    var damageText = new TextBlock
                    {
                        Text = bullet.AttackPower.ToString(),
                        Foreground = Brushes.Red,
                        FontSize = 20,
                        FontWeight = FontWeights.Bold
                    };
                    Canvas.SetLeft(damageText, enemy.Position.X + enemy.Width / 2);
                    Canvas.SetTop(damageText, enemy.Position.Y + enemy.Height / 2);
                    GameCanvas.Children.Add(damageText);
                    var anim = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(500),
                        AutoReverse = false
                    };
                    anim.Completed += (s, e) =>
                    {
                        GameCanvas.Children.Remove(damageText);
                    };
                    damageText.BeginAnimation(OpacityProperty, anim);
                }
            }
        }
    }

    private void LoseLife()
    {
        ViewModel.Lives -= 1;
        if (ViewModel.Lives <= 0)
        {
            GameOver(); // 生命耗尽则失败
        }

        System.Media.SystemSounds.Hand.Play();
        ShowDamageFlash();
        ShakeWindow();
    }

    private void KillEnemy(Enemy enemy)
    {
        GameCanvas.Children.Remove(enemy);
        ViewModel.Score += enemy.ScoreValue;
        ViewModel.CurrentExp += enemy.ExpValue;

        ShowExplosionEffect(enemy.Position.X, enemy.Position.Y);
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

            EffectsCanvas.Children.Add(particle);

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
            if (settingsViewModel.IsMusicEnabled)
                SoundPlayer.PauseBackgroundMusic();
            CompositionTargetEx.FrameUpdating -= GameLoop;
            PauseMenu.Visibility = Visibility.Visible;
            gameTime.Stop();
            isPaused = true;
        }
        else
        {
            if(settingsViewModel.IsMusicEnabled)
                SoundPlayer.PlayBackgroundMusic();
            CompositionTargetEx.FrameUpdating += GameLoop;
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
            if (el is Bullet or Enemy)
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
        ViewModel.Reset();
        gameTime.Reset();
        
        if(settingsViewModel.IsMusicEnabled)
            SoundPlayer.PlayBackgroundMusic();

        // 重新开始循环
        CompositionTargetEx.FrameUpdating += GameLoop;
    }

    private void GameOver()
    {
        CompositionTargetEx.FrameUpdating -= GameLoop;
        PauseMenu.Visibility = Visibility.Collapsed;
        RestartButton.Visibility = Visibility.Visible;
    }

    #endregion
}