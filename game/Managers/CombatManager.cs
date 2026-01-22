using System;
using System.Linq;
using System.Collections.Generic;


public class CombatManager
{
    public CombatContext Context = new();
    public class CombatContext
    {
        public Dungeon Dungeon { get; set; }
        public DungeonRoom Room { get; set; }
        public List<Hero> Party { get; set; }

        public List<Entity> AlliesOf(Entity entity)
        {
            return entity is Hero ? Party.Cast<Entity>().ToList() : Room.Enemies.Cast<Entity>().ToList();
        }

        public List<Entity> EnemiesOf(Entity entity)
        {
            return entity is Hero ? Room.Enemies.Cast<Entity>().ToList() : Party.Cast<Entity>().ToList();
        }
    }

    public interface ICombatBehavior
    {
        void ExecuteTurn(Entity actor, CombatContext context);
    }

    public abstract class BaseCombatBehavior : ICombatBehavior
    {
        public struct AttackResult
        {
            public int FinalDamage;
            public bool IsCritical;
            public bool IsHit;
            public string LogMessage;
        }

        public virtual void ExecuteTurn(Entity actor, CombatContext context)
        {
            var targets = context.EnemiesOf(actor).Where(e => e.HP > 0).ToList();
            if (targets.Count == 0) return;

            Entity target = SelectedTarget(actor, targets);
            target = HandleTankInterception(target, context);

            PerformAttack(actor, target, context);
            actor.HandleResources(context.Dungeon);
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
                multiplier = 0.8f;
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
            if (isCrit) result.FinalDamage = damageDealt > 1 ? damageDealt : 2;
            else result.FinalDamage = damageDealt;
            result.IsCritical = isCrit;

            return result;
        }

        public AttackResult CalculateHeal(Entity healer, int baseHeal)
        {
            var result = new AttackResult();

            bool concentrationCheck = healer.RollDice() > 30;

            int critThreshold = healer.GetCritThreshold();
            bool isCrit = healer.RollDice() >= critThreshold;

            float multiplier = 1.0f;

            if (isCrit)
            {
                multiplier = 2.0f;
                result.IsCritical = true;
            }
            else
            {
                multiplier = 0.5f;
                result.LogMessage = $"💦 {healer.Name} se distraiu e curou pouco...";
            }

            result.FinalDamage = Math.Max(1, (int)(baseHeal * multiplier));
            result.IsHit = true;

            return result;
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

        protected Entity SelectedTarget(Entity actor, List<Entity> targets)
        {
            if (targets == null || targets.Count == 0) return null;
            if (actor.RollDice() > 45)
            {
                return targets
                    .OrderBy(h => GetClassPriority(h.Class))
                    .ThenBy(e => e.HP)
                    .ThenBy(e => e.Armour)
                    .FirstOrDefault();
            }
            return targets[Rng.Rand.Next(targets.Count)];
        }

        protected Entity HandleTankInterception(Entity originalTarget, CombatContext context)
        {
            if (originalTarget.Class == Class.Tank) return originalTarget;

            var alliesOfTarget = context.AlliesOf(originalTarget).Where(e => e.HP > 0).ToList();

            var alliesTanks = alliesOfTarget.Where(e => e.Class == Class.Tank).ToList();

            if (alliesTanks.Count > 0 && Rng.Rand.Next(0, 100) > 75)
            {
                var interceptor = alliesTanks[Rng.Rand.Next(0, alliesTanks.Count)];
                bool isExhausted = (interceptor is Hero h && h.Energy <= 0 || interceptor is Enemy e && e.Energy <= 0);

                if (!isExhausted)
                {
                    string icon = interceptor is Hero ? "🏰" : "🛑";
                    context.Dungeon.Logs.Add($"{icon}[{interceptor.Initiative}]{interceptor.Name} interceptou o ataque em {originalTarget.Name}!");
                    return interceptor;
                }
                return originalTarget;
            }
            return originalTarget;
        }

        protected void PerformAttack(Entity attacker, Entity target, CombatContext context)
        {
            int dmg = 0;

            if (attacker is Hero h) dmg = h.IsMagical && h.Mana > 0 ? h.MagicPower : h.PhysicalDamage;
            else if (attacker is Enemy e) dmg = e.IsMagical && e.Mana > 0 ? e.MagicPower : e.PhysicalDamage;

            var attack = CalculateAttack(attacker, target, dmg, attacker.Element.Type);

            if (attack.IsHit)
            {
                int damageTaken = target.TakeDamage(attack.FinalDamage);
                string critText = attack.IsCritical ? "Crit:" : "";
                string icon = attacker is Hero ? "🏰" : "🛑";
                string attackIcon = attacker.IsMagical && attacker.Mana > 0 ? "🔥" : "⚔️";
                context.Dungeon.Logs.Add($"{icon}{critText} {attacker.Name} {attackIcon} {target.Name} | HP: {target.HP}/{target.MaxHP}");

                if (target.Class == Class.Tank)
                {
                    var thorn = Math.Max(1, (int)(damageTaken * 0.20));
                    if (target.RollDice() > 50 && !attacker.IsMagical)
                    {
                        attacker.TakeDamage(thorn);
                        context.Dungeon.Logs.Add($"Espinhos! {target.Name} devolveu -{thorn}HP em {attacker.Name}");
                    } 
                }
                if (attacker.Class == Class.Ranger) attacker.Heal(2);
            }
            else
            {
                string icon = attacker is Hero ? "🏰" : "🛑";
                context.Dungeon.Logs.Add(icon + attack.LogMessage);
            }
        }
    }

