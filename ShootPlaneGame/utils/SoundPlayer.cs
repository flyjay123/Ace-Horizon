using System.Windows.Media;

namespace ShootPlaneGame.Utils;

public class SoundPlayer
{
    private static MediaPlayer mediaPlayer = new MediaPlayer();

    public static double MusicVolume
    {
        get => mediaPlayer.Volume;
        set => mediaPlayer.Volume = value;
    }

    public static void BeginBackgroundMusic(string path)
    {
        mediaPlayer.Open(new Uri(path, UriKind.Relative));
        mediaPlayer.MediaEnded += (s, e) => { mediaPlayer.Position = TimeSpan.Zero; mediaPlayer.Play(); };
        mediaPlayer.Play();
    }
    
    public static void PlayBackgroundMusic()
    {
        mediaPlayer.Play();
    }
    
    public static void PauseBackgroundMusic()
    {
        mediaPlayer.Pause();
    }
    
    public static void SetVolume(double volume)
    {
        mediaPlayer.Volume = volume;
    }

    public static void StopBackgroundMusic()
    {
        mediaPlayer.Stop();
    }

    public static void PlaySoundEffect(string path)
    {
        MediaPlayer soundEffect = new MediaPlayer();
        soundEffect.Open(new Uri(path, UriKind.Relative));
        soundEffect.Play();
    }
}