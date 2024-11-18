// Copyright (c) Eiveo GmbH. All rights reserved.

using Eiveo.TrenchBroom.Attributes;
using Eiveo.TrenchBroom.Maps;
using RemnantOfTheAbyss.Nodes;

namespace RemnantOfTheAbyss.Entities;

/// <summary>Loads the entities.</summary>
[EntitiesLoader("Remnant of the Abyss")]
public partial class EntitiesLoader
{
    /// <summary>Loads an entity.</summary>
    /// <param name="entity">The entity to load.</param>
    /// <returns>The loaded entity.</returns>
    public partial Node Load(Entity entity);

    private static Node UnknownEntity(Entity entity)
    {
        Console.WriteLine($"Unknown entity: {entity.Properties["classname"]}");
        return new Node();
    }
}
