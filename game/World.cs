using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class World
{
    public static World Instance { get; private set; }
    private double totalSeconds;
    private double tick => Time.Tick;
    private InputManager _inputManager = new InputManager();
    public Guild Guild;
    public static TileMap Map { get; private set; }
    public WorldTimeManager WorldTimeManager;

    private Buildable _pendingBuildItem;

    public static List<Entity> Entities = new();


    public UIController UIController = new();

    public World()
    {
        _inputManager.WorldInstance = this;
        UIController.WorldInstance = this;
        if (BuildDatabase.AllItems.Count == 0) BuildDatabase.Initialize();
    }

    public void Start()
    {
        Map = new();
        WorldTimeManager = new(this);
        Guild = new(this, "Immortals");
        Guild.RefreshApplicants(Class.Warrior);
        Guild.RefreshAvailableDungeons(true, 5, 1);
        UIController.Build();
    }

    public void Update()
    {
        totalSeconds += Time.DeltaTime;
        _inputManager.Update();
        Map.Update();
        UIController.UpdateRealtimeValues();
        for (int i = Entities.Count - 1; i >= 0; i--)
        {
            Entities[i].Update();
        }
        while (totalSeconds >= tick)
        {
            WorldTimeManager.Update();
            Guild.Update();
            totalSeconds -= tick;
        }
        HandleBuildingLogic();
    }

    public void Draw()
    {
        Map.Draw();
        foreach (var entity in Entities)
        {
            entity.Draw();
        }

        DrawBuildPreview();
    }

    public void BuildUI()
    {
        UIController.Build();
    }

    public static bool Instantiate(Entity entity)
    {
        if (entity == null) return false;
        if (Entities.Contains(entity)) return false;
        Entities.Add(entity);
        return true;
    }

    public static bool Destroy(Entity entity)
    {
        if (entity == null) return false;
        if (!Entities.Contains(entity)) return false;
        Entities.Remove(entity);
        return true;
    }

    public void EnterBuildMode(Buildable item)
    {
        _pendingBuildItem = item;
    }

    public void CancelBuildMode()
    {
        _pendingBuildItem = null;
    }

    private void HandleBuildingLogic()
    {
        if (_pendingBuildItem == null) return;

        MouseState mouse = Mouse.GetState();

        if (mouse.RightButton == ButtonState.Pressed)
        {
            CancelBuildMode();
            return;
        }

        if (mouse.LeftButton == ButtonState.Pressed && IsMouseInsideMap())
        {
            if (!UIController.IsMouseOverGUI())
            {
                TryPlaceBuilding();
            }
        }
    }

    private bool IsMouseInsideMap()
    {
        return true;
    }

    private void TryPlaceBuilding()
    {
        MouseState mouse = Mouse.GetState();
        Point gridPos = Map.WorldToGrid(new Vector2(mouse.X, mouse.Y));

        if (!Map.IsValidPlacement(gridPos.X, gridPos.Y, _pendingBuildItem.Size))
        {
            Console.WriteLine("Invalid Position");
            return;
        }

        if (Guild.Gold >= _pendingBuildItem.GoldCost)
        {
            Guild.Gold -= _pendingBuildItem.GoldCost;

            Map.PlaceStructure(gridPos.X, gridPos.Y, _pendingBuildItem);

            Console.WriteLine($"Build {_pendingBuildItem.Name}!");
            UIController.RefreshInfo();
        }
    }

    private void DrawBuildPreview()
    {
        if (_pendingBuildItem == null) return;

        MouseState mouse = Mouse.GetState();
        Point gridPos = Map.WorldToGrid(new Vector2(mouse.X, mouse.Y));
        Vector2 worldPos = Map.GridToWorld(gridPos.X, gridPos.Y);

        Rectangle rect = new Rectangle(
            (int)worldPos.X,
            (int)worldPos.Y,
            _pendingBuildItem.Size.X * Map.TileSize,
            _pendingBuildItem.Size.Y * Map.TileSize
        );

        bool isValid = Map.IsValidPlacement(gridPos.X, gridPos.Y, _pendingBuildItem.Size);
        Color ghostColor = isValid ? Color.Blue * 0.5f : Color.Red * 0.5f;

        Render.SpriteBatch.Draw(Render.Pixel, rect, ghostColor);
    }

}