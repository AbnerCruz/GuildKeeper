using System;
using System.Collections.Generic;
using System.Linq;

public enum DungeonState
{
    NotStarted,
    Cleaning,
    Collecting,
    Completed,
}
public class Dungeon
{
    public World World { get; private set; }
    public MissionManager Mission;
    public CombatManager CombatManager = new();
    public int Price;
    public int Level;
    public int XpReward;
    public List<Hero> GuildParty = new();
    public List<Entity> GuildMiners = new(); // For future use to collect resources
    public Biome Biome = new();
    public List<DungeonRoom> Rooms = new();
    DungeonRoom CurrentRoom;

    int TimeToComplete = 0;

    public List<string> Logs = new();

    public List<Equipment> DungeonLoot = new();
    public DungeonResource DungeonResources;

    public DungeonState DungeonState = DungeonState.NotStarted;

    public Dungeon(World world, int level, int roomsCount, Biome biome)
    {
        World = world;
        Level = level;
        Price = level * roomsCount * 100;
        Biome = biome;
        DungeonResources = new(level);

        int realValue = 0;
        foreach (Resource resource in DungeonResources.Resources)
        {
            realValue += resource.Price * resource.Quantity;
        }
        float discountFactor = Rng.Rand.Next(40, 60) / 100f;
        Price = (int)(realValue * discountFactor);
        GenerateRooms(roomsCount);
    }

    public Dungeon(World world, int level, int rooms) : this(world, level, rooms, new()) { }
    public Dungeon(World world) : this(world, Rng.Rand.Next(1, 10), Rng.Rand.Next(1, 10)) { }
    public Dungeon(World world, int level) : this(world, level, Rng.Rand.Next(1, level + 2)) { }

    public void GenerateRooms(int quantity)
    {
        Rooms.Clear();
        for (int i = 0; i < quantity; i++)
        {
            var room = new DungeonRoom();
            Rooms.Add(room);
        }
    }

