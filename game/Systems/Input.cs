using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public static class Input
{
    public static void Update()
    {
        Keyboard.Update();
    }
    public static class Keyboard
    {
        public static KeyboardState currentState;
        public static KeyboardState previousState;

        public static void Update()
        {
            previousState = currentState;
            currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }

        public static bool Pressed(Keys key)
        {
            return currentState.IsKeyDown(key) && previousState.IsKeyUp(key);
        }
    }
}
