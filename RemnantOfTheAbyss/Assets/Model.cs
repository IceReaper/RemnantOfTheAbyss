// Copyright (c) Eiveo GmbH. All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;
using Mesh = RemnantOfTheAbyss.Graphics.Mesh;

namespace RemnantOfTheAbyss.Assets;

/// <summary>Represents a model, which consists of several meshes.</summary>
public class Model
{
    /// <summary>Gets the meshes this model contains.</summary>
    public required IReadOnlyCollection<Mesh> Meshes { get; init; }

    /// <summary>Creates a new model from the given path.</summary>
    /// <param name="graphicsDevice">The graphics device to use.</param>
    /// <param name="assetManager">The asset manager to use.</param>
    /// <param name="path">The path to the model file.</param>
    /// <returns>The created model.</returns>
    public static Model FromPath(GraphicsDevice graphicsDevice, AssetManager assetManager, string path)
    {
        var modelRoot = ModelRoot.Load(path);
        var vertices = new List<VertexPositionNormalTexture>();

        foreach (var triangle in modelRoot.DefaultScene.EvaluateTriangles())
        {
            foreach (var vertex in new[] { triangle.C, triangle.B, triangle.A })
            {
                var geometry = vertex.GetGeometry();
                var material = vertex.GetMaterial();

                var position = geometry.GetPosition();
                _ = geometry.TryGetNormal(out var normal);
                var texCoord = material.GetTexCoord(0);

                vertices.Add(new VertexPositionNormalTexture(position with { Z = -position.Z }, normal with { Z = -normal.Z }, texCoord));
            }
        }

        var texture = modelRoot.LogicalMaterials[0].FindChannel("BaseColor")?.Texture.Name ?? throw new KeyNotFoundException("BaseColor texture not found.");
        texture = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, texture);

        var mesh = new Mesh([.. vertices], Enumerable.Range(0, vertices.Count).Select(e => (ushort)e).ToArray()) { GraphicsDevice = graphicsDevice, };
        mesh.AlbedoTexture = assetManager.LoadTexture(texture, mesh);

        return new Model { Meshes = [mesh] };
    }
}
