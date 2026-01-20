using System;
using System.Collections.Generic;

public class MissionManager
{
    public World World;
    public List<Hero> Party = new();
    Dungeon Dungeon;

    public MissionManager(World world, Dungeon dungeon)
    {
        World = world;
        Dungeon = dungeon;
    }

    public void Start()
    {
        Dungeon.DungeonState = DungeonState.Cleaning;
        Console.WriteLine("Dungeon Cleaning");
        Dungeon.DungeonCycle();
        Party.RemoveAll(h => h.HP <= 0);
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