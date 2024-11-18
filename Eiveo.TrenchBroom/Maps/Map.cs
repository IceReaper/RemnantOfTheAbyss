// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Collections.ObjectModel;
using System.Numerics;

namespace Eiveo.TrenchBroom.Maps;

/// <summary>Represents a map.</summary>
public class Map
{
    /// <summary>Gets the entities in the map.</summary>
    public Collection<Entity> Entities { get; } = [];

    /// <summary>Reads a map from a stream.</summary>
    /// <param name="reader">The reader to use for reading.</param>
    /// <param name="getTextureSize">The function to use for getting the size of the texture.</param>
    /// <returns>The map that was read.</returns>
    public static Map Read(StreamReader reader, Func<string, Vector2> getTextureSize)
    {
        var map = new Map();

        while (true)
        {
            var line = reader.ReadLine();

            if (line == null)
                return map;

            switch (line[0])
            {
                case '/':
                    continue;

                case '{':
                    map.Entities.Add(Entity.Read(reader, getTextureSize));
                    break;

                default:
                    throw new FormatException("Map: Unexpected character.");
            }
        }
    }
}
