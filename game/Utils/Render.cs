using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Render
{
    public static SpriteBatch SpriteBatch;
    public static Texture2D Pixel;

    public Render(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SpriteBatch = spriteBatch;
        Pixel = new Texture2D(graphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });
    }

}