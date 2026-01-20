using System.Collections.Generic;

public enum ElementType
{
    None,
    Water,
    Earth,
    Fire,
    Air,
    Poison,
    Darkness,
    Light,
    Lightning,
    Ice,
}

public class ElementInfo
{
    public ElementType Type { get; set; }
    public List<ElementType> StrongAgainst { get; set; }
    public List<ElementType> WeakAgainst { get; set; }

    public ElementInfo(ElementType type)
    {
        Type = type;
        StrongAgainst = new();
        WeakAgainst = new();
        PopulateRelationships(type);
    }

    private void PopulateRelationships(ElementType element)
    {
        switch (element)
        {
            case ElementType.None:
                break;

            case ElementType.Water:
                StrongAgainst.AddRange(new[] {ElementType.Fire, ElementType.Earth });
                WeakAgainst.AddRange(new[] { ElementType.Lightning, ElementType.Ice, ElementType.Poison });
                break;

            case ElementType.Earth:
                StrongAgainst.AddRange(new[] { ElementType.Lightning, ElementType.Fire, ElementType.Poison });
                WeakAgainst.AddRange(new[] { ElementType.Water, ElementType.Air, ElementType.Ice });
                break;

            case ElementType.Fire:
                StrongAgainst.AddRange(new[] { ElementType.Ice, ElementType.Poison, ElementType.Darkness });
                WeakAgainst.AddRange(new[] { ElementType.Water, ElementType.Earth, ElementType.Air });
                break;

            case ElementType.Air:
                StrongAgainst.AddRange(new[] { ElementType.Earth, ElementType.Fire });
                WeakAgainst.AddRange(new[] { ElementType.Ice, ElementType.Lightning });
                break;

            case ElementType.Poison:
                StrongAgainst.AddRange(new[] { ElementType.Water, ElementType.Light });
                WeakAgainst.AddRange(new[] { ElementType.Fire, ElementType.Earth, ElementType.Ice });
                break;

            case ElementType.Darkness:
                StrongAgainst.AddRange(new[] { ElementType.Light, ElementType.Water });
                WeakAgainst.AddRange(new[] { ElementType.Light, ElementType.Lightning });
                break;

            case ElementType.Light:
                StrongAgainst.AddRange(new[] { ElementType.Darkness, ElementType.Poison });
                WeakAgainst.AddRange(new[] { ElementType.Darkness, ElementType.Ice });
                break;

            case ElementType.Lightning:
                StrongAgainst.AddRange(new[] { ElementType.Water, ElementType.Air, ElementType.Darkness });
                WeakAgainst.AddRange(new[] { ElementType.Earth });
                break;

            case ElementType.Ice:
                StrongAgainst.AddRange(new[] { ElementType.Water, ElementType.Earth, ElementType.Air, ElementType.Poison });
                WeakAgainst.AddRange(new[] { ElementType.Fire, ElementType.Lightning });
                break;
        }
    }
}