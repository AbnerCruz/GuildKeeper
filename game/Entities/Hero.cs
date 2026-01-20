using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Hero : Entity
{
    public Guild Guild;

    public int Level { get; set; } = 1;
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
    public int MaxHP => BaseHP + (int)(Constitution * 1.2f) + ((Level - 1) * 10);
    public int HP;

    public int BaseMana;
    public int MaxMana => Math.Max(1, (BaseMana + Wisdom) * Level);
    public int Mana;

    public int BaseEnergy;
    public int MaxEnergy => Math.Max(1, (BaseEnergy + (int)(Constitution * 1) + (int)(Charisma * 0.5)) + Level);
    public int Energy;

    public int BaseSatisfaction;
    public int MaxSatisfaction => BaseSatisfaction + (Charisma * 2);
    public int Satisfaction;

    public float CurrentStress;
    public float StressModifier => Modifier / (float)Math.Max(1, Charisma);
    public bool Stressed;

    public int Wage
    {
        get
        {
            int baseWage = Economy.MinimumWage;

            int strValue = Strength * ((int)(Economy.BasePrice * 1.5));
            int dexValue = Dexterity * ((int)(Economy.BasePrice * 4.5));
            int conValue = Constitution * ((int)(Economy.BasePrice * 2));
            int wisValue = Wisdom * ((int)(Economy.BasePrice * 2.5));
            int chaValue = Charisma * ((int)(Economy.BasePrice * 0.5));

            int levelTax = (Level - 1) * 50;

            return baseWage + strValue + dexValue + conValue + wisValue + chaValue + levelTax;
        }
    }

    public int EquipmentPhysicalDamage;
    public int EquipmentMagicPower;
    public int EquipmentArmour;

    public int StressDamageDebuff;
    public int MagicDamageDebuff;

    public int Initiative => Dexterity;
    public int PhysicalDamage
    {
        get
        {
            int baseDmg = 0;
            if (Class == Class.Ranger) baseDmg = (int)(Dexterity * 1.4f + Strength * 0.5f);
            else if(Class == Class.Tank) baseDmg = (int)(Strength * 0.8f + Constitution * 0.5f);

            else
                baseDmg = (int)(Strength * 1.3f + Dexterity * 0.3f);

            int totalDmg = baseDmg + EquipmentPhysicalDamage;
            if (Stressed) totalDmg = (int)(totalDmg * 0.7f);
            return Math.Max(1, totalDmg);
        }
    }
    public int MagicPower
    {
        get
        {
            int baseDmg = 0;
            if (Class == Class.Support)
                baseDmg = (int)(Charisma * 1.3f + Wisdom * 0.2f);
            else if(Class == Class.Mage)
            {
                baseDmg = (int)(Wisdom * 1.4f + Charisma * 0.4f);
            }
            else
                baseDmg = 0;

            int totalDmg = baseDmg + EquipmentMagicPower;

            if (Stressed) totalDmg = (int)(totalDmg * 0.5f);

            return Math.Max(1, totalDmg);
        }
    }
    public int Armour => Math.Max(1, (int)(Constitution * 0.7f) + (int)(Dexterity * 0.2f) + EquipmentArmour);

    public int Speed => Math.Min(15, 5 + (Dexterity / 4));

    public ElementInfo Element;

    public List<Equipment> Equipment;

    public float IdleTimer;
    public float WalkTimer;
    private Vector2 _wanderDirection;
    private bool _isWalking = false; 

    public Hero(Guild guild, int baseHp, int baseMana, int baseEnergy, int strength, int dexterity, int constitution, int wisdom, int charisma)
    {
        Guild = guild;
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

    public Hero(Guild guild) : this(guild, 10, 10, 10, 1, 1, 1, 1, 1) { }
    
    public Hero(Guild guild, int level) : this(guild, 10, 10, 10, 1, 1, 1, 1, 1)
    {
        RandomInitialAttributes(level);
    }

    public Hero(Guild guild, Transform transform) : this(guild, 10, 10, 10, 1, 1, 1, 1, 1)
    {
        Transform = transform;
    }

    public Hero(Guild guild, Class heroClass, int level) : this(guild, 10, 10, 10, 1, 1, 1, 1, 1)
    {
        RandomInitialAttributes(level, heroClass);
    }

    public override void Update()
    {
        base.Update();
        AI();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public void RestoreVitals()
    {
        Name = $"{Race} {Element.Type} {Class} Lvl: {Level}";
        HP = MaxHP;
        Mana = MaxMana;
        Energy = MaxEnergy;
        CurrentStress = 0;
        Stressed = false;
    }

    public void RandomInitialAttributes(int level, Class? forcedClass = null)
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
        ApplyClassArchetype();

        Strength = BaseStrength;
        Dexterity = BaseDexterity;
        Constitution = BaseConstitution;
        Wisdom = BaseWisdom;
        Charisma = BaseCharisma;

        BaseHP = 15 + BaseConstitution;
        BaseMana = 10 + BaseWisdom;
        BaseEnergy = 10 + BaseDexterity;

        for(int i = 0; i < level; i++)
        {
            LevelUp();
        }

        RestoreVitals();
    }

    private void ApplyClassArchetype()
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
                SetRandomElement(ElementType.Air, ElementType.Darkness, ElementType.Earth, ElementType.Fire, ElementType.Ice, ElementType.Light, ElementType.Lightning, ElementType.Water);
                break;

            case Class.Tank:
                BaseConstitution += hugeBonus;
                BaseCharisma += moderateBonus;
                BaseDexterity = Math.Max(1, BaseDexterity - moderatePenality);
                BaseWisdom = Math.Max(1, BaseWisdom - hugePenality);
                SetRandomElement(ElementType.Earth, ElementType.Light, ElementType.Ice);
                break;

            case Class.Ranger:
                BaseDexterity += hugeBonus;
                BaseStrength += moderateBonus;
                BaseConstitution = Math.Max(1, BaseConstitution - moderatePenality);
                SetRandomElement(ElementType.Fire, ElementType.Ice, ElementType.Poison);
                break;

            case Class.Support:
                BaseCharisma += hugeBonus;
                BaseWisdom += moderateBonus;
                BaseStrength = Math.Max(1, BaseStrength - hugePenality);
                BaseConstitution = Math.Max(1, BaseConstitution - moderatePenality);
                SetRandomElement(ElementType.Water, ElementType.Light, ElementType.Air);
                break;
        }
    }

    private void SetRandomElement(params ElementType[] allowedTypes)
    {
        var rng = Rng.Rand.NextDouble();
        if (rng < 0.1) Element = new(Rng.RandEnum<ElementType>());
        else if(rng < 0.4) Element = new(ElementType.None);
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

    public void AI()
    {
        if (_isWalking)
        {
            Move(_wanderDirection, Speed);
            if (Time.Timer(ref WalkTimer))
            {
                _isWalking = false;
                Time.ResetTimer(ref IdleTimer, Rng.Rand.Next(1, 10));
            }
        }
        else
        {
            if (Time.Timer(ref IdleTimer))
            {
                _isWalking = true;

                int[] axis = new int[] { -1, 0, 1 };
                float x = axis[Rng.Rand.Next(0, axis.Length)];
                float y = axis[Rng.Rand.Next(0, axis.Length)];

                if (x == 0 && y == 0) x = 1;

                _wanderDirection = new Vector2(x, y);
                Time.ResetTimer(ref WalkTimer, Rng.Rand.Next(1, 10));
            }
        }
    }

    public int TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 0) HP = 0;
        return damage;
    }

    public int Heal(int heal)
    {
        HP += heal;
        if (HP >= MaxHP)
        {
            HP = MaxHP;
        }
        return heal;
    }
    
    public bool Stress()
    {
        CurrentStress += StressModifier;
        if (CurrentStress >= 10)
        {
            Stressed = true;
            Satisfaction--;
        }
        else
        {
            Stressed = false;
        }
        return Stressed;
    }

    public bool IsAlive()
    {
        return HP > 0;
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
        RestoreVitals();
        Guild?.World?.BuildUI();
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

    public void Pay()
    {
        Guild.Gold -= Wage;
        Satisfaction = Math.Min(MaxSatisfaction, Satisfaction + 10);
    }

    public override int GetAccuracy()
    {
        if (Class == Class.Mage || Class == Class.Support)
            return Wisdom + (Level * 2);

        return Dexterity + (Level * 2);
    }

    public override int GetEvasion()
    {
        return (Dexterity / 2) + Level;
    }
}