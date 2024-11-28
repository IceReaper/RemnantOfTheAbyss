// Copyright (c) Eiveo GmbH. All rights reserved.

using Eiveo.TrenchBroom.Attributes;
using Microsoft.Xna.Framework;
using RemnantOfTheAbyss.Nodes;
using Vector3 = System.Numerics.Vector3;

namespace RemnantOfTheAbyss.Entities.Functionals;

/// <summary>A sliding door.</summary>
[Entity("func_door_sliding", true, "A sliding door.")]
public partial class DoorSliding : Node
{
    private Func<DoorSliding, bool> _currentScript = Closed;
    private float _progress;

    /// <summary>Gets or sets the distance the door slides.</summary>
    [Property("The distance the door slides.")]
    public Vector3 Distance { get; set; } = new(0, 128, 0);

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
        var totalDistance = door.Distance.Length();
        var currentDistance = Math.Min(door.Speed * door._progress, totalDistance);
        var currentOffset = Microsoft.Xna.Framework.Vector3.Normalize(door.Distance) * currentDistance;

        if (door.Parent is World { Simulate2D: true })
            currentOffset = Microsoft.Xna.Framework.Vector3.Round(currentOffset / door.Step) * door.Step;

        foreach (var child in door.Children)
            child.LocalTransform = Matrix.CreateTranslation(currentOffset);

        if (currentDistance - totalDistance != 0)
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
        var totalDistance = door.Distance.Length();
        var currentDistance = totalDistance - Math.Min(door.Speed * door._progress, totalDistance);
        var currentOffset = Microsoft.Xna.Framework.Vector3.Normalize(door.Distance) * currentDistance;

        if (door.Parent is World { Simulate2D: true })
            currentOffset = Microsoft.Xna.Framework.Vector3.Round(currentOffset / door.Step) * door.Step;

        foreach (var child in door.Children)
            child.LocalTransform = Matrix.CreateTranslation(currentOffset);

        if (currentDistance != 0)
            return false;

        door._currentScript = Closed;
        door._progress = 0;
        return true;
    }
}
