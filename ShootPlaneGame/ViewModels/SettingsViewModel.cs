using System.ComponentModel;
using System.Runtime.CompilerServices;
using ShootPlaneGame.Utils;

namespace ShootPlaneGame.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public int InitialLives; // 初始生命值
    public int InitialScore; // 初始分数
    // 升级所需经验值
    public int[] LevelExp = { 5, 20, 50, 100, 200, 300, 400, 500};
    
    private bool _isMusicEnabled;
    public bool IsMusicEnabled
    {
        get => _isMusicEnabled;
        set
        {
            _isMusicEnabled = value; OnPropertyChanged(nameof(IsMusicEnabled));
            if (value)
            {
                SoundPlayer.PlayBackgroundMusic();
            }
            else
            {
                SoundPlayer.PauseBackgroundMusic();
            }
        }
    }

    private double _musicVolume = 0.5;
    public double MusicVolume
    {
        get => _musicVolume;
        set
        {
            SetField(ref _musicVolume, value);
            SoundPlayer.SetVolume(value / 100.0);
        }
    }

    private double _enemySpeed = 100;
    public double EnemySpeed
    {
        get => _enemySpeed;
        set => SetField(ref _enemySpeed, value);
    }

    private double _bulletSpeed = 300;
    public double BulletSpeed
    {
        get => _bulletSpeed;
        set => SetField(ref _bulletSpeed, value);
    }

    private int _bulletCount = 1;
    public int BulletCount
    {
        get => _bulletCount;
        set => SetField(ref _bulletCount, value);
    }

    private double _bulletSpawnInterval = 200;
    public double BulletSpawnInterval
    {
        get => _bulletSpawnInterval;
        set => SetField(ref _bulletSpawnInterval, value);
    }
    
    private double _enemySpawnInterval = 1000;
    public double EnemySpawnInterval
    {
        get => _enemySpawnInterval;
        set => SetField(ref _enemySpawnInterval, value);
    }
    
    private double _bossSpawnInterval = 5000;
    public double BossSpawnInterval
    {
        get => _bossSpawnInterval;
        set => SetField(ref _bossSpawnInterval, value);
    }

    public SettingsViewModel()
    {
        IsMusicEnabled = false;
        Reset();
    }
    
    public void Reset()
    {
        EnemySpeed = 100;
        BulletSpeed = 500;
        EnemySpawnInterval = 1000;
        BulletSpawnInterval = 200;
        BossSpawnInterval = 5000;
        InitialLives = 3;
        InitialScore = 0;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        if (propertyName != null) OnPropertyChanged(propertyName);
    }
}