    public List<Enemy> GenerateEnemies(DungeonRoom room, int enemiesCount)
    {
        var list = new List<Enemy>();
        for (int j = 0; j < enemiesCount; j++)
        {
            var sortedElement = ElementType.None;
            if (Biome.BiomeCurrentElements.Count > 0) sortedElement = Biome.BiomeCurrentElements[Rng.Rand.Next(Biome.BiomeCurrentElements.Count)];
            var randomCreature = Biome.GetRandomNativeCreature();
            var randomClass = Rng.RandEnum<Class>();

            list.Add(new Enemy(Level));
        }
        return list;
    }
    public void DungeonCycle()
    {
        GuildParty = Mission.Party;

        int maxEnemies = Math.Min(6, Level + GuildParty.Count - 1);
        int enemiesRoom = Rng.Rand.Next(1, maxEnemies + 1);
        int totalEnemies = 0;
        foreach (DungeonRoom room in Rooms)
        {
            room.Enemies = GenerateEnemies(room, enemiesRoom);
            totalEnemies += room.Enemies.Count;
        }
        TimeToComplete = Rng.Rand.Next(totalEnemies * 10, totalEnemies * 40);

        Logs.Add($"\n=== 🏰 DUNGEON START: {Biome.Type} (Lvl {Level}) ===");
        Logs.Add($"📊 Stats: {Rooms.Count} Salas | {totalEnemies} Inimigos | Party: {GuildParty.Count}");
        while (DungeonState != DungeonState.Completed && DungeonState != DungeonState.NotStarted)
        {
            GuildParty.RemoveAll(h => h.HP <= 0);
            if (GuildParty.All(h => h.HP <= 0) || GuildParty.Count == 0)
            {
                Logs.Add("\n💀💀 A Morte chegou para todos... MISSÃO FALHOU. 💀💀");
                DungeonState = DungeonState.NotStarted;
                break;
            }
            if (DungeonState == DungeonState.Cleaning)
            {
                var nextRoom = Rooms.FirstOrDefault(r => !r.IsCleared);

                if (nextRoom == null)
                {
                    DungeonState = DungeonState.Collecting;
                    continue;
                }

                CurrentRoom = nextRoom;
                int roomIndex = Rooms.IndexOf(CurrentRoom) + 1;
                CurrentRoom.Enemies.RemoveAll(e => e.HP <= 0);

                Logs.Add($"\n--- 🚪 Entrando na Sala {roomIndex}/{Rooms.Count} ---");
                if (CurrentRoom.Enemies.Any(e => e.HP > 0))
                {
                    var enemySummary = string.Join(", ", CurrentRoom.Enemies.Select(e => $"{e.Name}"));
                    Logs.Add($"⚠️ COMBATE: {enemySummary}");
                }
                while (CurrentRoom.Enemies.Any(e => e.HP > 0))
                {
                    CombatManager.CombatEncounter(this, CurrentRoom, GuildParty);

                    if (GuildParty.All(h => h.HP <= 0)) break;
                }

                if (GuildParty.Any(h => h.HP > 0))
                {
                    CurrentRoom.IsCleared = true;
                    CurrentRoom.Enemies.RemoveAll(e => e.HP <= 0);
                    Logs.Add($"✅ Sala {roomIndex} Limpa!");

                    int totalXp = XpReward;
                    if (GuildParty.Count > 0)
                    {
                        foreach (var hero in GuildParty)
                        {
                            hero.GainXP(totalXp);
                            hero.Guild.GainXP(totalXp/GuildParty.Count);
                        }
                        Logs.Add($"✨ Vitória! Party recebeu {totalXp} XP total");
                    }
                    XpReward = 0;
                    foreach (var hero in GuildParty)
                    {
                        hero.Heal(hero.Constitution / 4);
                        // hero.Energy = Math.Min(hero.MaxEnergy, hero.Energy + (hero.Constitution / 4));
                        // hero.Mana = Math.Min(hero.MaxMana, hero.Mana + (hero.Wisdom / 4));
                        // hero.Heal(hero.Constitution / 4);
                    }
                }
            }

            else if (DungeonState == DungeonState.Collecting)
            {
                Logs.Add("\n📦 Coletando espólios das salas...");
                int itemsFound = 0;
                foreach (var room in Rooms)
                {
                    if (room.RoomLoot.Count > 0)
                    {
                        DungeonLoot.AddRange(room.RoomLoot);
                        itemsFound += room.RoomLoot.Count;
                        room.RoomLoot.Clear();
                    }
                }
                int resourcesValue = DungeonResources.CollectResources(totalEnemies * Rng.Rand.Next(10, 200));
                World.Guild.Gold += resourcesValue;
                Logs.Add($"💰 Venda de Recursos: {resourcesValue}g | Itens: {itemsFound}");
                DungeonState = DungeonState.Completed;
                Logs.Add("🏆 === DUNGEON COMPLETADA COM SUCESSO! ===\n");
            }
        }
        foreach (var log in Logs)
        {
            Console.WriteLine(log);
        }
        UIController.WorldInstance.BuildUI();
    }
}



public class DungeonResource
{
    public List<Resource> Resources = new();

    public DungeonResource(int level)
    {
        int abundanceBudget = (level * 100) + Rng.Rand.Next(10, 40);

        for (int i = 0; i < Economy.Resources.Count; i++)
        {
            var template = Economy.Resources[i];
            if ((i * 2) >= level + 1) continue;

            if (template.Price > abundanceBudget) continue;

            int maxQuantity = abundanceBudget / template.Price;

            int generatedAmount = Rng.Rand.Next(1, maxQuantity + 1);

            if (generatedAmount > 0)
            {
                var newResource = new Resource(template.Type, template.Price);
                newResource.Quantity = generatedAmount;
                Resources.Add(newResource);

                abundanceBudget -= (generatedAmount * template.Price);
            }
        }
        if(abundanceBudget > 10)
        {
            var iron = Resources.FirstOrDefault(r => r.Type == IResource.Iron);
            if (iron != null)
            {
                iron.Quantity += abundanceBudget / iron.Price;
            }
            else
            {
                Resources.Add(new Resource(IResource.Iron, Economy.Resources[0].Price) { Quantity = 5 });
            }
        }
        if (Resources.Count == 0)
        {
            var template = Economy.Resources[0];
            Resources.Add(new Resource(template.Type, template.Price) { Quantity = Rng.Rand.Next(1, 3) });
        }

    }

    public int CollectResources(int bonus)
    {
        int sum = 0;
        foreach (var resource in Resources)
        {
            sum += resource.Quantity * resource.Price;
        }
        return sum + (int)(bonus * 0.2f);
    }
}

public class DungeonRoom
{
    public bool IsCleared;
    public List<Enemy> Enemies;
    public List<Equipment> RoomLoot = new();
}