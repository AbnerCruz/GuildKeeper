using System;
using System.Collections.Generic;

public static class Economy
{
    public static int MinimumWage = 50;
    public static int BasePrice = 3;
    private static float interestMonthRate = 2;
    public static float interestDailyRate => interestMonthRate / 30;
    public static List<Resource> Resources = new()
    {
        new(IResource.Iron, BasePrice * 10),
        new(IResource.Gold, BasePrice * 15),
        new(IResource.Platinum, BasePrice * 30),
        new(IResource.Esmerald, BasePrice * 50),
        new(IResource.Diamond, BasePrice * 70),
    };

    public static void HandleInterest(ref int value)
    {
        value += (int)(value * interestDailyRate);
    }
}

public class Resource
{
    public IResource Type;
    public int Quantity;
    public int Price;

    public Resource(IResource type, int price)
    {
        Type = type;
        Price = price;
    }
}

public enum IResource
{
    Iron,
    Gold,
    Diamond,
    Platinum,
    Esmerald,
}