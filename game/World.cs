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
    private InputManager _inputManager = new InputManager();
    public Guild Guild;
    public static TileMap Map { get; private set; }
    public WorldTimeManager WorldTimeManager;

    public static List<Entity> Entities = new();


    public UIController UIController = new();

    public World()
    {
        _inputManager.WorldInstance = this;
        UIController.WorldInstance = this;
    }

    public void Start()
    {
        Map = new();
        WorldTimeManager = new(this);
        Guild = new(this, "Immortals");
        Guild.RefreshApplicants(true, 5, 1);
        Guild.RefreshAvailableDungeons(true, 5, 1);
        UIController.Build();
    }

    public void Update()
    {
        _inputManager.Update();
        WorldTimeManager.Update();
        Map.Update();
        for (int i = Entities.Count - 1; i >= 0; i--)
        {
            Entities[i].Update();
        }
        UIController.UpdateRealtimeValues();
    }

    public void Draw()
    {
        Map.Draw();
        foreach (var entity in Entities)
        {
            entity.Draw();
        }
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
}