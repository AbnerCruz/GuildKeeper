using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

public enum BuildCategory
{
    Structure,
    Floor,
    Furniture,
    Production,
    Medical,
    Joy,
    Decor
}

public class Buildable
{
    public int ID { get; set; }
    public string stringID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Point Size { get; set; }
    public int GoldCost { get; set; }
    //public List<Resource> ResourceCost { get; set; }
    public Color PlaceholderColor { get; set; }
    public BuildCategory Category;
}

public static class BuildDatabase
{
    public static List<Buildable> AllItems = new();

    public static void Initialize()
    {
        //STRUCTURES
        AllItems.Add(new()
        {
            ID = 1,
            Name = "Rock Wall",
            Category = BuildCategory.Structure,
            Size = new Point(1, 1),
            GoldCost = 0,
            PlaceholderColor = Color.Gray
        });
        //FLOORS
        AllItems.Add(new()
        {
            ID = 10,
            Name = "Dirty Path",
            Category = BuildCategory.Floor,
            Size = new Point(1, 1),
            GoldCost = 0,
            PlaceholderColor = Color.Brown
        });
        AllItems.Add(new()
        {
            ID = 11,
            Name = "Wood Floor",
            Category = BuildCategory.Floor,
            Size = new Point(1, 1),
            GoldCost = 5,
            PlaceholderColor = Color.BurlyWood
        });

        //FURNITURE
        AllItems.Add(new()
        {
            ID = 12,
            Name = "Straw Bed",
            Category = BuildCategory.Furniture,
            Size = new Point(1, 2),
            GoldCost = 15,
            PlaceholderColor = Color.Yellow
        });

        //MEDICAL
        AllItems.Add(new()
        {
            ID = 13,
            Name = "Herbal Bed",
            Category = BuildCategory.Medical,
            Size = new Point(1, 2),
            GoldCost = 50,
            PlaceholderColor = Color.LightGreen
        });

        //Training
        AllItems.Add(new()
        {
            ID = 14,
            Name = "Dummy",
            Category = BuildCategory.Production,
            Size = new Point(1, 1),
            GoldCost = 20,
            PlaceholderColor = Color.Red
        });
    }

    public static List<Buildable> GetByCategory(BuildCategory category)
    {
        return AllItems.Where(x => x.Category == category).ToList();
    }

}
