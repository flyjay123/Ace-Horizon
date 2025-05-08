using System.Diagnostics;

namespace ShootPlaneGame.Utils;

public class GameTime
{
    private Stopwatch stopwatch = new Stopwatch();
    private long lastTimestamp;
    
    private int frameCount = 0;
    private double fpsTimer = 0;

    public int FPS { get; private set; }

    public double DeltaTime { get; private set; } // 单位：秒
    public double TotalTime { get; private set; } // 单位：秒

    public GameTime()
    {
        stopwatch.Start();
        lastTimestamp = stopwatch.ElapsedMilliseconds;
        SoundPlayer.BeginBackgroundMusic("Assets/Sounds/tiaolouji.aac");
    }
    
    public void Stop()
    {
        stopwatch.Stop();
    }
    
    public void Start()
    {
        stopwatch.Start();
    }

    public void Update()
    {
        long current = stopwatch.ElapsedMilliseconds;
        DeltaTime = (current - lastTimestamp) / 1000.0;
        TotalTime = current / 1000.0;
        lastTimestamp = current;

        // FPS 统计
        frameCount++;
        fpsTimer += DeltaTime;

        if (fpsTimer >= 1.0)
        {
            FPS = frameCount;
            frameCount = 0;
            fpsTimer = 0;
        }
    }

    public void Reset()
    {
        stopwatch.Restart();
        lastTimestamp = stopwatch.ElapsedMilliseconds;
        DeltaTime = 0;
        TotalTime = 0;
    }
}