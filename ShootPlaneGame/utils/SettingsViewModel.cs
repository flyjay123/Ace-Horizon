using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShootPlaneGame.Utils;

public class SettingsViewModel : INotifyPropertyChanged
{
    public int InitialLives; // 初始生命值
    public int InitialScore; // 初始分数
    
    private bool _isMusicEnabled = true;
    private double _musicVolume = 0.5;
    private double _enemySpeed = 100;
    private double _bulletSpeed = 300;
    private double _bulletSpawnInterval = 200;
    private double _enemySpawnInterval = 1000;
    private double _bossSpawnInterval = 5000;
    
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

    public double MusicVolume
    {
        get => _musicVolume;
        set
        {
            SetField(ref _musicVolume, value);
            SoundPlayer.SetVolume(value / 100.0);
        }
    }

    public double EnemySpeed
    {
        get => _enemySpeed;
        set => SetField(ref _enemySpeed, value);
    }

    public double BulletSpeed
    {
        get => _bulletSpeed;
        set => SetField(ref _bulletSpeed, value);
    }

    public double BulletSpawnInterval
    {
        get => _bulletSpawnInterval;
        set => SetField(ref _bulletSpawnInterval, value);
    }
    
    public double EnemySpawnInterval
    {
        get => _enemySpawnInterval;
        set => SetField(ref _enemySpawnInterval, value);
    }
    
    public double BossSpawnInterval
    {
        get => _bossSpawnInterval;
        set => SetField(ref _bossSpawnInterval, value);
    }

    public SettingsViewModel()
    {
        Reset();
    }
    
    public void Reset()
    {
        EnemySpeed = 200;
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