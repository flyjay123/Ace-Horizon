namespace ShootPlaneGame.Utils;

public static class GameSetting
{
    // 速度单位：像素/秒
    public static int PlayerSpeed = 500; // 玩家飞机速度
    public static int EnemySpeed = 300; // 敌机速度
    public static int BulletSpeed = 500; // 子弹速度

    public static int EnemySpawnInterval = 1000; // 敌机生成间隔（秒）
    public static int BulletSpawnInterval = 200; // 子弹生成间隔（秒）

    public static int InitialLives = 3; // 初始生命值
    public static int InitialScore = 0; // 初始分数
}