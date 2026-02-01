using System.Linq;
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
            for (int y = 0; y < Rows; y++)
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
                else if (tileID == 2) color = Color.Brown; //spawn
                else
                {
                    var item = BuildDatabase.AllItems.FirstOrDefault(i => i.ID == tileID);
                    if (item != null)
                    {
                        color = item.PlaceholderColor;
                    }
                    else
                    {
                        color = Color.Magenta;
                    }
                }

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

    public Vector2 GetSpawnPosition()
    {
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

    public bool IsValidPlacement(int startX, int startY, Point size)
    {
        for (int x = startX; x < startX + size.X; x++)
        {
            for (int y = startY; y < startY + size.Y; y++)
            {
                if (!IsValidIndex(x, y)) return false;

                if (Data[x, y] != 0) return false;
            }
        }
        return true;
    }

    public void PlaceStructure(int startX, int startY, Buildable item)
    {
        for (int x = startX; x < startX + item.Size.X; x++)
        {
            for (int y = startY; y < startY + item.Size.Y; y++)
            {
                Data[x, y] = item.ID;
            }
        }
    }
}