using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Entity
{
    public string Name;
    public int Level;
    public int HP;
    public int MaxHP;
    public int Mana;
    public bool DeathLogged { get; set; } = false;
    public int Initiative;
    public int Armour;
    public Transform Transform = new();
    public Rectangle CollisionBox => Transform.Rectangle();
    public Creatures Race;
    public Class Class;
    
    public bool IsMagical => Class == Class.Mage || Class == Class.Support;
    public ElementInfo Element;


    public virtual void Update() {}
    public virtual void Draw()
    {
        Render.SpriteBatch.Draw(Render.Pixel, CollisionBox, Color.White);
    }

    public bool IsAlive()
    {
        return HP > 0;
    }

    public void Move(Vector2 direction, int speed)
    {
        if (direction != Vector2.Zero) direction.Normalize();
        var newPosition = direction * speed * Time.DeltaTime;
        if (newPosition == Vector2.Zero) return;
        Transform.Position += newPosition;
    }

    public int RollDice()
    {
        return Rng.Rand.Next(0, 100);
    }

    public virtual int GetCritThreshold()
    {
        return this.Class switch
        {
            Class.Warrior => 90,
            Class.Tank => 95,
            Class.Ranger => 85,
            Class.Mage => 85,
            Class.Support => 90,
            _ => 100
        };
    }

    public virtual int CalculateMitigation(float damage, ElementType attackerElement, ElementInfo targetElement, int targetArmour)
    {
        int damageTaken = Math.Max(1, (int)(damage - targetArmour));
        float multiplier = 1f;
        if (targetElement.StrongAgainst.Contains(attackerElement) || targetElement.Type == attackerElement)
        {
            multiplier = 0.7f;
        }
        else if (targetElement.WeakAgainst.Contains(attackerElement))
        {
            multiplier = 1.2f;
        }
        damageTaken = (int)(damageTaken * multiplier);
        damageTaken = (damageTaken <= 0) ? 1 : damageTaken;
        return damageTaken;
    }

    public int TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 0) HP = 0;
        return damage;
    }

    public virtual int GetAccuracy()
    {
        return 0;
    }

    public virtual int GetEvasion()
    {
        return 0;
    }

    public virtual void HandleResources(Dungeon dungeon) { }

    public virtual int Heal(int heal)
    {
        HP += heal;
        if (HP >= MaxHP)
        {
            HP = MaxHP;
        }
        return heal;
    }
}