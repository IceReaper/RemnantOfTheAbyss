// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Numerics;
using Eiveo.TrenchBroom.Maps;
using Microsoft.Xna.Framework.Graphics;
using RemnantOfTheAbyss.Graphics;
using Plane = Eiveo.TrenchBroom.Maps.Plane;

namespace RemnantOfTheAbyss.Nodes;

/// <summary>Loads a trenchbroom map into the game.</summary>
public static class MapLoader
{
    private const float Delta = 1f / 16;

    /// <summary>Creates meshes from the entity.</summary>
    /// <param name="entity">The entity to create meshes from.</param>
    /// <param name="graphicsDevice">The graphics device to use.</param>
    /// <param name="getTexture">The function to use for getting a texture.</param>
    /// <returns>The meshes that were created.</returns>
    public static Mesh[] CreateMeshes(Entity entity, GraphicsDevice graphicsDevice, Func<string, Texture2D> getTexture)
    {
        var meshes = new Dictionary<string, Mesh>();

        foreach (var brush in entity.Brushes)
        {
            var brushMeshes = CreateMeshes(brush, graphicsDevice, getTexture);

            foreach (var (material, mesh) in brushMeshes)
            {
                if (!meshes.TryAdd(material, mesh))
                    meshes[material].Merge(mesh);
            }
        }

        return [.. meshes.Values];
    }

    private static Dictionary<string, Mesh> CreateMeshes(Brush brush, GraphicsDevice graphicsDevice, Func<string, Texture2D> getTexture)
    {
        var polygons = new Dictionary<Plane, List<VertexPositionNormalTexture>>();

        for (var i = 0; i < brush.Planes.Count - 2; i++)
        {
            for (var j = i + 1; j < brush.Planes.Count - 1; j++)
            {
                for (var k = j + 1; k < brush.Planes.Count; k++)
                {
                    var plane1 = brush.Planes[i];
                    var plane2 = brush.Planes[j];
                    var plane3 = brush.Planes[k];

                    if (!GetIntersection(plane1, plane2, plane3, out var intersection))
                        continue;

                    if (brush.Planes.Any(plane => Vector3.Dot(plane.Normal, intersection) - plane.Distance > Delta))
                        continue;

                    _ = polygons.TryAdd(plane1, []);
                    _ = polygons.TryAdd(plane2, []);
                    _ = polygons.TryAdd(plane3, []);

                    polygons[plane1].Add(new VertexPositionNormalTexture
                    {
                        Position = intersection,
                        Normal = plane1.Normal,
                        TextureCoordinate = CalculateUv(plane1, intersection),
                    });

                    polygons[plane2].Add(new VertexPositionNormalTexture
                    {
                        Position = intersection,
                        Normal = plane2.Normal,
                        TextureCoordinate = CalculateUv(plane2, intersection),
                    });

                    polygons[plane3].Add(new VertexPositionNormalTexture
                    {
                        Position = intersection,
                        Normal = plane3.Normal,
                        TextureCoordinate = CalculateUv(plane3, intersection),
                    });
                }
            }
        }

        var meshes = new Dictionary<string, Mesh>();

        foreach (var group in polygons.GroupBy(polygon => polygon.Key.Texture))
        {
            if (group.Key == "__TB_empty")
                continue;

            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            foreach (var (plane, planeVertices) in group)
            {
                var firstVertex = vertices.Count;
                var processedVertices = SortVertices(plane, planeVertices.DistinctBy(vertex => vertex.Position).ToArray());

                if (processedVertices.Length < 3)
                    continue;

                vertices.AddRange(processedVertices);

                for (var i = 0; i < processedVertices.Length - 2; i++)
                {
                    indices.Add((ushort)firstVertex);
                    indices.Add((ushort)(firstVertex + i + 1));
                    indices.Add((ushort)(firstVertex + i + 2));
                }
            }

            if (vertices.Count < 3)
                continue;

            vertices = vertices.Select(vertex => new VertexPositionNormalTexture(
                new Vector3(vertex.Position.X, vertex.Position.Z, vertex.Position.Y),
                vertex.Normal,
                vertex.TextureCoordinate)).ToList();

            meshes.Add(group.Key, new Mesh([.. vertices], [.. indices])
            {
                GraphicsDevice = graphicsDevice,
                AlbedoTexture = getTexture(group.Key),
            });
        }

        return meshes;
    }

    private static bool GetIntersection(Plane a, Plane b, Plane c, out Vector3 intersection)
    {
        var denom = Vector3.Dot(a.Normal, Vector3.Cross(b.Normal, c.Normal));

        if (Math.Abs(denom) < Delta)
        {
            intersection = Vector3.Zero;
            return false;
        }

        intersection = (
            (Vector3.Cross(b.Normal, c.Normal) * a.Distance) +
            (Vector3.Cross(c.Normal, a.Normal) * b.Distance) +
            (Vector3.Cross(a.Normal, b.Normal) * c.Distance)) / denom;
        return true;
    }

    private static VertexPositionNormalTexture[] SortVertices(Plane plane, VertexPositionNormalTexture[] vertices)
    {
        var center = vertices.Aggregate(Vector3.Zero, (current, vertex) => current + vertex.Position.ToNumerics()) / vertices.Length;

        var a = (plane.Normal - Vector3.UnitX).Length() < Delta || (plane.Normal + Vector3.UnitX).Length() < Delta ? Vector3.UnitY : Vector3.UnitX;
        var b = Vector3.Normalize(Vector3.Cross(plane.Normal, a));
        var c = Vector3.Normalize(Vector3.Cross(plane.Normal, b));

        return [.. vertices.OrderByDescending(vertex => (float)Math.Atan2(Vector3.Dot(vertex.Position.ToNumerics() - center, c), Vector3.Dot(vertex.Position.ToNumerics() - center, b)))];
    }

    private static Vector2 CalculateUv(Plane plane, Vector3 vertex)
    {
        var v0 = plane.Vertex2.Position - plane.Vertex1.Position;
        var v1 = plane.Vertex3.Position - plane.Vertex1.Position;
        var v2 = vertex - plane.Vertex1.Position;

        var dot00 = Vector3.Dot(v0, v0);
        var dot01 = Vector3.Dot(v0, v1);
        var dot02 = Vector3.Dot(v0, v2);
        var dot11 = Vector3.Dot(v1, v1);
        var dot12 = Vector3.Dot(v1, v2);

        var denom = (dot00 * dot11) - (dot01 * dot01);

        if (Math.Abs(denom) < Delta)
            return Vector2.Zero;

        var u = ((dot11 * dot02) - (dot01 * dot12)) / denom;
        var v = ((dot00 * dot12) - (dot01 * dot02)) / denom;

        var uv0 = plane.Vertex1.Texture;
        var uv1 = plane.Vertex2.Texture;
        var uv2 = plane.Vertex3.Texture;

        return uv0 + (u * (uv1 - uv0)) + (v * (uv2 - uv0));
    }
}
