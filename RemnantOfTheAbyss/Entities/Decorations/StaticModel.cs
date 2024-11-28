// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Numerics;
using Eiveo.TrenchBroom.Attributes;
using Microsoft.Xna.Framework;
using RemnantOfTheAbyss.Assets;
using RemnantOfTheAbyss.Nodes;
using Vector3 = System.Numerics.Vector3;

namespace RemnantOfTheAbyss.Entities.Decorations;

/// <summary>A static model.</summary>
[Entity("deco_model_static", false, "A static model.")]
public partial class StaticModel : Node
{
    private Model? _model;

    /// <summary>Gets or sets the model to use.</summary>
    [Property("The Model to use.")]
    [Model]
    public string? Model { get; set; }

    /// <summary>Gets or sets the rotation.</summary>
    [Property("The rotation.")]
    public Vector3 Angles { get; set; }

    /// <summary>Gets or sets the position.</summary>
    [Property("The position.")]
    public Vector3 Origin { get; set; }

    /// <summary>Gets or sets the scale.</summary>
    [Property("The scale.")]
    [ModelScale(nameof(Model))]
    public Vector3 Scale { get; set; } = new(1, 1, 1);

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (_model == null && Model != null)
        {
            _model = World?.AssetManager.LoadModel(Model, this);

            if (_model == null)
                return;

            foreach (var mesh in _model.Meshes)
                Add(new MeshInstance { Mesh = mesh });
        }

        var rotation = -Angles * MathF.PI / 180;
        LocalTransform = Matrix4x4.CreateScale(Scale.X, Scale.Z, Scale.Y)
                         * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.Z, rotation.X)
                         * Matrix4x4.CreateTranslation(Origin.X, Origin.Z, Origin.Y);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        World?.AssetManager.Unload(this);

        base.Dispose(disposing);
    }
}
