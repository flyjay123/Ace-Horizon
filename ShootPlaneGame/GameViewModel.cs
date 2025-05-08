using System.Collections.ObjectModel;
using ShootPlaneGame.Utils;

namespace ShootPlaneGame;

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
    private int score;
    public int Score
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