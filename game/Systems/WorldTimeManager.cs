using System;
using Microsoft.Xna.Framework;

public class WorldTimeManager
{
    public World World;
    public float TimeScale { get; set; } = 10f;

    public DateTime CurrentTime { get; private set; }
    private DateTime _lasCheck;

    public WorldTimeManager(World world)
    {
        World = world;
        CurrentTime = new DateTime(1200, 1, 1);
        _lasCheck = CurrentTime;
    }
    public void Update()
    {
        SetTurboMode(true, 500);
        double minutesPassed = Time.DeltaTime * TimeScale;
        CurrentTime = CurrentTime.AddMinutes(minutesPassed);
        CheckEvents();
    }

    public void CheckEvents()
    {
        if (CurrentTime.Month != _lasCheck.Month)
        {
            World.Guild.PayDay();
        }
        _lasCheck = CurrentTime;
    }
    public void SetTurboMode(bool enabled, int turbo)
    {
        TimeScale = enabled ? turbo : 10f; // 5000x mais rápido ou volta ao normal
    }

    public void SkipMonth()
    {
        CurrentTime = CurrentTime.AddDays(30);
        Console.WriteLine("DEBUG: Saltou 30 dias no tempo.");
    }

}