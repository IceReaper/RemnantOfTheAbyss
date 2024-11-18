// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Eiveo.TrenchBroom.Maps;

/// <summary>Represents an entity.</summary>
public class Entity
{
    /// <summary>Gets the properties of the entity.</summary>
    public Dictionary<string, string> Properties { get; } = [];

    /// <summary>Gets the brushes of the entity.</summary>
    public Collection<Brush> Brushes { get; } = [];

    /// <summary>Reads an entity from a stream.</summary>
    /// <param name="reader">The reader to use for reading.</param>
    /// <param name="getTextureSize">The function to use for getting the size of the texture.</param>
    /// <returns>The entity that was read.</returns>
    public static Entity Read(StreamReader reader, Func<string, Vector2> getTextureSize)
    {
        var entity = new Entity();

        while (true)
        {
            var line = reader.ReadLine() ?? throw new FormatException("Entity: Unexpected end of file.");

            switch (line[0])
            {
                case '{':
                    entity.Brushes.Add(Brush.Read(reader, getTextureSize));
                    break;

                case '"':
                    var match = Regex.Match(line, @"^""(.*?)(?<!\\)"" (?<!\\)""(.*?)(?<!\\)""$");

                    if (!match.Success)
                    {
                        throw new FormatException("Entity: Invalid property.");
                    }

                    entity.Properties[match.Groups[1].Value] = match.Groups[2].Value;
                    break;

                case '}':
                    return entity;

                case '/':
                    continue;

                default:
                    throw new FormatException("Entity: Unexpected character.");
            }
        }
    }
}
