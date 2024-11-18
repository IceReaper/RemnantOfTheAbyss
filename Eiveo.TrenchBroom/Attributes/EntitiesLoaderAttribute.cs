// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.TrenchBroom.Attributes;

/// <summary>Indicates that the class is an entities loader.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EntitiesLoaderAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="EntitiesLoaderAttribute"/> class.</summary>
    /// <param name="game">The game name.</param>
    public EntitiesLoaderAttribute(string game)
    {
        Game = game;
    }

    /// <summary>Gets the id of the entity in the editor.</summary>
    public string Game { get; }
}
