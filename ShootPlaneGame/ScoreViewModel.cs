namespace ShootPlaneGame;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ScoreViewModel : INotifyPropertyChanged
{
    private int score;

    public int Score
    {
        get => score;
        set
        {
            if (score != value)
            {
                score = value;
                OnPropertyChanged();
            }
        }
    }
    
    private int lives = 3;
    public int Lives
    {
        get => lives;
        set
        {
            if (lives != value)
            {
                lives = value;
                OnPropertyChanged();
            }
        }
    }
    
    public void Reset()
    {
        Score = 0;
        Lives = 3;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}