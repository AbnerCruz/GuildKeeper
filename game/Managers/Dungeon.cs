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
    public Dungeon(World world, int level) : this(world, level, Rng.Rand.Next(1, level + 1)) { }

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
            Elements sortedElement = Biome.BiomeCurrentElements[Rng.Rand.Next(Biome.BiomeCurrentElements.Count)];
            var randomClass = Enum.GetValues<Class>()[Rng.Rand.Next(0, Enum.GetValues<Class>().Length)];
            Creatures randomCreature = Biome.GetRandomNativeCreature();

            list.Add(new Enemy(Level, GuildParty.Count, randomCreature, randomClass, sortedElement, sortedElement));
        }
        return list;
    }
    public void DungeonCycle()
    {
        GuildParty = Mission.Party;

        int maxEnemies = Math.Min(6, 1 + (int)((Level + GuildParty.Count - 1) / 2));
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
                    var enemySummary = string.Join(", ", CurrentRoom.Enemies.Select(e => $"{e.Race}({e.DamageElement})"));
                    Logs.Add($"⚠️ COMBATE: {enemySummary}");
                }
                while (CurrentRoom.Enemies.Any(e => e.HP > 0))
                {
                    CombatEncounter(CurrentRoom);

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
                        int xpPerHero = totalXp / GuildParty.Count;
                        foreach (var hero in GuildParty)
                        {
                            hero.GainXP(xpPerHero);
                            hero.Guild.GainXP(xpPerHero);
                        }
                        Logs.Add($"✨ Vitória! Party recebeu {totalXp} XP total ({xpPerHero} por herói).");
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
    public void CombatEncounter(DungeonRoom room)
    {
        var allCombatants = new List<Entity>();
        allCombatants.AddRange(GuildParty.Where(h => h.HP > 0));
        allCombatants.AddRange(room.Enemies.Where(e => e.HP > 0));

        allCombatants = allCombatants.OrderByDescending(c =>
            (c is Hero h) ? h.Initiative : ((Enemy)c).Initiative
        ).ToList();

        foreach (Entity combatant in allCombatants)
        {
            if (combatant is Hero h && h.HP <= 0) continue;
            if (combatant is Enemy e && e.HP <= 0) continue;

            if (combatant is Enemy enemy)
            {
                switch (enemy.Class)
                {
                    default:
                        var validTargets = GuildParty.Where(h => h.HP > 0).ToList();
                        if (validTargets.Count == 0)
                        {
                            Logs.Add("💀 Todos os aliados morreram.");
                            break;
                        }

                        Hero target = null;

                        float roll = (float)Rng.Rand.NextDouble();

                        if (roll < 0.4f)
                        {
                            target = validTargets.OrderBy(h => GetClassPriority(h.Class)).FirstOrDefault();
                        }

                        if (target == null)
                        {
                            target = validTargets[Rng.Rand.Next(0, validTargets.Count)];
                        }

                        if (target.Class != Class.Tank)
                        {
                            var tanks = validTargets.Where(h => h.Class == Class.Tank && h.HP > 0).ToList();
                            if (tanks.Count > 0 && Rng.Rand.NextDouble() < 0.20)
                            {
                                var interceptor = tanks[Rng.Rand.Next(tanks.Count)];
                                Logs.Add($"🏰{interceptor.Name} (Tank) interceptou o ataque destinado a {target.Name}!");
                                target = interceptor;
                            }
                        }
                        var attack = CombatManager.CalculateAttack(enemy, target, enemy.Damage, enemy.DamageElement);

                        if (attack.IsHit)
                        {
                            int damage = target.TakeDamage(attack.FinalDamage);
                            string critText = attack.IsCritical ? "Crit:" : "";
                            Logs.Add($"🛑[{enemy.Initiative}]{enemy.Name}-{enemy.DamageElement} {critText} ⚔️ {target.Name} ({target.Class}) : -{damage} HP (HP: {target.HP})");
                            if (target.Class == Class.Tank) {
                                var thorn = Math.Max(1, (int)(damage * 0.25));
                                enemy.TakeDamage(thorn);
                                Logs.Add($"Espinhos eficientes! {target.Name} ({target.Class}) devolveu -{thorn}HP em [{enemy.Initiative}]{enemy.Name} {enemy.DamageElement}");
                            }
                            CheckEnemyDeath(enemy);

                        }
                        else
                        {
                            Logs.Add("🛑"+attack.LogMessage);
                        }
                        if (target.HP <= 0)
                        {
                            Logs.Add($"💀💀 {target.Name} ({target.Class}) caiu em combate! 💀💀");
                        }
                        break;
                    case Class.Support:
                        var woundedAlly = room.Enemies.FirstOrDefault(e => e != enemy && e.HP > 0 && e.HP < e.MaxHP);

                        if (woundedAlly != null)
                        {
                            var heal = CombatManager.CalculateHeal(enemy, enemy.Damage);
                            if (heal.IsHit)
                            {
                                int healAmount = heal.FinalDamage;
                                string critText = heal.IsCritical ? "Crit:" : "";
                                woundedAlly.HP = (woundedAlly.HP >= woundedAlly.MaxHP) ? woundedAlly.MaxHP : woundedAlly.HP + healAmount;
                                Logs.Add($"🛑[{enemy.Initiative}]{enemy.Name} {critText} 💚 {woundedAlly.Race} ({woundedAlly.Class}) : +{healAmount} HP");
                                continue;
                            }
                            else
                            {
                                Logs.Add($"🛑[{enemy.Initiative}] " + heal.LogMessage);
                            }
                        }
                        else
                        {
                            goto default;
                        }
                        break;
                }
            }

            else if (combatant is Hero hero)
            {
                switch (hero.Class)
                {
                    default:
                        var physicalTargets = room.Enemies.Where(e => e.HP > 0).ToList();
                        if (physicalTargets.Count > 0)
                        {
                            Enemy target = physicalTargets.OrderBy(e => e.HP).ThenBy(e => e.Armour).FirstOrDefault();
                            if (target.Class != Class.Tank)
                            {
                                var enemyTanks = room.Enemies.Where(e => e.Class == Class.Tank && e.HP > 0).ToList();
                                if (enemyTanks.Count > 0 && Rng.Rand.NextDouble() < 0.20)
                                {
                                    var interceptor = enemyTanks[Rng.Rand.Next(enemyTanks.Count)];
                                    Logs.Add($"🛑[{interceptor.Initiative}]{interceptor.Name} {interceptor.DamageElement} entrou na frente do ataque em {target.Race}!");
                                    target = interceptor;
                                }
                            }
                            Elements dmgType = hero.Damages[Rng.Rand.Next(0, hero.Damages.Count)];

                            int energyCost = (hero.Class == Class.Mage || hero.Class == Class.Support) ? 2 : 1;
                            if (hero.Energy > 0)
                            {
                                hero.Energy--;
                            }
                            else
                            {
                                if (hero.Stress()) Logs.Add($"🏰[{hero.Initiative}]{hero.Name}-({dmgType}) está estressado!");
                            }

                            var attack = CombatManager.CalculateAttack(hero, target, hero.PhysicalDamage, dmgType);
                            if (attack.IsHit)
                            {
                                int damage = target.TakeDamage(attack.FinalDamage);
                                string critText = attack.IsCritical ? "Crit:" : "";
                                //if (target.Class == Class.Tank) hero.TakeDamage((int)(damage * 0.25));
                                Logs.Add($"🏰[{hero.Initiative}]{hero.Name}-({dmgType}) {critText} ⚔️ [{target.Initiative}]{target.Name}-({target.DamageElement}) : -{damage} HP (HP: {target.HP})");
                                CheckEnemyDeath(target);
                            }
                            else
                            {
                                Logs.Add("🏰" + attack.LogMessage);
                            }
                        }
                        break;
                    case Class.Mage:
                        var magicTargets = room.Enemies.Where(e => e.HP > 0).ToList();
                        if (magicTargets.Count > 0)
                        {
                            Enemy target = magicTargets.OrderBy(e => e.HP).ThenBy(e => e.Armour).FirstOrDefault();

                            if (target.Class != Class.Tank)
                            {
                                var enemyTanks = room.Enemies.Where(e => e.Class == Class.Tank && e.HP > 0).ToList();
                                if (enemyTanks.Count > 0 && Rng.Rand.NextDouble() < 0.20)
                                {
                                    var interceptor = enemyTanks[Rng.Rand.Next(enemyTanks.Count)];
                                    Logs.Add($"🛑[{interceptor.Initiative}]{interceptor.Name} {interceptor.DamageElement} protegeu {target.Name} do feitiço!");
                                    target = interceptor;
                                }
                            }

                            Elements dmgType = hero.Damages[Rng.Rand.Next(0, hero.Damages.Count)];

                            if (hero.Mana > 0)
                            {
                                var attack = CombatManager.CalculateAttack(hero, target, hero.MagicPower, dmgType);
                                if (attack.IsHit)
                                {
                                    int damageText = target.TakeDamage(attack.FinalDamage);
                                    string critText = attack.IsCritical ? "Crit:" : "";
                                    hero.Mana--;
                                    Logs.Add($"🏰[{hero.Initiative}]{hero.Name}-({dmgType}) {critText} 🔥 {target.Name}-({target.DamageElement}) : -{damageText} HP (HP: {target.HP})");
                                    CheckEnemyDeath(target);
                                }
                                else
                                {
                                    Logs.Add("🏰"+attack.LogMessage);
                                }
                            }
                            else
                            {
                                goto default;
                            }

                        }
                        break;
                    case Class.Support:
                        var lowestAlly = GuildParty.Where(a => a != hero && a.HP > 0 && a.HP < a.MaxHP).OrderBy(a => a.HP).FirstOrDefault();

                        if (lowestAlly != null)
                        {
                            if (hero.Mana > 0)
                            {
                                var heal = CombatManager.CalculateHeal(hero, hero.MagicPower);
                                if (heal.IsHit)
                                {
                                    int healAmount = heal.FinalDamage;
                                    string critText = heal.IsCritical ? "Crit:" : "";
                                    int healed = lowestAlly.Heal(healAmount);
                                    hero.Mana--;
                                    Logs.Add($"🏰[{hero.Initiative}]{hero.Name} {critText} 💚 {lowestAlly.Name} : +{healed} HP");
                                    continue;
                                }
                                else
                                {
                                    Logs.Add($"🏰[{hero.Initiative}] " + heal.LogMessage);
                                }
                            }
                            else
                            {
                                goto default;
                            }
                        }
                        else
                        {
                            goto default;
                        }
                        break;
                }
            }
        }
    }

    private void CheckEnemyDeath(Enemy target)
    {
        if (target.HP <= 0)
        {
            int XPsum = target.Level * 50;
            XpReward += XPsum;
            Logs.Add($"☠️ [{target.Initiative}]{target.Name} foi derrotado! (+{XPsum} XP acumulado)");
        }
    }

    public int GetClassPriority(Class c)
    {
        return c switch
        {
            Class.Tank => 0,
            Class.Warrior => 1,
            Class.Ranger => 2,
            _ => 3,
        };
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