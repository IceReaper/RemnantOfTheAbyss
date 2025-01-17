// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Drawing;
using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RemnantOfTheAbyss.Extensions;

namespace RemnantOfTheAbyss.Graphics;

/// <summary>Represents a deferred renderer.</summary>
public sealed class DeferredRenderer : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly Effect _renderInfoBuffersEffect;

    private RenderTarget2D _position;
    private RenderTarget2D _normal;
    private RenderTarget2D _albedo;

    private Size _viewport;

    /// <summary>Initializes a new instance of the <see cref="DeferredRenderer"/> class.</summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="content">The content manager.</param>
    public DeferredRenderer(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(_graphicsDevice);
        _renderInfoBuffersEffect = content.Load<Effect>("Effects/RenderIntoBuffers");

        var viewport = _graphicsDevice.Viewport;

        _position = new RenderTarget2D(_graphicsDevice, viewport.Width, viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        _normal = new RenderTarget2D(_graphicsDevice, viewport.Width, viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        _albedo = new RenderTarget2D(_graphicsDevice, viewport.Width, viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
    }

    /// <summary>Gets or sets a value indicating whether to draw debug information.</summary>
    public bool Debug { get; set; }

    /// <summary>Begin rendering.</summary>
    /// <param name="camera">The camera.</param>
    public void Begin(Camera camera)
    {
        _viewport = camera.ScreenViewport;

        var worldViewport = camera.WorldViewport;

        if (_position.Width != worldViewport.Width || _position.Height != worldViewport.Height)
        {
            _position.Dispose();
            _position = new RenderTarget2D(_graphicsDevice, worldViewport.Width, worldViewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }

        if (_normal.Width != worldViewport.Width || _normal.Height != worldViewport.Height)
        {
            _normal.Dispose();
            _normal = new RenderTarget2D(_graphicsDevice, worldViewport.Width, worldViewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        if (_albedo.Width != worldViewport.Width || _albedo.Height != worldViewport.Height)
        {
            _albedo.Dispose();
            _albedo = new RenderTarget2D(_graphicsDevice, worldViewport.Width, worldViewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        _graphicsDevice.SetRenderTargets(_position, _normal, _albedo);
        _graphicsDevice.Clear(Color.Empty.ToMonoGame());

        _renderInfoBuffersEffect.Parameters["View"].SetValue(camera.View);
        _renderInfoBuffersEffect.Parameters["Projection"].SetValue(camera.Projection);

        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    /// <summary>Draws a mesh.</summary>
    /// <param name="mesh">The mesh to draw.</param>
    /// <param name="transform">The transformation matrix.</param>
    public void Draw(Mesh mesh, Matrix4x4 transform)
    {
        _graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
        _graphicsDevice.Indices = mesh.IndexBuffer;

        _renderInfoBuffersEffect.Parameters["Model"].SetValue(transform);
        _renderInfoBuffersEffect.Parameters["AlbedoTexture"].SetValue(mesh.AlbedoTexture);

        _renderInfoBuffersEffect.CurrentTechnique.Passes[0].Apply();
        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.IndexBuffer.IndexCount / 3);
    }

    /// <summary>End rendering and draw the result.</summary>
    public void End()
    {
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.Black.ToMonoGame());

        // TODO this should be the final output!
        var outputs = new List<RenderTarget2D> { _albedo };

        if (Debug)
        {
            outputs.Add(_position);
            outputs.Add(_normal);
            outputs.Add(_albedo);
        }

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var outputsX = (int)Math.Ceiling(Math.Sqrt(outputs.Count));
        var outputsY = (int)Math.Ceiling(outputs.Count / (double)outputsX);
        var width = _viewport.Width / outputsX;
        var height = _viewport.Height / outputsY;

        for (var y = 0; y < outputsY; y++)
        {
            for (var x = 0; x < outputsX; x++)
            {
                var index = x + (y * outputsX);

                if (index >= outputs.Count)
                    break;

                _spriteBatch.Draw(
                    outputs[index],
                    new Rectangle(x * width, y * height, width, height).ToMonoGame(),
                    null,
                    Color.White.ToMonoGame(),
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally,
                    0);
            }
        }

        _spriteBatch.End();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _position.Dispose();
        _normal.Dispose();
        _albedo.Dispose();

        _renderInfoBuffersEffect.Dispose();
        _spriteBatch.Dispose();
    }
}
