using Microsoft.Xna.Framework;

public class Transform
{
    public Vector2 Position;
    public Vector2 Size = new Vector2(32);

    public float OriginX => Position.X - Size.X / 2f;
    public float OriginY => Position.Y - Size.Y / 2f;
    public float Width => Size.X;
    public float Height => Size.Y;

    public Transform()
    {
        Position = Vector2.Zero + (Size / 2f);
    }
    public Transform(Vector2 position)
    {
        Position = position;
    }

    public Transform(int x, int y)
    {
        Position = new Vector2(x, y);
    }

    public Transform(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    public Transform(int x, int y, int w, int h)
    {
        Position = new Vector2(x, y);
        Size = new Vector2(w, h);
    }

    public Rectangle Rectangle()
    {
        return new Rectangle((int)OriginX, (int)OriginY, (int)Width, (int)Height);
    }
}