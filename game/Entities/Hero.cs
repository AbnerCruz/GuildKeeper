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
            if (Class == Class.Ranger)
                baseDmg = (int)(Dexterity * 1.3f + Strength * 0.5f);
            else
                baseDmg = (int)(Strength * 1.5f + Dexterity * 0.3f);

            int totalDmg = baseDmg + EquipmentPhysicalDamage;
            if (Stressed) totalDmg = (int)(totalDmg * 0.5f);
            return Math.Max(1, totalDmg);
        }
    }
    public int MagicPower
    {
        get
        {
            int baseDmg = 0;
            if (Class == Class.Support)
                baseDmg = (int)(Charisma * 1.3f + Wisdom * 0.5f);
            else if(Class == Class.Mage)
            {
                baseDmg = (int)(Wisdom * 1.5f + Charisma * 0.5f);
            }
            else
                baseDmg = 0;

            int totalDmg = baseDmg + EquipmentMagicPower;

            if (Stressed) totalDmg = (int)(totalDmg * 0.5f);

            return Math.Max(1, totalDmg);
        }
    }
    public int Armour => Math.Max(1, (int)(Constitution * 0.5f) + (int)(Dexterity * 0.2f) + EquipmentArmour);

    public int Speed => Math.Min(15, 5 + (Dexterity / 4));

    public List<Elements> Damages { get; set; } = new(){Elements.Physical};
    public List<Elements> Resistances { get; set; } = new(){Elements.None};

    public List<Equipment> Equipment;

    public float IdleTimer;
    public float WalkTimer;
    private Vector2 _wanderDirection;
    private bool _isWalking = false; 

    public Hero(int baseHp, int baseMana, int baseEnergy, int strength, int dexterity, int constitution, int wisdom, int charisma)
    {
        Race = Creatures.Human;
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

    public Hero() : this(10, 10, 10, 1, 1, 1, 1, 1) { }
    
    public Hero(int level) : this(10, 10, 10, 1, 1, 1, 1, 1)
    {
        RandomInitialAttributes(level);
    }

    public Hero(Transform transform) : this(10, 10, 10, 1, 1, 1, 1, 1)
    {
        Transform = transform;
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
        Name = $"{Race.ToString()} {Class} Lvl: {Level}";
        HP = MaxHP;
        Mana = MaxMana;
        Energy = MaxEnergy;
        CurrentStress = 0;
        Stressed = false;
    }

    public void RandomInitialAttributes(int level)
    {
        Level = level;

        var classValues = Enum.GetValues(typeof(Class));
        Class = (Class)classValues.GetValue(Rng.Rand.Next(classValues.Length));

        BaseStrength = Rng.Rand.Next(1, 10);
        BaseDexterity = Rng.Rand.Next(1, 10);
        BaseConstitution = Rng.Rand.Next(1, 10);
        BaseWisdom = Rng.Rand.Next(1, 10);
        BaseCharisma = Rng.Rand.Next(1, 10);
        ApplyClassArchetype();

        int levelBonus = Level/5;
        Strength = BaseStrength + levelBonus;
        Dexterity = BaseDexterity + levelBonus;
        Constitution = BaseConstitution + levelBonus;
        Wisdom = BaseWisdom + levelBonus;
        Charisma = BaseCharisma + levelBonus;

        BaseHP = 15 + BaseConstitution;
        BaseMana = 10 + BaseWisdom;
        BaseEnergy = 10 + BaseDexterity;

        RestoreVitals();
    }

    private void ApplyClassArchetype()
    {
        int primaryBonus = 5;
        int secondaryBonus = 2;

        Damages.Clear();
        Resistances.Clear();

        switch (Class)
        {
            case Class.Warrior:
                BaseStrength += primaryBonus;
                BaseConstitution += secondaryBonus;
                SetRandomDamage(Elements.Physical, Elements.Fire);
                SetRandomResistances(Elements.Physical, Elements.Fire);
                break;

            case Class.Mage:
                BaseWisdom += primaryBonus;
                BaseCharisma += secondaryBonus;
                BaseDexterity = Math.Max(1, BaseDexterity - 1);
                BaseStrength = Math.Max(1, BaseStrength - 1);
                SetRandomDamage(Elements.Fire, Elements.Water, Elements.Earth, Elements.Air, Elements.Darkness);
                SetRandomResistances(Elements.Fire, Elements.Water, Elements.Earth, Elements.Air, Elements.Darkness);
                break;

            case Class.Tank:
                BaseConstitution += primaryBonus;
                BaseCharisma += secondaryBonus;
                BaseStrength = Math.Max(1, BaseStrength - 2);
                BaseWisdom = Math.Max(1, BaseWisdom - 4);
                SetRandomDamage(Elements.Physical, Elements.Earth);
                SetRandomResistances(Elements.Physical, Elements.Earth);
                break;

            case Class.Ranger:
                BaseDexterity += primaryBonus;
                BaseStrength += secondaryBonus;
                BaseConstitution = Math.Max(1, BaseConstitution - 2);
                SetRandomDamage(Elements.Physical, Elements.Poison, Elements.Air);
                SetRandomResistances(Elements.None);
                break;

            case Class.Support:
                BaseCharisma += primaryBonus;
                BaseWisdom += secondaryBonus;
                BaseStrength -= Math.Max(1, BaseStrength - 4);
                BaseConstitution -= Math.Max(1, BaseConstitution - 1);
                SetRandomDamage(Elements.Water, Elements.Air);
                SetRandomResistances(Elements.None);
                break;
        }
    }

    private void SetRandomDamage(params Elements[] allowedTypes)
    {
        if (allowedTypes.Length > 0)
        {
            int index = Rng.Rand.Next(0, allowedTypes.Length);
            Damages.Add(allowedTypes[index]);
        }
        else
        {
            Damages.Add(Elements.Physical);
        }
    }

    private void SetRandomResistances(params Elements[] allowedTypes)
    {
        if (allowedTypes.Length > 0)
        {
            int index = Rng.Rand.Next(0, allowedTypes.Length);
            Resistances.Add(allowedTypes[index]);
        }
        else
        {
            Resistances.Add(Elements.None);
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

    public override int CalculateMitigation(float damage, Elements damageElement)
    {
        int damageTaken = (int)(damage - Armour);

        if (damageTaken < 0) damageTaken = 0;
        if (damageTaken == 0 && Rng.Rand.NextDouble() > 0.5) damageTaken = 1;

        if (Resistances.Contains(damageElement))
        {
            damageTaken = Math.Max(1, (int)(damageTaken * 0.8f));
        }
        return damageTaken;
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
        LevelUpPoints += 5;
        AutoDistributeAttributePoints();
        RestoreVitals();
        UIController.WorldInstance.BuildUI();
    }

    public void AutoDistributeAttributePoints()
    {
        while (LevelUpPoints > 0)
        {
            int roll = Rng.Rand.Next(0, 5);
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