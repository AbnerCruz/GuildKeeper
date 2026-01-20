using System;
using System.Collections.Generic;

public static class Economy
{
    public static int MinimumWage = 50;
    public static int BasePrice = 5;
    public static List<Resource> Resources = new()
    {
        new(IResource.Iron, BasePrice * 4),
        new(IResource.Gold, BasePrice * 10),
        new(IResource.Platinum, BasePrice * 20),
        new(IResource.Esmerald, BasePrice * 35),
        new(IResource.Diamond, BasePrice * 55),
    };
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