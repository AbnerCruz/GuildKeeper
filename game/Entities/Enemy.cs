using System;

public class Enemy : Entity
{
    public int Level;
    public int Damage;
    public int MaxHP;
    public int HP;
    public int Armour;
    public int Dexterity;
    public int Initiative;
    public Elements DamageElement;
    public Elements ResistanceElement;

    public Enemy(int level, int partySize, Creatures creature, Class enemyClass, Elements damageElement = Elements.Physical, Elements resistanceElement = Elements.None)
    {
        Level = level;
        Race = creature;
        Class = enemyClass;
        DamageElement = damageElement;
        ResistanceElement = resistanceElement;

        float partyScaling = (float)Math.Sqrt(partySize);
        MaxHP = (int)((20 + (level * 5)) * partyScaling - 1);
        HP = MaxHP; 
        Damage = Rng.Rand.Next(1, 10) + level;

        Dexterity = Rng.Rand.Next(1, 10) + level/5;
        Armour = 5 + level / 2;

        ApplyClassArchetype();

        Initiative = Rng.Rand.Next(Dexterity / 2, Dexterity * 2);
        Name = $"{Race.ToString()} {Class} lvl {Level}";
    }

    private void ApplyClassArchetype()
    {
        switch (Class)
        {
            case Class.Tank:
                HP = (int)(HP * 1.4f);
                Armour += 2 + (Level / 2);
                Damage = Math.Max(1, (int)(Damage * 0.8f));
                Dexterity = (int)(Dexterity * 0.5f);
                break;

            case Class.Warrior:
                Damage = (int)(Damage * 1.2f);
                HP = (int)(HP * 1.1f);
                break;

            case Class.Ranger:
                Damage = (int)(Damage * 1.4f);
                HP = (int)(HP * 0.7f);
                Dexterity = (int)(Dexterity * 1.5f);
                Armour = Math.Max(0, Armour - 2);
                break;

            case Class.Mage:
                Damage = (int)(Damage * 1.6f);
                HP = (int)(HP * 0.6f);
                Armour = Level;
                break;

            case Class.Support:
                Damage = (int)(Damage * 0.5f);
                break;
        }
    }

    public override int CalculateMitigation(float damage, Elements damageElement)
    {
        int damageTaken = Math.Max(1, (int)(damage - Armour));
        if (ResistanceElement == damageElement)
        {
            damageTaken = Math.Max(1, (int)(damageTaken * 0.7));
        }
        return damageTaken;
    }

    public int TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 0) HP = 0;
        return damage;
    }

    public override int GetAccuracy()
    {
        return Dexterity + Level;
    }

    public override int GetEvasion()
    {
        return (Dexterity / 2) + Level;
    }
}
    