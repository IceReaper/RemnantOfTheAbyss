// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Drawing;
using System.Numerics;

namespace RemnantOfTheAbyss.Graphics;

/// <summary>Represents a camera.</summary>
public class Camera
{
    /// <summary>Gets or sets the position.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Gets or sets the rotation.</summary>
    public Vector3 Rotation { get; set; }

    /// <summary>Gets or sets the screen viewport.</summary>
    public Size ScreenViewport { get; set; }

    /// <summary>Gets the world viewport.</summary>
    public Size WorldViewport { get; private set; }

    /// <summary>Gets the view matrix.</summary>
    public Matrix4x4 View { get; private set; }

    /// <summary>Gets the projection matrix.</summary>
    public Matrix4x4 Projection { get; private set; }

    /// <summary>Gets or sets the target viewport.</summary>
    public Size TargetViewport { get; set; }

    /// <summary>Gets or sets the zoom.</summary>
    public float Zoom { get; set; } = 1;

    /// <summary>Updates the camera matrices.</summary>
    public void Update()
    {
        var targetAspect = TargetViewport.Width / TargetViewport.Height;
        var screenAspect = (float)ScreenViewport.Width / ScreenViewport.Height;

        var framebufferWidth = ((screenAspect > targetAspect ? (int)Math.Ceiling(TargetViewport.Height * screenAspect) : TargetViewport.Width) + 1) >> 1 << 1;
        var framebufferHeight = ((screenAspect < targetAspect ? (int)Math.Ceiling(TargetViewport.Width / screenAspect) : TargetViewport.Height) + 1) >> 1 << 1;

        WorldViewport = new Size(framebufferWidth, framebufferHeight);
        Projection = Matrix4x4.CreateOrthographicOffCenter(
            framebufferWidth / -2f * Zoom,
            framebufferWidth / 2f * Zoom,
            framebufferHeight / 2f * Zoom,
            framebufferHeight / -2f * Zoom,
            short.MinValue,
            short.MaxValue);

        var angleY = Rotation.Y * (float)(Math.PI / 180.0);
        var angleUp = Rotation.X * (float)(Math.PI / 180.0);

        var x = (float)Math.Cos(angleY) * (float)Math.Cos(angleUp);
        var y = (float)Math.Sin(angleUp);
        var z = (float)Math.Sin(angleY) * (float)Math.Cos(angleUp);

        var rotationMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.Normalize(new Vector3(x, y, z)), Vector3.UnitY);
        _ = Matrix4x4.Invert(rotationMatrix, out var inverseRotationMatrix);

        var position = Vector3.Transform(Vector3.Round(Vector3.Transform(Position, rotationMatrix)), inverseRotationMatrix);
        var direction = Vector3.Normalize(new Vector3(x, y, z));

        View = Matrix4x4.CreateLookAt(position, position + direction, Vector3.UnitY);
    }
}
