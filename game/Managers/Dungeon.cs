using System;
using System.Collections.Generic;
using System.Linq;

public enum DungeonState
{
    NotStarted,
    Cleaning,
    Resting,
    Collecting,
    Completed,
    Failed
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
    public DungeonRoom CurrentRoom { get; private set; }

    public const int smallDelay = 10;
    public const int timePerTurn = 30;
    public const int timePerRest = 75;
    public const int timePerLoot = 100;
    public const int timePerRoom = 150;

    public DateTime? EndTime { get; private set; }
    public DateTime NextActionTime { get; set; }

    public int PendingXp { get; private set; }
    public int PendingGold { get; private set; }
    public List<string> PendingLogs { get; private set; } = new();

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
        NextActionTime = world.WorldTimeManager.CurrentTime;
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

            list.Add(new Enemy(Level, new(sortedElement)));
        }
        return list;
    }

    public void PrepareDungeon(List<Hero> Party)
    {
        GuildParty = Party;
        int maxEnemies = GuildParty.Count + (Level - GuildParty[0].Level); // TODO: implement a new party system where player can create how much predefinided party he want, and we can calculate the medium level, elements abundance, and particular logs (hero has been added, hero has been removed, party started a dungeon, dungeon.logs, etc...) etc... 
        int enemiesRoom = Rng.Rand.Next(1, maxEnemies + 1);
        int totalEnemies = 0;
        foreach (DungeonRoom room in Rooms)
        {
            room.Enemies = GenerateEnemies(room, enemiesRoom);
            totalEnemies += room.Enemies.Count;
        }

        Logs.Add($"\n=== 🏰 DUNGEON START: {Biome.Type} (Lvl {Level}) ===");
        Logs.Add($"📊 Stats: {Rooms.Count} Salas | {totalEnemies} Inimigos | Party: {GuildParty.Count}");
        Logs.Add($"Exploring...");
    }

    public void UpdateDungeon()
    {
        if (DungeonState == DungeonState.NotStarted ||
        DungeonState == DungeonState.Completed ||
        DungeonState == DungeonState.Failed) return;
        if (World.WorldTimeManager.CurrentTime < NextActionTime) return;

        GuildParty = Mission.Party;
        if (GuildParty.Count == 0)
        {
            Logs.Add("\n💀 A MISSÃO FALHOU. 💀");
            DungeonState = DungeonState.Failed;
            return;
        }

        switch (DungeonState)
        {
            case DungeonState.Cleaning:
                ProcessCleaningState();
                break;
            case DungeonState.Resting:
                ProcessRestingState();
                break;
            case DungeonState.Collecting:
                ProcessCollectingState();
                break;
        }
    }

    private void ProcessCleaningState()
    {
        if (CurrentRoom == null || (CurrentRoom.IsCleared && CurrentRoom.Enemies.All(e => e.HP <= 0)))
        {
            var nextRoom = Rooms.FirstOrDefault(r => !r.IsCleared);
            if (nextRoom == null)
            {
                Logs.Add("\n📦 Iniciando coleta de espólios...");
                DungeonState = DungeonState.Collecting;
                int totalLootTime = Rooms.Count * timePerLoot;
                NextActionTime = World.WorldTimeManager.CurrentTime.AddMinutes(Rng.Rand.Next(totalLootTime / 2, totalLootTime));
                return;
            }

            CurrentRoom = nextRoom;
            int roomIndex = Rooms.IndexOf(CurrentRoom) + 1;

            NextActionTime = roomIndex == 1 ? World.WorldTimeManager.CurrentTime.AddMinutes(1) : World.WorldTimeManager.CurrentTime.AddMinutes(Rng.Rand.Next(timePerRoom / 2, timePerRoom));

            Logs.Add($"\n--- 🚪 Entrando na Sala {roomIndex}/{Rooms.Count} (Explorando...) ---");

            if (CurrentRoom.Enemies.Any(e => e.HP > 0))
            {
                var enemySummary = string.Join(", ", CurrentRoom.Enemies.Select(e => $"{e.Name}"));
                Logs.Add($"⚠️ Inimigos avistados: {enemySummary}");
            }
            return;
        }

        if (CurrentRoom.Enemies.Any(e => e.HP > 0))
        {
            CombatManager.CombatEncounterStep(this, CurrentRoom, GuildParty);
            NextActionTime = World.WorldTimeManager.CurrentTime.AddMinutes(Rng.Rand.Next(timePerTurn / 2, timePerTurn));
        }
        else
        {
            CurrentRoom.IsCleared = true;
            Logs.Add($"✅ Sala Limpa!");
            HandleXpDistribution();
            if(CheckForRest()) return;
        }
    }
    
    private void ProcessRestingState()
    {
        bool someoneRested = false;
        foreach (Hero hero in GuildParty)
        {
            if (hero.IsAlive() && hero.RestCount > 0)
            {
                hero.CombatRest(this);
                someoneRested = true;
            }
        }
        if (someoneRested)
        {
            Logs.Add("💚 A party completou um descanso.");
        }
        DungeonState = DungeonState.Cleaning;
        NextActionTime = World.WorldTimeManager.CurrentTime.AddMinutes(smallDelay);
    }

    private void ProcessCollectingState()
    {
        Logs.Add("📦 Coleta finalizada!");
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

        int totalEnemies = Rooms.Sum(r => r.Enemies.Count);
        int resourcesValue = DungeonResources.CollectResources(totalEnemies * Rng.Rand.Next(10, 200));

        PendingGold = resourcesValue;

        DungeonState = DungeonState.Completed;
        ReturnPartyToGuild();
    }

    public void ReturnPartyToGuild()
    {
        if (Mission != null && Mission.Party.Count > 0)
        {
            var returningHeroes = Mission.Party.ToList();

            foreach (var hero in returningHeroes)
            {
                World.Guild.Hire(hero);
            }

            Mission.Party.Clear();

            Logs.Add("🏠 A equipe retornou para a guilda e aguarda o relatório.");
        }
    }

    public void HandleXpDistribution()
    {
        if (XpReward <= 0) return;
        int totalXp = XpReward;

        foreach (var hero in GuildParty)
        {
            hero.GainXP(totalXp);
            hero.Guild.GainXP(totalXp / GuildParty.Count);
        }
        Logs.Add($"✨ Vitória! Party recebeu {totalXp} XP total");
        XpReward = 0;
    }

    private bool CheckForRest()
    {
        var tiredHeroes = GuildParty.Where(e => e.HP < e.MaxHP * 0.5f || !e.IsMagical ? e.Energy < e.MaxEnergy * 0.5f : e.Mana < e.MaxMana * 0.5f).ToList();
        bool criticalCondition = GuildParty.Any(h => h.HP < h.MaxHP * 0.2f);
        bool majorityTired = tiredHeroes.Count > (GuildParty.Count * 0.5f);

        if (majorityTired || criticalCondition)
        {
            Logs.Add("⛺ A party decidiu montar acampamento...");
            DungeonState = DungeonState.Resting;
            NextActionTime = World.WorldTimeManager.CurrentTime.AddMinutes(Rng.Rand.Next(timePerRest / 2, timePerRest));

            return true;
        }
        return false;
    }

    private string FormatTime(int totalSeconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
        return t.ToString(@"hh\:mm\:ss");
    }

    public void CollectRewards()
    {
        if (PendingGold > 0)
        {
            World.Guild.Gold += PendingGold;
            Logs.Add($"💰 {PendingGold}g adicionados aos cofres da guilda.");
            PendingGold = 0;
        }
        if (DungeonLoot.Count > 0)
        {
            Logs.Add($"🎒 {DungeonLoot.Count} itens transferidos para o estoque.");
            DungeonLoot.Clear();
        }
        Logs.Add("✅ Missão Finalizada com Sucesso.");
        DungeonState = DungeonState.NotStarted;
        World.Guild.OwnedDungeons.Remove(this);
    }

    public void ResetDungeon()
    {
        DungeonState = DungeonState.NotStarted;
        CurrentRoom = null;
        XpReward = 0;
        Logs.Clear();
        PendingLogs.Clear();

        foreach (var room in Rooms)
        {
            room.IsCleared = false;
            room.Enemies.Clear();
            room.RoomLoot.Clear();
        }
    }
}




public class DungeonResource
{
    public List<Resource> Resources = new();

    public DungeonResource(int level)
    {
        int abundanceBudget = (level * 300) + Rng.Rand.Next(10, 100);

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
        if (abundanceBudget > 10)
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