// Copyright (c) Eiveo GmbH. All rights reserved.

using RemnantOfTheAbyss.Nodes;

namespace RemnantOfTheAbyss.Entities;

/// <summary>Represents the world.</summary>
public class World : Node
{
    /// <summary>Gets or sets a value indicating whether the game is simulating 2d.</summary>
    public bool Simulate2D { get; set; } = true;
}
