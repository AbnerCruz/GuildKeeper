using System;
using System.Linq;
using System.Collections.Generic;

public class CombatManager
{
    Dungeon Dungeon;
    public struct AttackResult
    {
        public int FinalDamage;
        public bool IsCritical;
        public bool IsHit;
        public string LogMessage;
    }

    public void CombatEncounter(Dungeon dungeon, DungeonRoom room, List<Hero> GuildParty)
    {
        Dungeon = dungeon;
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
                            Dungeon.Logs.Add("💀 Todos os aliados morreram.");
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
                                Dungeon.Logs.Add($"🏰{interceptor.Name} interceptou o ataque destinado a {target.Name}!");
                                target = interceptor;
                            }
                        }
                        var attack = CalculateAttack(enemy, target, enemy.Damage, enemy.Element.Type);

                        if (attack.IsHit)
                        {
                            int damage = target.TakeDamage(attack.FinalDamage);
                            string critText = attack.IsCritical ? "Crit:" : "";
                            Dungeon.Logs.Add($"🛑[{enemy.Initiative}]{enemy.Name} {critText} ⚔️ {target.Name} : -{damage} HP (HP: {target.HP})");
                            if (target.Class == Class.Tank)
                            {
                                var thorn = Math.Max(1, (int)(damage * 0.25));
                                enemy.TakeDamage(thorn);
                                Dungeon.Logs.Add($"Espinhos eficientes! {target.Name} devolveu -{thorn}HP em [{enemy.Initiative}]{enemy.Name}");
                            }
                            CheckEnemyDeath(enemy);

                        }
                        else
                        {
                            Dungeon.Logs.Add("🛑" + attack.LogMessage);
                        }
                        if (target.HP <= 0)
                        {
                            Dungeon.Logs.Add($"💀💀 {target.Name} caiu em combate! 💀💀");
                        }
                        break;
                    case Class.Support:
                        var woundedAlly = room.Enemies.FirstOrDefault(e => e != enemy && e.HP > 0 && e.HP < e.MaxHP);

                        if (woundedAlly != null)
                        {
                            var heal = CalculateHeal(enemy, enemy.Damage);
                            if (heal.IsHit)
                            {
                                int healAmount = heal.FinalDamage;
                                string critText = heal.IsCritical ? "Crit:" : "";
                                woundedAlly.HP = (woundedAlly.HP >= woundedAlly.MaxHP) ? woundedAlly.MaxHP : woundedAlly.HP + healAmount;
                                Dungeon.Logs.Add($"🛑[{enemy.Initiative}]{enemy.Name} {critText} 💚 {woundedAlly.Name} : +{healAmount} HP");
                                continue;
                            }
                            else
                            {
                                Dungeon.Logs.Add($"🛑[{enemy.Initiative}] " + heal.LogMessage);
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
                                    Dungeon.Logs.Add($"🛑[{interceptor.Initiative}]{interceptor.Name} entrou na frente do ataque em {target.Name}!");
                                    target = interceptor;
                                }
                            }

                            int energyCost = (hero.Class == Class.Mage || hero.Class == Class.Support) ? 2 : 1;
                            if (hero.Energy > 0)
                            {
                                hero.Energy--;
                            }
                            else
                            {
                                if (hero.Stress()) Dungeon.Logs.Add($"🏰[{hero.Initiative}]{hero.Name} está estressado!");
                            }

                            var attack = CalculateAttack(hero, target, hero.PhysicalDamage, hero.Element.Type);
                            if (attack.IsHit)
                            {
                                int damage = target.TakeDamage(attack.FinalDamage);
                                string critText = attack.IsCritical ? "Crit:" : "";
                                if (target.Class == Class.Tank) hero.TakeDamage(1);
                                Dungeon.Logs.Add($"🏰[{hero.Initiative}]{hero.Name} {critText} ⚔️ [{target.Initiative}]{target.Name} : -{damage} HP (HP: {target.HP})");
                                CheckEnemyDeath(target);
                            }
                            else
                            {
                                Dungeon.Logs.Add("🏰" + attack.LogMessage);
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
                                    Dungeon.Logs.Add($"🛑[{interceptor.Initiative}]{interceptor.Name} protegeu {target.Name} do feitiço!");
                                    target = interceptor;
                                }
                            }
                            if (hero.Mana > 0)
                            {
                                var attack = CalculateAttack(hero, target, hero.MagicPower, hero.Element.Type);
                                if (attack.IsHit)
                                {
                                    int damageText = target.TakeDamage(attack.FinalDamage);
                                    string critText = attack.IsCritical ? "Crit:" : "";
                                    hero.Mana--;
                                    Dungeon.Logs.Add($"🏰[{hero.Initiative}]{hero.Name} {critText} 🔥 {target.Name} : -{damageText} HP (HP: {target.HP})");
                                    CheckEnemyDeath(target);
                                }
                                else
                                {
                                    Dungeon.Logs.Add("🏰" + attack.LogMessage);
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
                                var heal = CalculateHeal(hero, hero.MagicPower);
                                if (heal.IsHit)
                                {
                                    int healAmount = heal.FinalDamage;
                                    string critText = heal.IsCritical ? "Crit:" : "";
                                    int healed = lowestAlly.Heal(healAmount);
                                    hero.Mana--;
                                    Dungeon.Logs.Add($"🏰[{hero.Initiative}]{hero.Name} {critText} 💚 {lowestAlly.Name} : +{healed} HP");
                                    continue;
                                }
                                else
                                {
                                    Dungeon.Logs.Add($"🏰[{hero.Initiative}] " + heal.LogMessage);
                                }
                            }
                            else
                            {
                                goto default;
                            }
                        }
                        else
                        {
                            var supportTargets = room.Enemies.Where(e => e.HP > 0).ToList();
                            if (supportTargets.Count > 0)
                            {
                                Enemy target = supportTargets.OrderBy(e => e.HP).ThenBy(e => e.Armour).FirstOrDefault();

                                if (target.Class != Class.Tank)
                                {
                                    var enemyTanks = room.Enemies.Where(e => e.Class == Class.Tank && e.HP > 0).ToList();
                                    if (enemyTanks.Count > 0 && Rng.Rand.NextDouble() < 0.20)
                                    {
                                        var interceptor = enemyTanks[Rng.Rand.Next(enemyTanks.Count)];
                                        Dungeon.Logs.Add($"🛑[{interceptor.Initiative}]{interceptor.Name} protegeu {target.Name} do feitiço!");
                                        target = interceptor;
                                    }
                                }
                                if (hero.Mana > 0)
                                {
                                    var attack = CalculateAttack(hero, target, hero.MagicPower, hero.Element.Type);
                                    if (attack.IsHit)
                                    {
                                        int damageText = target.TakeDamage(attack.FinalDamage);
                                        string critText = attack.IsCritical ? "Crit:" : "";
                                        hero.Mana--;
                                        Dungeon.Logs.Add($"🏰[{hero.Initiative}]{hero.Name} {critText} 🔥 {target.Name} : -{damageText} HP (HP: {target.HP})");
                                        CheckEnemyDeath(target);
                                    }
                                    else
                                    {
                                        Dungeon.Logs.Add("🏰" + attack.LogMessage);
                                    }
                                }
                                else
                                {
                                    goto default;
                                }

                            }
                        }
                        break;
                }
            }
        }
    }

    public AttackResult CalculateAttack(Entity attacker, Entity target, int baseDamage, ElementType element)
    {
        var result = new AttackResult();

        int attackRoll = attacker.RollDice();
        int totalHit = attackRoll + attacker.GetAccuracy();

        int defenseRating = 10 + target.GetEvasion();
        if (attackRoll <= 5)
        {
            result.IsHit = false;
            result.LogMessage = $"⛓️‍💥 {attacker.Name} errou o ataque! (Roll: {attackRoll})";
            return result;
        }

        if (totalHit >= defenseRating)
        {
            result.IsHit = true;
        }
        else
        {
            result.IsHit = false;
            result.LogMessage = $"[{attacker.Name}] errou! ({totalHit} vs {defenseRating})";
            return result;
        }


        int critThreshold = attacker.GetCritThreshold();
        var dice = attacker.RollDice();
        bool isCrit = dice >= critThreshold;
        float multiplier = 1f;

        if (isCrit)
        {
            multiplier = 1.5f;
        }
        else if (dice <= 10)
        {
            multiplier = 0.5f;
        }
        int damageDealt = 0;
        switch (target)
        {
            case Enemy:
                Enemy e = (Enemy)target;
                damageDealt = target.CalculateMitigation(baseDamage * multiplier, element, e.Element, e.Armour);
                break;
            case Hero:
                Hero h = (Hero)target;
                damageDealt = target.CalculateMitigation(baseDamage * multiplier, element, h.Element, h.Armour);
                break;
        }
        result.FinalDamage = damageDealt;
        result.IsCritical = isCrit;

        return result;
    }

    public AttackResult CalculateHeal(Entity healer, int baseHeal)
    {
        var result = new AttackResult();
        result.IsHit = healer.RollDice() > 25;
        result.LogMessage = result.IsHit ? "" : $"⛓️‍💥 {healer.Name} não conseguiu se concentrar";

        int critThreshold = healer.GetCritThreshold();
        bool isCrit = healer.RollDice() >= critThreshold;

        float multiplier = isCrit ? 2.0f : 1.0f;

        result.FinalDamage = (int)(baseHeal * multiplier);
        result.IsCritical = isCrit;

        return result;
    }

    private void CheckEnemyDeath(Enemy target)
    {
        if (target.HP <= 0)
        {
            int XPsum = target.Level * 50;
            Dungeon.XpReward += XPsum;
            Dungeon.Logs.Add($"☠️ [{target.Initiative}]{target.Name} foi derrotado! (+{XPsum} XP acumulado)");
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