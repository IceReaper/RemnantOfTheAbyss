// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Collections.ObjectModel;
using System.Numerics;

namespace Eiveo.TrenchBroom.Maps;

/// <summary>Represents a brush.</summary>
public class Brush
{
    /// <summary>Gets the planes of the brush.</summary>
    public Collection<Plane> Planes { get; } = [];

    /// <summary>Reads a brush from a stream.</summary>
    /// <param name="reader">The reader to use for reading.</param>
    /// <param name="getTextureSize">The function to use for getting the size of the texture.</param>
    /// <returns>The brush that was read.</returns>
    public static Brush Read(StreamReader reader, Func<string, Vector2> getTextureSize)
    {
        var brush = new Brush();

        while (true)
        {
            var line = reader.ReadLine() ?? throw new FormatException("Brush: Unexpected end of file.");

            switch (line[0])
            {
                case '(':
                    brush.Planes.Add(Plane.Read(line, getTextureSize));
                    break;

                case '}':
                    return brush;

                case '/':
                    continue;

                default:
                    throw new FormatException("Brush: Unexpected character.");
            }
        }
    }
}
