using System;
using System.Collections.Generic;

public class Biome
{
    public Biomes Type;
    public List<ElementType> BiomeTotalElements = new();
    public List<ElementType> BiomeCurrentElements = new();
    public List<Creatures> NativeCreatures = new();

    public Biome()
    {
        Type = Rng.RandEnum<Biomes>();
        switch (Type)
        {
            case Biomes.Cave:
                BiomeTotalElements.AddRange(new[] { ElementType.Darkness, ElementType.Earth });
                NativeCreatures.AddRange(new[] { Creatures.Bat, Creatures.Troll, Creatures.Dwarf, Creatures.Slime });
                break;
            case Biomes.Desert:
                BiomeTotalElements.AddRange(new[] { ElementType.Earth, ElementType.Fire });
                NativeCreatures.AddRange(new[] { Creatures.Scorpion, Creatures.Orc, Creatures.SandWorm });
                break;
            case Biomes.Forest:
                BiomeTotalElements.AddRange(new[] { ElementType.Earth, ElementType.Poison });
                NativeCreatures.AddRange(new[] { Creatures.Elf, Creatures.Wolve, Creatures.Bear, Creatures.Treant });
                break;
            case Biomes.Ice:
                BiomeTotalElements.AddRange(ElementType.Air, ElementType.Water);
                NativeCreatures.AddRange(new[] { Creatures.Yeti, Creatures.WhiteWolf, Creatures.IceGolem });
                break;
            case Biomes.Ocean:
                BiomeTotalElements.AddRange(new[] { ElementType.Water, ElementType.Lightning });
                NativeCreatures.AddRange(new[] { Creatures.Siren, Creatures.Kraken, Creatures.Fishman });
                break;
            case Biomes.Vulcan:
                BiomeTotalElements.AddRange(new[] { ElementType.Fire, ElementType.Earth });
                NativeCreatures.AddRange(new[] { Creatures.Dragon, Creatures.FireElemental, Creatures.Imp });
                break;
            case Biomes.Swamp:
                BiomeTotalElements.AddRange(new[] { ElementType.Poison, ElementType.Water, ElementType.Darkness });
                NativeCreatures.AddRange(new[] { Creatures.Slime, Creatures.Witch, Creatures.MosquitoGiant });
                break;
            case Biomes.SkySanctuary:
                BiomeTotalElements.AddRange(new[] { ElementType.Air, ElementType.Light, ElementType.Lightning });
                NativeCreatures.AddRange(new[] { Creatures.Harpy, Creatures.Griffin, Creatures.Angel });
                break;

            case Biomes.Graveyard:
                BiomeTotalElements.AddRange(new[] { ElementType.Darkness, ElementType.Poison });
                NativeCreatures.AddRange(new[] { Creatures.Skeleton, Creatures.Zombie, Creatures.Ghost, Creatures.Necromancer });
                break;

            case Biomes.CrystalCavern:
                BiomeTotalElements.AddRange(new[] { ElementType.Light, ElementType.Earth });
                NativeCreatures.AddRange(new[] { Creatures.Golem, Creatures.Dwarf, Creatures.ManaWisp });
                break;

            case Biomes.Ruins:
                BiomeTotalElements.AddRange(new[] { ElementType.Earth });
                NativeCreatures.AddRange(new[] { Creatures.Human, Creatures.Construct, Creatures.Bandit });
                break;
        }
        int elementCount = Rng.Rand.Next(1, 3);
        for (int i = 0; i < elementCount; i++)
        {
            var randElement = GetRandomBiomeElement();
            if (!BiomeCurrentElements.Contains(randElement) && randElement != ElementType.None) BiomeCurrentElements.Add(randElement);
        }
        
        if(BiomeCurrentElements.Count == 0)
        {
            BiomeCurrentElements.Add(ElementType.None);
        }
    }
    public Creatures GetRandomNativeCreature()
    {
        if (NativeCreatures.Count == 0) return Creatures.Human;
        return NativeCreatures[Rng.Rand.Next(NativeCreatures.Count)];
    }

    public ElementType GetRandomBiomeElement()
    {
        if (BiomeTotalElements.Count == 0) return ElementType.None;
        return BiomeTotalElements[Rng.Rand.Next(BiomeTotalElements.Count)];
    }
}

public enum Biomes
{
    Cave,
    Desert,
    Forest,
    Ice,
    Ocean,
    Vulcan,
    Swamp,
    SkySanctuary,
    Graveyard,
    CrystalCavern,
    Ruins
}