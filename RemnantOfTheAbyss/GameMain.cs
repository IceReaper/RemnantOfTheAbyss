// Copyright (c) Eiveo GmbH. All rights reserved.

using Eiveo.TrenchBroom.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RemnantOfTheAbyss.Assets;
using RemnantOfTheAbyss.Entities;
using RemnantOfTheAbyss.Graphics;
using RemnantOfTheAbyss.Input;
using RemnantOfTheAbyss.Input.Enums;
using RemnantOfTheAbyss.Nodes;

namespace RemnantOfTheAbyss;

/// <summary>Represents the games base class.</summary>
public sealed class GameMain : IDisposable
{
    private readonly GameWrapper _gameWrapper;
    private readonly AssetManager _assetManager;
    private readonly DeferredRenderer _deferredRenderer;
    private readonly Camera _camera;
    private readonly World _world;
    private readonly InputManager _inputManager;

    /// <summary>Initializes a new instance of the <see cref="GameMain"/> class.</summary>
    /// <param name="gameWrapper">The game wrapper.</param>
    public GameMain(GameWrapper gameWrapper)
    {
        _gameWrapper = gameWrapper;

        _gameWrapper.IsFixedTimeStep = false;
        _gameWrapper.IsMouseVisible = true;
        _gameWrapper.Window.AllowUserResizing = true;
        _gameWrapper.Window.Title = "Remnant of the Abyss";
        _gameWrapper.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

        var dummyTexture = new Texture2D(_gameWrapper.GraphicsDevice, 1, 1);
        dummyTexture.SetData(new byte[] { 0xff, 0x00, 0xff, 0xff });

        _assetManager = new AssetManager
        {
            GraphicsDevice = _gameWrapper.GraphicsDevice,
            DummyTexture = dummyTexture,
        };

        _deferredRenderer = new DeferredRenderer(_gameWrapper.GraphicsDevice, _gameWrapper.Content);

        _camera = new Camera
        {
            Rotation = new Vector3(-45, 135, 0),
            TargetViewport = new Vector2(640, 480),
        };

        _world = new World();

        _inputManager = new InputManager();

        // TODO hardcoded content
        using var stream = File.OpenRead("Maps/Test.map");
        using var reader = new StreamReader(stream);

        var map = Map.Read(reader, name =>
        {
            var texture = _assetManager.LoadTexture(name, this);
            return new System.Numerics.Vector2(texture.Width, texture.Height);
        });

        var entitiesLoader = new EntitiesLoader();

        foreach (var entity in map.Entities)
        {
            var node = entitiesLoader.Load(entity);
            _world.Add(node);

            foreach (var mesh in MapLoader.CreateMeshes(entity, _gameWrapper.GraphicsDevice, name => _assetManager.LoadTexture(name, this)))
            {
                node.Add(new MeshInstance { Mesh = mesh });
            }
        }
    }

    /// <summary>Called when the game should update.</summary>
    /// <param name="gameTime">Snapshot of the game's timing state.</param>
    public void Update(GameTime gameTime)
    {
        _inputManager.Update();

        var move = Vector3.Zero;

        if (_inputManager.IsPressed(Keys.W) || _inputManager.GetGamePadThumbStick(GamePadThumbStick.Left).Y >= .5)
            move.Z += 1f;

        if (_inputManager.IsPressed(Keys.S) || _inputManager.GetGamePadThumbStick(GamePadThumbStick.Left).Y <= -.5)
            move.Z -= 1f;

        if (_inputManager.IsPressed(Keys.A) || _inputManager.GetGamePadThumbStick(GamePadThumbStick.Left).X <= -.5)
            move.X -= 1f;

        if (_inputManager.IsPressed(Keys.D) || _inputManager.GetGamePadThumbStick(GamePadThumbStick.Left).X >= .5)
            move.X += 1f;

        if (_inputManager.IsPressed(Keys.LeftControl) || _inputManager.IsPressed(Buttons.DPadUp))
            move.Y -= 1f;

        if (_inputManager.IsPressed(Keys.Space) || _inputManager.IsPressed(Buttons.DPadDown))
            move.Y += 1f;

        if (move.Length() > 1)
            move.Normalize();

        if (move.Length() > 0)
        {
            var angle = _camera.Rotation.Y * (float)(Math.PI / 180.0);

            var x = (-move.X * Math.Cos(angle)) + (-move.Z * Math.Sin(angle));
            var y = move.Y;
            var z = (move.X * Math.Sin(angle)) + (-move.Z * Math.Cos(angle));

            _camera.Position += new Vector3((float)x, y, (float)z) * (float)gameTime.ElapsedGameTime.TotalSeconds * 512;
        }

        if ((_inputManager.IsPressed(Keys.P) && !_inputManager.WasPressed(Keys.P)) || (_inputManager.IsPressed(Buttons.Back) && !_inputManager.WasPressed(Buttons.Back)))
            _deferredRenderer.Debug = !_deferredRenderer.Debug;

        _world.Update(gameTime);
    }

    /// <summary>Called when the game should draw a frame.</summary>
    /// <param name="gameTime">Snapshot of the game's timing state.</param>
    public void Draw(GameTime gameTime)
    {
        _gameWrapper.Window.Title = $"{1 / gameTime.ElapsedGameTime.TotalSeconds:N0} FPS";
        _camera.ScreenViewport = _gameWrapper.GraphicsDevice.Viewport;
        _camera.Update();

        _deferredRenderer.Begin(_camera);
        _world.Draw(_deferredRenderer);
        _deferredRenderer.End();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _world.Dispose();
        _assetManager.Dispose();
        _deferredRenderer.Dispose();
    }
}
