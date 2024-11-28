// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Numerics;
using Eiveo.TrenchBroom.Attributes;
using Microsoft.Xna.Framework;
using RemnantOfTheAbyss.Nodes;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace RemnantOfTheAbyss.Entities.Functionals;

/// <summary>A sliding door.</summary>
[Entity("func_door_sliding", true, "A sliding door.")]
public partial class DoorSliding : Node
{
    private Func<DoorSliding, bool> _currentScript = Closed;
    private float _progress;

    /// <summary>Gets or sets the open direction.</summary>
    [Property("The open direction.")]
    public Vector3 Angles { get; set; }

    /// <summary>Gets or sets the distance the door slides.</summary>
    [Property("The distance the door slides.")]
    public float Distance { get; set; } = 128;

    /// <summary>Gets or sets the speed at which the door slides.</summary>
    [Property("The speed at which the door slides.")]
    public short Speed { get; set; } = 64;

    /// <summary>Gets or sets the animation step distance.</summary>
    [Property("The steps this door animates in.")]
    public short Step { get; set; } = 16;

    /// <summary>Gets or sets the number of seconds the door waits when open before closing.</summary>
    [Property("The number of seconds the door waits when open before closing.")]
    public byte Wait { get; set; } = 3;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _progress += (float)gameTime.ElapsedGameTime.TotalSeconds;

        while (true)
        {
            if (!_currentScript(this))
                break;
        }
    }

    private static bool Closed(DoorSliding door)
    {
        if (door._progress < door.Wait)
            return false;

        door._currentScript = Opening;
        door._progress = 0;
        return true;
    }

    private static bool Opening(DoorSliding door)
    {
        var rotation = -door.Angles * MathF.PI / 180;
        var direction = Vector3.Transform(new Vector3(1, 0, 0), Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.Z, rotation.X));
        var currentDistance = Math.Min(door.Speed * door._progress, door.Distance);
        var currentOffset = direction * currentDistance;

        if (door.Parent is World { Simulate2D: true })
            currentOffset = Vector3.Round(currentOffset / door.Step) * door.Step;

        foreach (var child in door.Children)
            child.LocalTransform = Matrix4x4.CreateTranslation(currentOffset);

        if (currentDistance - door.Distance != 0)
            return false;

        door._currentScript = Open;
        door._progress = 0;
        return true;
    }

    private static bool Open(DoorSliding door)
    {
        if (door._progress < door.Wait)
            return false;

        door._currentScript = Closing;
        door._progress = 0;
        return true;
    }

    private static bool Closing(DoorSliding door)
    {
        var rotation = -door.Angles * MathF.PI / 180;
        var direction = Vector3.Transform(new Vector3(1, 0, 0), Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.Z, rotation.X));
        var currentDistance = door.Distance - Math.Min(door.Speed * door._progress, door.Distance);
        var currentOffset = direction * currentDistance;

        if (door.Parent is World { Simulate2D: true })
            currentOffset = Vector3.Round(currentOffset / door.Step) * door.Step;

        foreach (var child in door.Children)
            child.LocalTransform = Matrix4x4.CreateTranslation(currentOffset);

        if (currentDistance != 0)
            return false;

        door._currentScript = Closed;
        door._progress = 0;
        return true;
    }
}
