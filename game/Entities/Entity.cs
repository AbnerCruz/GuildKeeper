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
            Class.Warrior => 80,
            Class.Tank => 90,
            Class.Ranger => 75,
            Class.Mage => 80,
            Class.Support => 90,
            _ => 100
        };
    }

    public virtual int CalculateMitigation(float damage, Elements damageElement)
    {
        return (int)damage;
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