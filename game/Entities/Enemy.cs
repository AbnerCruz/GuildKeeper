using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Enemy : Entity
{
    public int LevelUpPoints;
    public int XP { get; set; }
    public int NextLevelXP => Level * 500;

    public int BaseStrength { get; private set; }
    public int BaseDexterity { get; private set; }
    public int BaseConstitution { get; private set; }
    public int BaseWisdom { get; private set; }
    public int BaseCharisma { get; private set; }
    public int Modifier = 5;

    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Wisdom;
    public int Charisma;

    public int BaseHP;

    public int BaseMana;
    public int MaxMana => Math.Max(1, (BaseMana + Wisdom) * Level);

    public int BaseEnergy;
    public int MaxEnergy => Math.Max(1, (BaseEnergy + (int)(Constitution * 1) + (int)(Charisma * 0.5)) + Level);
    public int Energy;

    public int EquipmentPhysicalDamage;
    public int EquipmentMagicPower;
    public int EquipmentArmour;

    public int DamageDebuff;
    public int MagicDamageDebuff;

    public int PhysicalDamage
    {
        get
        {
            int baseDmg = 0;
            if (Class == Class.Ranger) baseDmg = (int)(Dexterity * 1f + Strength * 0.5f);
            else if (Class == Class.Tank) baseDmg = (int)(Strength * 0.5f + Constitution * 0.5f);

            else
                baseDmg = (int)(Strength * 1f + Dexterity * 0.3f);

            int totalDmg = baseDmg + EquipmentPhysicalDamage;
            return Math.Max(1, totalDmg);
        }
    }
    public int MagicPower
    {
        get
        {
            int baseDmg = 0;
            if (Class == Class.Support)
                baseDmg = (int)(Charisma * 1f + Wisdom * 0.2f);
            else if (Class == Class.Mage)
            {
                baseDmg = (int)(Wisdom * 1f + Charisma * 0.4f);
            }
            else
                baseDmg = 0;

            int totalDmg = baseDmg + EquipmentMagicPower;
            return Math.Max(1, totalDmg);
        }
    }

    public List<Equipment> Equipment;
    public bool IsLooted = false;

    public Enemy(int baseHp, int baseMana, int baseEnergy, int strength, int dexterity, int constitution, int wisdom, int charisma)
    {
        Race = Creatures.Human;
        Element = new(ElementType.None);
        BaseHP = baseHp;
        BaseMana = baseMana;
        BaseEnergy = baseEnergy;
        BaseStrength = strength;
        BaseDexterity = dexterity;
        BaseConstitution = constitution;
        BaseWisdom = wisdom;
        BaseCharisma = charisma;

        Strength = BaseStrength;
        Dexterity = BaseDexterity;
        Constitution = BaseConstitution;
        Wisdom = BaseWisdom;
        Charisma = BaseCharisma;

        RestoreVitals();
    }

    public Enemy() : this(10, 10, 10, 1, 1, 1, 1, 1) { }

    public Enemy(int level, ElementInfo element) : this(10, 10, 10, 1, 1, 1, 1, 1)
    {
        RandomInitialAttributes(level, null, true);
        Element = element;
    }

    public Enemy(Class enemyClass, int level) : this(10, 10, 10, 1, 1, 1, 1, 1)
    {
        RandomInitialAttributes(level, enemyClass);
    }

    public override void Update()
    {
        base.Update();
    }

    public void RestoreVitals()
    {
        MaxHP = BaseHP + (int)(Constitution * 2f) + ((Level - 1) * 15);
        HP = MaxHP;
        Initiative = Dexterity;
        Armour = Math.Max(1, (int)(Constitution * 0.5f) + (int)(Dexterity * 0.2f) + EquipmentArmour);
        Mana = MaxMana;
        Energy = MaxEnergy;
    }

    public void RandomInitialAttributes(int level, Class? forcedClass = null, bool preserveELement = false)
    {
        Level = 0;

        if (forcedClass.HasValue)
        {
            Class = forcedClass.Value;
        }
        else
        {
            Class = Rng.RandEnum<Class>();
        }
        BaseStrength = Rng.Rand.Next(1, 8);
        BaseDexterity = Rng.Rand.Next(1, 8);
        BaseConstitution = Rng.Rand.Next(1, 8);
        BaseWisdom = Rng.Rand.Next(1, 8);
        BaseCharisma = Rng.Rand.Next(1, 8);
        ApplyClassArchetype(!preserveELement);

        Strength = BaseStrength;
        Dexterity = BaseDexterity;
        Constitution = BaseConstitution;
        Wisdom = BaseWisdom;
        Charisma = BaseCharisma;

        BaseHP = 40 + BaseConstitution;
        BaseMana = 10 + BaseWisdom;
        BaseEnergy = 10 + BaseDexterity;

        for (int i = 0; i < level; i++)
        {
            LevelUp();
        }

        RestoreVitals();
    }

    private void ApplyClassArchetype(bool generateElement = true)
    {
        int hugeBonus = 4;
        int moderateBonus = 2;
        int hugePenality = 4;
        int moderatePenality = 2;

        switch (Class)
        {
            case Class.Warrior:
                BaseStrength += hugeBonus;
                BaseConstitution += moderateBonus;
                SetRandomElement(ElementType.Earth, ElementType.Fire, ElementType.Ice, ElementType.Lightning, ElementType.Poison);
                break;

            case Class.Mage:
                BaseWisdom += hugeBonus;
                BaseCharisma += moderateBonus;
                BaseDexterity = Math.Max(1, BaseDexterity - moderatePenality);
                BaseStrength = Math.Max(1, BaseStrength - moderatePenality);
                if(generateElement) SetRandomElement(ElementType.Air, ElementType.Darkness, ElementType.Earth, ElementType.Fire, ElementType.Ice, ElementType.Light, ElementType.Lightning, ElementType.Water);
                break;

            case Class.Tank:
                BaseConstitution += hugeBonus;
                BaseCharisma += moderateBonus;
                BaseDexterity = Math.Max(1, BaseDexterity - moderatePenality);
                BaseWisdom = Math.Max(1, BaseWisdom - hugePenality);
                if (generateElement) SetRandomElement(ElementType.Earth, ElementType.Light, ElementType.Ice);
                break;

            case Class.Ranger:
                BaseDexterity += hugeBonus;
                BaseStrength += moderateBonus;
                BaseConstitution = Math.Max(1, BaseConstitution - moderatePenality);
                if (generateElement) SetRandomElement(ElementType.Fire, ElementType.Ice, ElementType.Poison);
                break;

            case Class.Support:
                BaseCharisma += hugeBonus;
                BaseWisdom += moderateBonus;
                BaseStrength = Math.Max(1, BaseStrength - hugePenality);
                BaseConstitution = Math.Max(1, BaseConstitution - moderatePenality);
                if (generateElement) SetRandomElement(ElementType.Water, ElementType.Light, ElementType.Air);
                break;
        }
    }

    private void SetRandomElement(params ElementType[] allowedTypes)
    {
        var rng = Rng.Rand.NextDouble();
        if (rng < 0.1) Element = new(Rng.RandEnum<ElementType>());
        else if (rng < 0.4) Element = new(ElementType.None);
        else
        {
            int index = Rng.Rand.Next(0, allowedTypes.Length);
            Element = new(allowedTypes[index]);
        }
    }

    public void NewEquipment(Equipment equipment)
    {
        Equipment.Add(equipment);
        EquipmentPhysicalDamage = 0;
        EquipmentMagicPower = 0;
        EquipmentArmour = 0;
        if (Equipment.Count != 0)
        {
            foreach (Equipment e in Equipment)
            {
                EquipmentPhysicalDamage += e.PhysicalDamage;
                EquipmentMagicPower += e.MagicDamage;
                EquipmentArmour += e.Armour;
            }
        }
    }

    public void GainXP(int amount)
    {
        XP += amount;
        while (XP >= NextLevelXP)
        {
            XP -= NextLevelXP;
            LevelUp();
        }
    }

    public void LevelUp()
    {
        Level++;
        LevelUpPoints += 3;
        AutoDistributeAttributePoints();
        Name = $"{Race} {Element.Type} {Class} Lvl: {Level}";
    }

    public void AutoDistributeAttributePoints()
    {
        int attributesCount = 5;
        double classFocusChance = 0.4;
        while (LevelUpPoints > 0)
        {
            bool prioritizeAttributes = Rng.Rand.NextDouble() > classFocusChance;
            if (prioritizeAttributes)
            {
                int roll = Rng.Rand.Next(0, attributesCount);
                switch (Class)
                {
                    case Class.Warrior:
                        if (roll < 3) Strength++; else Constitution++;
                        break;
                    case Class.Ranger:
                        if (roll < 3) Dexterity++; else Strength++;
                        break;
                    case Class.Mage:
                        if (roll < 3) Wisdom++; else Charisma++;
                        break;
                    case Class.Tank:
                        if (roll < 3) Constitution++; else Strength++;
                        break;
                    case Class.Support:
                        if (roll < 3) Charisma++; else Wisdom++;
                        break;
                    default:
                        Strength++;
                        break;
                }
            }
            else
            {
                int roll = Rng.Rand.Next(0, attributesCount);
                switch (roll)
                {
                    case 0: Strength++; break;
                    case 1: Dexterity++; break;
                    case 2: Constitution++; break;
                    case 3: Wisdom++; break;
                    case 4: Charisma++; break;
                }
            }
            LevelUpPoints--;
        }
    }

    public override int GetAccuracy()
    {
        int acc = 0;
        if (Class == Class.Mage || Class == Class.Support)
            acc = Wisdom + (Level * 2);
        else
            acc = Dexterity + (Level * 2);

        if (Energy <= 0) acc -= 5;

        return acc;
    }

    public override int GetEvasion()
    {
        int eva = (Dexterity / 2) + Level;

        if (Energy <= 0) eva -= 5;

        return eva;
    }

    public override void HandleResources(Dungeon dungeon)
    {
        Energy -= (IsMagical) ? 0 : 1;
        Mana -= (IsMagical) ? 2 : 0;

        if (Energy <= 0)
        {
            Energy = 0;
        }
        else if (Mana <= 0)
        {
            Mana = 0;
            Energy -= 2;
        }
        if (Energy <= MaxEnergy * 0.3f && Energy > 0)
            dungeon.Logs.Add($"💦 {Name} está ficando ofegante...");
    }

    

}