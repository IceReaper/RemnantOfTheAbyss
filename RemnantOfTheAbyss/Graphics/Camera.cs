// Copyright (c) Eiveo GmbH. All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RemnantOfTheAbyss.Graphics;

/// <summary>Represents a camera.</summary>
public class Camera
{
    /// <summary>Gets or sets the position.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Gets or sets the rotation.</summary>
    public Vector3 Rotation { get; set; }

    /// <summary>Gets or sets the screen viewport.</summary>
    public Viewport ScreenViewport { get; set; }

    /// <summary>Gets the world viewport.</summary>
    public Viewport WorldViewport { get; private set; }

    /// <summary>Gets the view matrix.</summary>
    public Matrix View { get; private set; }

    /// <summary>Gets the projection matrix.</summary>
    public Matrix Projection { get; private set; }

    /// <summary>Gets or sets the target viewport.</summary>
    public Vector2 TargetViewport { get; set; }

    /// <summary>Gets or sets the zoom.</summary>
    public float Zoom { get; set; } = 1;

    /// <summary>Updates the camera matrices.</summary>
    public void Update()
    {
        var baseAspect = TargetViewport.X / TargetViewport.Y;
        var windowAspect = (float)ScreenViewport.Width / ScreenViewport.Height;

        var framebufferWidth = (int)TargetViewport.X;
        var framebufferHeight = (int)TargetViewport.Y;

        if (windowAspect > baseAspect)
            framebufferWidth = (int)Math.Ceiling(TargetViewport.Y * windowAspect);
        else if (windowAspect < baseAspect)
            framebufferHeight = (int)Math.Ceiling(TargetViewport.X / windowAspect);

        framebufferWidth = (framebufferWidth + 1) >> 1 << 1;
        framebufferHeight = (framebufferHeight + 1) >> 1 << 1;

        WorldViewport = new Viewport(0, 0, framebufferWidth, framebufferHeight);
        Projection = Matrix.CreateOrthographicOffCenter(
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
        var rotationMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Normalize(new Vector3(x, y, z)), Vector3.Up);

        var transformedPosition = Vector3.Transform(Position, rotationMatrix);
        transformedPosition = new Vector3(MathF.Round(transformedPosition.X), MathF.Round(transformedPosition.Y), MathF.Round(transformedPosition.Z));

        var position = Vector3.Transform(transformedPosition, Matrix.Invert(rotationMatrix));
        var direction = Vector3.Normalize(new Vector3(x, y, z));

        View = Matrix.CreateLookAt(position, position + direction, Vector3.Up);
    }
}