    public static class CombatStrategyFactory
    {
        public class AggressiveBehavior : BaseCombatBehavior
        {
            public override void ExecuteTurn(Entity actor, CombatContext context)
            {
                base.ExecuteTurn(actor, context);
            }
        }
        public class RangerBehavior : BaseCombatBehavior
        {
            public override void ExecuteTurn(Entity actor, CombatContext context)
            {
                base.ExecuteTurn(actor, context);
            }
        }
        public class MagicBehavior : BaseCombatBehavior
        {
            public override void ExecuteTurn(Entity actor, CombatContext context)
            {
                base.ExecuteTurn(actor, context);
            }
        }
        public class SupportBehavior : BaseCombatBehavior
        {
            public override void ExecuteTurn(Entity actor, CombatContext context)
            {
                var allies = context.AlliesOf(actor).Where(a => a.HP > 0 && a.HP < a.MaxHP).ToList();
                var lowestAlly = allies.OrderBy(a => a.HP).FirstOrDefault();

                bool shouldHeal = false;
                if(lowestAlly != null)
                {
                    var roll = Rng.Rand.Next(0, 100);
                    int threshold = (lowestAlly == actor) ? 60 : 30;
                    shouldHeal = actor.RollDice() > threshold;
                }

                if (lowestAlly != null && shouldHeal)
                {
                    int dmg = 0;
                    if (actor is Hero h) dmg = h.MagicPower;
                    else if (actor is Enemy e) dmg = e.MagicPower;
                    var heal = CalculateHeal(actor, dmg);

                    if (heal.IsHit)
                    {
                        int healAmount = heal.FinalDamage;
                        string critText = heal.IsCritical ? "Crit:" : "";
                        lowestAlly.Heal(healAmount);
                        context.Dungeon.Logs.Add($"💚 {actor.Name} curou {lowestAlly.Name}...");
                    }
                    else
                    {
                        context.Dungeon.Logs.Add(heal.LogMessage);
                    }
                }
                else
                {
                    base.ExecuteTurn(actor, context);
                }
            }
        }

        private static readonly Dictionary<Class, ICombatBehavior> _strategies = new()
        {
            { Class.Warrior, new AggressiveBehavior() }, // Você cria essa classe herdando de Base
            { Class.Tank,    new AggressiveBehavior() },
            { Class.Ranger,  new RangerBehavior() },
            { Class.Mage,    new MagicBehavior() },      // Lógica de magia
            { Class.Support, new SupportBehavior() }
        };

        public static ICombatBehavior GetBehavior(Class c)
        {
            return _strategies.ContainsKey(c) ? _strategies[c] : _strategies[Class.Warrior];
        }
    }



    public void CombatEncounter(Dungeon dungeon, DungeonRoom room, List<Hero> GuildParty)
    {
        Context = new CombatContext
        {
            Dungeon = dungeon,
            Room = room,
            Party = GuildParty
        };

        var allCombatants = new List<Entity>();
        allCombatants.AddRange(GuildParty.Where(h => h.HP > 0));
        allCombatants.AddRange(room.Enemies.Where(e => e.HP > 0));
        allCombatants = allCombatants.OrderByDescending(c => (c is Hero h) ? h.Initiative : ((Enemy)c).Initiative).ToList();

        foreach (Entity combatant in allCombatants)
        {
            if (!combatant.IsAlive()) continue;

            if (GuildParty.All(h => !h.IsAlive()))
            {
                dungeon.Logs.Add("💀 Todos os aliados morreram.");
                break;
            }
            if (room.Enemies.All(e => !e.IsAlive()))
            {
                dungeon.Logs.Add("✨ Vitória! Sala limpa.");
                break;
            }

            var strategy = CombatStrategyFactory.GetBehavior(combatant.Class);
            strategy.ExecuteTurn(combatant, Context);
            CheckDeaths(allCombatants, dungeon);

        }
    }
    private void CheckDeaths(List<Entity> combatants, Dungeon dungeon)
    {
        foreach (var c in combatants)
        {
            if (!c.IsAlive() && c is Enemy e && !e.IsLooted)
            {
                dungeon.Logs.Add($"☠️ {e.Name} foi derrotado!");
                int XPsum = e.Level * 20;
                dungeon.XpReward += XPsum;
                e.IsLooted = true;
            }
            else if (!c.IsAlive() && !c.DeathLogged && c is Hero h)
            {
                dungeon.Logs.Add($"💀💀 {h.Name} caiu em combate!");
                h.DeathLogged = true;
            }
        }
    }
}