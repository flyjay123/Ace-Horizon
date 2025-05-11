using System.Collections.ObjectModel;
using ShootPlaneGame.Helper;

namespace ShootPlaneGame.ViewModels;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class GameViewModel : INotifyPropertyChanged
{
    private SettingsViewModel settingsViewModel;
    
    public GameViewModel()
    {
        settingsViewModel = new SettingsViewModel();
        Reset();
    }
    
    public GameViewModel(SettingsViewModel settingsViewModel)
    {
        this.settingsViewModel = settingsViewModel;
        Reset();
    }
    
    // The player's score
    private double score;
    public double Score
    {
        get => score;
        set => SetField(ref score, value);
    }
    
    // The number of lives left
    private int lives;
    public int Lives
    {
        get => lives;
        set
        {
            SetField(ref lives, value);
            OnPropertyChanged(nameof(LifeIcons));
        }
    }
    
    // Current Exp
    private double currentExp;
    public double CurrentExp
    {
        get => currentExp;
        set
        {
            if (value >= MaxExp)
            {
                Level++;
                MaxExp = settingsViewModel.LevelExp[MathHelper.Clamp(Level - 1, 0, settingsViewModel.LevelExp.Length - 1)];
                SetField(ref currentExp, 0);
            }
            else
            {
                SetField(ref currentExp, value);
            }
        }
    }

    // Max Exp
    private double maxExp;
    public double MaxExp
    {
        get => maxExp;
        set => SetField(ref maxExp, value);
    }
    
    private int level;
    public int Level
    {
        get => level;
        set => SetField(ref level, value);
    }

    public ObservableCollection<int> LifeIcons => new(LivesIcons());

    public IEnumerable<int> LivesIcons()
    {
        return Enumerable.Range(1, Lives);
    }
    
    // The frames per second (FPS)
    private int fps;
    public int FPS
    {
        get => fps;
        set => SetField(ref fps, value);
    }
    
    public void Reset()
    {
        Score = settingsViewModel.InitialScore;
        Lives = settingsViewModel.InitialLives;
        FPS = 0;
        
        Level = 1;
        MaxExp = settingsViewModel.LevelExp[0];
        CurrentExp = 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}