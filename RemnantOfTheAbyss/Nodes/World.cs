// Copyright (c) Eiveo GmbH. All rights reserved.

using RemnantOfTheAbyss.Assets;

namespace RemnantOfTheAbyss.Nodes;

/// <summary>Represents the world.</summary>
public class World : Node
{
    /// <summary>Gets the asset manager.</summary>
    public required AssetManager AssetManager { get; init; }

    /// <summary>Gets or sets a value indicating whether the game is simulating 2d.</summary>
    public bool Simulate2D { get; set; } = true;
}
