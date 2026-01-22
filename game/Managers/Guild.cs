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
        Name = name;
        Gold = 0;
    }

    public bool Hire(Hero hero)
    {
        if (!Heroes.Contains(hero))
        {
            hero.Guild = this;
            World.Instantiate(hero);
            Heroes.Add(hero);
            World.UIController.RefreshInfo();
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
            World.UIController.RefreshInfo();
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

        World.UIController.RefreshInfo();
    }

    public bool BuyDungeon(Dungeon dungeon)
    {
        if (!OwnedDungeons.Contains(dungeon))
        {
            OwnedDungeons.Add(dungeon);
            AvailableDungeons.Remove(dungeon);
            World.UIController.RefreshInfo();
            return true;
        }
        World.UIController.RefreshInfo();
        return false;
    }

    public bool RemoveDungeon(Dungeon dungeon)
    {
        if (OwnedDungeons.Contains(dungeon))
        {
            OwnedDungeons.Remove(dungeon);
            World.UIController.RefreshInfo();
            return true;
        }
        World.UIController.RefreshInfo();
        return false;
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
            World.UIController.RefreshInfo();
            return;
        }
        for (int i = 0; i < quantity; i++)
        {
            d = new Dungeon(World);
            AvailableDungeons.Add(d);
        }
        World.UIController.RefreshInfo();
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
        bool rosterChanged = false; // Flag para saber se precisamos redesenhar a UI

        // Usamos HashSet para evitar pagar o mesmo herói duas vezes se ele estiver bugado em 2 listas
        var allHeroes = new HashSet<Hero>();
        allHeroes.UnionWith(Heroes);

        foreach (MissionManager mission in Missions)
        {
            allHeroes.UnionWith(mission.Party);
        }

        var heroesList = allHeroes.ToList();

        if (heroesList.Count > 0)
        {
            for (int i = heroesList.Count - 1; i >= 0; i--)
            {
                var hero = heroesList[i];

                if (Gold >= hero.Wage)
                {
                    Gold -= hero.Wage;
                    hero.Pay();
                    Console.WriteLine($"[SISTEMA] Pagamento realizado para {hero.Name}");
                }
                else
                {
                    hero.UnPayed();
                    rosterChanged = true;
                    Console.WriteLine($"[SISTEMA] Falha no pagamento de {hero.Name}. Satisfação: {hero.Satisfaction}");
                }

                if (hero.Satisfaction <= 0)
                {
                    Console.WriteLine($"{hero.Name} cansou de ser desrespeitado e foi embora.");
                    rosterChanged = true;

                    if (Heroes.Contains(hero))
                    {
                        World.Destroy(hero);
                        Heroes.Remove(hero);
                    }
                    else
                    {
                        foreach (MissionManager mission in Missions)
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

        if (rosterChanged)
        {
            World.UIController.RefreshInfo();
        }
    }
}