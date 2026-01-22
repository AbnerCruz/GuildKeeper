using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

public class InputManager
{
    public World WorldInstance;
    public void Update()
    {
        Input.Update();
        Inputs();
    }

    public void Inputs()
    {
        UIStateController();

        if (Input.Keyboard.Pressed(Keys.R))
        {
            WorldInstance.Guild.RefreshApplicants(true, 5, Rng.Rand.Next(1, WorldInstance.Guild.Level + 2));
            WorldInstance.Guild.RefreshAvailableDungeons(true, 5, Rng.Rand.Next(1, WorldInstance.Guild.Level + 2));
        }
        if (Input.Keyboard.Pressed(Keys.S))
        {
            WorldInstance.WorldTimeManager.SkipMonth();
        }
    }

    public void UIStateController()
    {
        var originalStateIndex = (int)WorldInstance.UIController.CurrentUIState;
        var newStateIndex = originalStateIndex;

        if (Input.Keyboard.Pressed(Keys.Up))
        {
            newStateIndex++;
            if (newStateIndex > Enum.GetNames(typeof(UIState)).Length - 1)
                newStateIndex = 0;
        }

        if (Input.Keyboard.Pressed(Keys.Down))
        {
            newStateIndex--;
            if (newStateIndex < 0)
                newStateIndex = Enum.GetNames(typeof(UIState)).Length - 1;
        }

        if (newStateIndex != originalStateIndex)
        {
            WorldInstance.UIController.ChangeUIState((UIState)newStateIndex);
        }
    }
}