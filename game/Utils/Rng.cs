using System;

public static class Rng
{
    public static Random Rand = new Random();

    public static T RandEnum<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Rand.Next(values.Length));
    }
}