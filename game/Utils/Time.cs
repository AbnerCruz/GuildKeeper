using Microsoft.Xna.Framework;

public static class Time
{
    public static GameTime gameTime;
    public static float DeltaTime => (float)gameTime.ElapsedGameTime.TotalSeconds;
    public static double Tick = 0.2;

    public static bool Timer(ref float timer)
    {
        if (timer <= 0f) return true;

        timer -= DeltaTime;
        return timer <= 0f;
    }

    public static void ResetTimer(ref float timer, float duration)
    {
        timer = duration;
    }
}