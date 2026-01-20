using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Guild
{
    public World World { get; private set; }
    public string Name = "New Guild";

    public int Gold;
    public int Debt;

    public int Level { get; set; } = 1;
    public int XP { get; set; }
    public int MaxXP => Level * 1000;

    public List<Miners> Miners { get; set; } = new();
    public List<Hero> Heroes { get; set; } = new();
    public List<Hero> Applicants { get; set; } = new();

    public List<Dungeon> OwnedDungeons { get; set; } = new();
    public List<Dungeon> AvailableDungeons { get; set; } = new();

    public List<MissionManager> Missions = new();

    public Guild(World world, string name)
    {
        World = world;
        Gold = 500;
        Name = name;
    }

    public bool Hire(Hero hero)
    {
        if (!Heroes.Contains(hero))
        {
            hero.Guild = this;
            World.Instantiate(hero);
            Heroes.Add(hero);
            return true;
        }
        return false;
    }

    public bool Fire(Hero hero)
    {
        if (Heroes.Contains(hero))
        {
            World.Destroy(hero);
            Heroes.Remove(hero);
            return true;
        }
        return false;
    }

    public void RefreshApplicants(bool configurable = false, int amount = 1, int level = 1)
    {
        Applicants.Clear();
        int quantity = 0;
        if (!configurable)
        {
            quantity = 2 + (Level / 2);
        }
        else
        {
            quantity = amount;
        }

        for (int x = 0; x < quantity; x++)
        {
            Hero newHero = new Hero(this, Rng.Rand.Next(1, level + 1));

            //Hero newHero = new Hero(this, Class.Support, 1);
            newHero.Transform = new Transform(World.Map.GetSpawnPosition() + newHero.Transform.Size / 2);

            Applicants.Add(newHero);
        }

        //PrintApplicantsDebug();
    }

    public bool BuyDungeon(Dungeon dungeon)
    {
        if (!OwnedDungeons.Contains(dungeon))
        {
            OwnedDungeons.Add(dungeon);
            AvailableDungeons.Remove(dungeon);
            return true;
        }
        return false;
    }

    public bool RemoveDungeon(Dungeon dungeon)
    {
        if (OwnedDungeons.Contains(dungeon))
        {
            OwnedDungeons.Remove(dungeon);
        }
        return true;
    }
    
    public void RefreshAvailableDungeons(bool configurable, int quantity = 5, int level = 1)
    {
        var d = new Dungeon(World); 
        if (configurable)
        {
            for (int i = 0; i < quantity; i++)
            {
                d = new Dungeon(World, Rng.Rand.Next(1, level + 1));
                AvailableDungeons.Add(d);
            }
            return;
        }
        for (int i = 0; i < quantity; i++)
        {
            d = new Dungeon(World);
            AvailableDungeons.Add(d);
        }
    }

    public void GainXP(int amount)
    {
        XP += amount;
        while (XP >= MaxXP)
        {
            XP -= MaxXP;
            LevelUp();
        }
    }

    public void LevelUp()
    {
        Level++;
    }

    public void PayDay()
    {
        var allHeroes = new List<Hero>();
        allHeroes.AddRange(Heroes);
        foreach(MissionManager mission in Missions)
        {
            allHeroes.AddRange(mission.Party);
        }
        for(int i = allHeroes.Count - 1; i >=0; i--)
        {
            var hero = allHeroes[i];
            if (Gold >= hero.Wage)
            {
                hero.Pay();
            }
            else
            {
                hero.Satisfaction = Math.Max(0, hero.Satisfaction - 20);
            }
            if (hero.Satisfaction <= 0)
            {
                if (Heroes.Contains(hero))
                {
                    Fire(hero);
                }
                else
                {
                    foreach(MissionManager mission in Missions)
                    {
                        if (mission.Party.Contains(hero))
                        {
                            mission.Party.Remove(hero);
                        }
                    }
                }
            }
        }
    }
}