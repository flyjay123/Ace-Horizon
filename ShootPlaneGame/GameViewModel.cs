namespace ShootPlaneGame;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class GameViewModel : INotifyPropertyChanged
{
    // The player's score
    private int score;
    public int Score
    {
        get => score;
        set => SetField(ref score, value);
    }
    
    // The number of lives left
    private int lives = 3;
    public int Lives
    {
        get => lives;
        set => SetField(ref lives, value);
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
        Score = 0;
        Lives = 3;
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