using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class TileMap
{
    public int TileSize = 32;
    public int[,] Data = new int[40, 21];
    
    public int Columns => Data.GetLength(0);
    public int Rows => Data.GetLength(1);

    public TileMap()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (x == 0 || x == Columns - 1 || y == 0 || y == Rows - 1)
                {
                    Data[x, y] = 1;
                }
                else
                {
                    Data[x, y] = 0;
                }
            }
        }

        //Center
        Data[Columns / 2, Rows / 2] = 2; //Spawn Heroes
    }

    public void Update()
    {
        for (int x = 0; x < Columns; x++)
        {
            for(int y = 0; y < Rows; y++)
            {
                continue;
            }
        }
    }

    public void Draw()
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                int tileID = Data[x, y];

                Vector2 position = new Vector2(x * TileSize, y * TileSize);
                Rectangle rect = new Rectangle((int)position.X, (int)position.Y, TileSize, TileSize);

                Color color = Color.White;

                if (tileID == 0) color = Color.DarkGreen; //grass
                if (tileID == 1) color = Color.Gray; //wall
                if (tileID == 2) color = Color.Brown; //spawn

                Render.SpriteBatch.Draw(Render.Pixel, rect, color);
            }
        }

        MouseState mouse = Mouse.GetState();
        Point gridPos = WorldToGrid(new Vector2(mouse.X, mouse.Y));

        if (IsValidIndex(gridPos.X, gridPos.Y))
        {
            Rectangle selectedRect = new Rectangle(gridPos.X * TileSize, gridPos.Y * TileSize, TileSize, TileSize);

            Render.SpriteBatch.Draw(Render.Pixel, selectedRect, Color.White * 0.5f);
        }
    }

    public Vector2 GetSpawnPosition(){
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (Data[x, y] == 2)
                {
                    return GridToWorld(x, y);
                }
            }
        }
        return Vector2.One * 16;
    }

    public bool IsValidIndex(int x, int y)
    {
        return x >= 0 && x < Columns && y >= 0 && y < Rows;
    }

    public Point WorldToGrid(Vector2 worldPosition)
    {
        return new Point((int)(worldPosition.X / TileSize), (int)(worldPosition.Y / TileSize));
    }
    
    public Vector2 GridToWorld(int x, int y)
    {
        return new Vector2(x * TileSize, y * TileSize);
    }
}