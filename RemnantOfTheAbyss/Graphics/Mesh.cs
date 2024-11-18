// Copyright (c) Eiveo GmbH. All rights reserved.

using Microsoft.Xna.Framework.Graphics;

namespace RemnantOfTheAbyss.Graphics;

/// <summary>Represents a mesh.</summary>
public sealed class Mesh : IDisposable
{
    private readonly List<VertexPositionNormalTexture> _vertices = [];
    private readonly List<ushort> _indices = [];

    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;

    /// <summary>Initializes a new instance of the <see cref="Mesh"/> class.</summary>
    /// <param name="vertices">The vertices of the mesh.</param>
    /// <param name="indices">The indices of the mesh.</param>
    public Mesh(VertexPositionNormalTexture[]? vertices = null, ushort[]? indices = null)
    {
        if (vertices != null)
            _vertices.AddRange(vertices);

        if (indices != null)
            _indices.AddRange(indices);
    }

    /// <summary>Gets the graphics device.</summary>
    public required GraphicsDevice GraphicsDevice { get; init; }

    /// <summary>Gets or sets the texture of the mesh.</summary>
    public Texture2D? AlbedoTexture { get; set; }

    /// <summary>Gets the index buffer.</summary>
    public VertexBuffer VertexBuffer
    {
        get
        {
            if (_vertexBuffer != null)
                return _vertexBuffer;

            _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), _vertices.Count, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices.ToArray());

            return _vertexBuffer;
        }
    }

    /// <summary>Gets the index buffer.</summary>
    public IndexBuffer IndexBuffer
    {
        get
        {
            if (_indexBuffer != null)
                return _indexBuffer;

            _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), _indices.Count, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices.ToArray());

            return _indexBuffer;
        }
    }

    /// <summary>Merges the mesh with another mesh.</summary>
    /// <param name="mesh">The mesh to merge with.</param>
    public void Merge(Mesh mesh)
    {
        _indices.AddRange(mesh._indices.Select(index => (ushort)(index + _vertices.Count)));
        _vertices.AddRange(mesh._vertices);

        _vertexBuffer?.Dispose();
        _vertexBuffer = null;

        _indexBuffer?.Dispose();
        _indexBuffer = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _indexBuffer?.Dispose();
        _vertexBuffer?.Dispose();
    }
}
