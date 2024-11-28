// Copyright (c) Eiveo GmbH. All rights reserved.

using Microsoft.Xna.Framework;

namespace RemnantOfTheAbyss.Extensions;

/// <summary>Contains extension methods for Microsoft.Xna.Framework.</summary>
public static class MonoGameExtensions
{
    /// <summary>Converts a System.Drawing.Color to a Microsoft.Xna.Framework.Color.</summary>
    /// <param name="value">The color to convert.</param>
    /// <returns>The converted color.</returns>
    public static Color ToMonoGame(this System.Drawing.Color value)
    {
        return new Color(value.R, value.G, value.B, value.A);
    }

    /// <summary>Converts a System.Drawing.Rectangle to a Microsoft.Xna.Framework.Rectangle.</summary>
    /// <param name="value">The rectangle to convert.</param>
    /// <returns>The converted rectangle.</returns>
    public static Rectangle ToMonoGame(this System.Drawing.Rectangle value)
    {
        return new Rectangle(value.X, value.Y, value.Width, value.Height);
    }
}
