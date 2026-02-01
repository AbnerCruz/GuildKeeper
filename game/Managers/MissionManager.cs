using System;
using System.Collections.Generic;

public class MissionManager
{
    public World World;
    public List<Hero> Party = new();
    public Dungeon Dungeon;

    public MissionManager(World world, Dungeon dungeon)
    {
        World = world;
        Dungeon = dungeon;
    }

    public void Start()
    {
        if (Dungeon.DungeonState != DungeonState.NotStarted) return;
        Dungeon.DungeonState = DungeonState.Cleaning;
        Dungeon.NextActionTime = World.WorldTimeManager.CurrentTime.AddMinutes(Rng.Rand.Next(Dungeon.smallDelay, Dungeon.timePerRoom));

        Dungeon.PrepareDungeon(Party);
    }

    public void Update()
    {
        Party.RemoveAll(h => h.HP <= 0);
        if (Dungeon != null)
        {
            Dungeon.UpdateDungeon();
        }
    }

    public bool AddToParty(Hero hero)
    {
        if (!Party.Contains(hero) && Party.Count < 4)
        {
            Party.Add(hero);
            World.Guild.Fire(hero);
            return true;
        }
        return false;
    }

    public bool RemoveParty(Hero hero)
    {
        if (Party.Contains(hero))
        {
            Party.Remove(hero);
            World.Guild.Hire(hero);
            return true;
        }
        return false;
    }
}