public class CombatManager
{
    public struct AttackResult
    {
        public int FinalDamage;
        public bool IsCritical;
        public bool IsHit;
        public string LogMessage;
    }

    public AttackResult CalculateAttack(Entity attacker, Entity target, int baseDamage, Elements element)
    {
        var result = new AttackResult();

        int attackRoll = attacker.RollDice();
        int totalHit = attackRoll + attacker.GetAccuracy();

        int defenseRating = 20 + target.GetEvasion();
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
            result.LogMessage = $"🛡️ [{attacker.Name}] errou! ({totalHit} vs {defenseRating})";
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
        else if(dice <= 10)
        {
            multiplier = 0.5f;
        }

        int damageDealt = target.CalculateMitigation(baseDamage * multiplier, element);
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
}