// Copyright (c) Eiveo GmbH. All rights reserved.

using Microsoft.Xna.Framework;

namespace RemnantOfTheAbyss;

/// <summary>Wraps the monogame api to avoid making most of the base class nullable.</summary>
public class GameWrapper : Game
{
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private GameMain? _game;

    /// <summary>Initializes a new instance of the <see cref="GameWrapper"/> class.</summary>
    public GameWrapper()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        _game = new GameMain(this);
    }

    /// <inheritdoc />
    protected override void Update(GameTime gameTime)
    {
        _game?.Update(gameTime);
    }

    /// <inheritdoc />
    protected override void Draw(GameTime gameTime)
    {
        _game?.Draw(gameTime);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _game?.Dispose();
        _graphicsDeviceManager.Dispose();
    }
}
