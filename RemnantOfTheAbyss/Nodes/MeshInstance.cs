// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Numerics;
using RemnantOfTheAbyss.Graphics;

namespace RemnantOfTheAbyss.Nodes;

/// <summary>Represents a mesh instance.</summary>
public class MeshInstance : Node
{
    /// <summary>Gets or sets the position.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Gets or sets the rotation.</summary>
    public Vector3 Rotation { get; set; }

    /// <summary>Gets or sets the scale.</summary>
    public Vector3 Scale { get; set; }

    /// <summary>Gets the mesh.</summary>
    public required Mesh Mesh { get; init; }

    /// <inheritdoc />
    public override void Draw(DeferredRenderer renderer)
    {
        renderer.Draw(Mesh, GlobalTransform);
    }
}
