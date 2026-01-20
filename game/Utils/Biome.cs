using System;
using System.Collections.Generic;

public class Biome
{
    public Biomes Type;
    public List<Elements> BiomeTotalElements = new();
    public List<Elements> BiomeCurrentElements = new() { Elements.None };
    public List<Creatures> NativeCreatures = new();

    public Biome()
    {
        Type = Enum.GetValues<Biomes>()[Rng.Rand.Next(0, 2)];
        switch (Type)
        {
            case Biomes.Cave:
                BiomeTotalElements.AddRange(new[] { Elements.Darkness, Elements.Earth });
                NativeCreatures.AddRange(new[] { Creatures.Bat, Creatures.Troll, Creatures.Dwarf, Creatures.Slime });
                break;
            case Biomes.Desert:
                BiomeTotalElements.AddRange(new[] { Elements.Earth, Elements.Fire });
                NativeCreatures.AddRange(new[] { Creatures.Scorpion, Creatures.Orc, Creatures.SandWorm });
                break;
            case Biomes.Forest:
                BiomeTotalElements.AddRange(new[] { Elements.Earth, Elements.Poison });
                NativeCreatures.AddRange(new[] { Creatures.Elf, Creatures.Wolve, Creatures.Bear, Creatures.Treant });
                break;
            case Biomes.Ice:
                BiomeTotalElements.AddRange(Elements.Air, Elements.Water);
                NativeCreatures.AddRange(new[] { Creatures.Yeti, Creatures.WhiteWolf, Creatures.IceGolem });
                break;
            case Biomes.Ocean:
                BiomeTotalElements.AddRange(new[] { Elements.Water, Elements.Lightning });
                NativeCreatures.AddRange(new[] { Creatures.Siren, Creatures.Kraken, Creatures.Fishman });
                break;
            case Biomes.Vulcan:
                BiomeTotalElements.AddRange(new[] { Elements.Fire, Elements.Earth, Elements.Metal });
                NativeCreatures.AddRange(new[] { Creatures.Dragon, Creatures.FireElemental, Creatures.Imp });
                break;
            case Biomes.Swamp:
                BiomeTotalElements.AddRange(new[] { Elements.Poison, Elements.Water, Elements.Darkness });
                NativeCreatures.AddRange(new[] { Creatures.Slime, Creatures.Witch, Creatures.MosquitoGiant });
                break;
            case Biomes.SkySanctuary:
                BiomeTotalElements.AddRange(new[] { Elements.Air, Elements.Light, Elements.Lightning });
                NativeCreatures.AddRange(new[] { Creatures.Harpy, Creatures.Griffin, Creatures.Angel });
                break;

            case Biomes.Graveyard:
                BiomeTotalElements.AddRange(new[] { Elements.Darkness, Elements.Spirit, Elements.Poison });
                NativeCreatures.AddRange(new[] { Creatures.Skeleton, Creatures.Zombie, Creatures.Ghost, Creatures.Necromancer });
                break;

            case Biomes.CrystalCavern:
                BiomeTotalElements.AddRange(new[] { Elements.Arcane, Elements.Light, Elements.Earth });
                NativeCreatures.AddRange(new[] { Creatures.Golem, Creatures.Dwarf, Creatures.ManaWisp });
                break;

            case Biomes.Ruins:
                BiomeTotalElements.AddRange(new[] { Elements.Physical, Elements.Metal });
                NativeCreatures.AddRange(new[] { Creatures.Human, Creatures.Construct, Creatures.Bandit });
                break;
        }
        for(int i = 0; i < Rng.Rand.Next(0, 2); i++)
        {
            var randElement = GetRandomBiomeElement();
            if (!BiomeCurrentElements.Contains(randElement)) BiomeCurrentElements.Add(randElement);
        }
    }
    public Creatures GetRandomNativeCreature()
    {
        if (NativeCreatures.Count == 0) return Creatures.Human;
        return NativeCreatures[Rng.Rand.Next(NativeCreatures.Count)];
    }

    public Elements GetRandomBiomeElement()
    {
        if (BiomeTotalElements.Count == 0) return Elements.None;
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