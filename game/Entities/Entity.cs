using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Entity
{
    public string Name;
    public Transform Transform = new();
    public Rectangle CollisionBox => Transform.Rectangle();
    public Creatures Race;
    public Class Class;

    public virtual void Update() {}
    public virtual void Draw()
    {
        Render.SpriteBatch.Draw(Render.Pixel, CollisionBox, Color.White);
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
            Class.Warrior => 85,
            Class.Tank => 95,
            Class.Ranger => 80,
            Class.Mage => 85,
            Class.Support => 95,
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

    public virtual int GetAccuracy()
    {
        return 0;
    }

    public virtual int GetEvasion()
    {
        return 0;
    }
}