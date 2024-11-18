// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Eiveo.TrenchBroom.Maps;

/// <summary>Represents a plane.</summary>
public class Plane
{
    /// <summary>Gets the first vertex of the plane.</summary>
    public (Vector3 Position, Vector2 Texture) Vertex1 { get; private set; }

    /// <summary>Gets the first vertex of the plane.</summary>
    public (Vector3 Position, Vector2 Texture) Vertex2 { get; private set; }

    /// <summary>Gets the first vertex of the plane.</summary>
    public (Vector3 Position, Vector2 Texture) Vertex3 { get; private set; }

    /// <summary>Gets the texture of the plane.</summary>
    public string Texture { get; private set; } = string.Empty;

    /// <summary>Gets the normal of the plane.</summary>
    public Vector3 Normal { get; private set; }

    /// <summary>Gets the distance of the plane.</summary>
    public float Distance { get; private set; }

    /// <summary>Reads a plane from a string.</summary>
    /// <param name="line">The string to read from.</param>
    /// <param name="getTextureSize">The function to use for getting the size of the texture.</param>
    /// <returns>The plane that was read.</returns>
    public static Plane Read(string line, Func<string, Vector2> getTextureSize)
    {
        var plane = new Plane();

        const string valueRegex = @"([^\s]+)";
        const string vertexRegex = $@"\( {valueRegex} {valueRegex} {valueRegex} \)";
        const string textureRegex = $@"\[ {valueRegex} {valueRegex} {valueRegex} {valueRegex} \]";
        const string planeRegex = $"^{vertexRegex} {vertexRegex} {vertexRegex} (.+) {textureRegex} {textureRegex} {valueRegex} {valueRegex} {valueRegex}$";

        var match = Regex.Match(line, planeRegex);

        if (!match.Success)
            throw new FormatException("Plane: Invalid format.");

        var position1 = new Vector3(
            float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));

        var position2 = new Vector3(
            float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture));

        var position3 = new Vector3(
            float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture));

        plane.Texture = match.Groups[10].Value;

        var u = new Vector3(
            float.Parse(match.Groups[11].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[12].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[13].Value, CultureInfo.InvariantCulture));

        var offsetX = float.Parse(match.Groups[14].Value, CultureInfo.InvariantCulture);

        var v = new Vector3(
            float.Parse(match.Groups[15].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[16].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[17].Value, CultureInfo.InvariantCulture));

        var offsetY = float.Parse(match.Groups[18].Value, CultureInfo.InvariantCulture);

        var scaleX = float.Parse(match.Groups[20].Value, CultureInfo.InvariantCulture);
        var scaleY = float.Parse(match.Groups[21].Value, CultureInfo.InvariantCulture);

        var axisU = u / scaleX;
        var axisV = v / scaleY;

        var textureSize = getTextureSize(plane.Texture);

        plane.Vertex1 = (position1, new Vector2(
            (Vector3.Dot(position1, axisU) + offsetX) / textureSize.X,
            (Vector3.Dot(position1, axisV) + offsetY) / textureSize.Y));

        plane.Vertex3 = (position2, new Vector2(
            (Vector3.Dot(position2, axisU) + offsetX) / textureSize.X,
            (Vector3.Dot(position2, axisV) + offsetY) / textureSize.Y));

        plane.Vertex2 = (position3, new Vector2(
            (Vector3.Dot(position3, axisU) + offsetX) / textureSize.X,
            (Vector3.Dot(position3, axisV) + offsetY) / textureSize.Y));

        plane.Normal = Vector3.Normalize(Vector3.Cross(position3 - position1, position2 - position1));
        plane.Distance = Vector3.Dot(plane.Normal, position1);

        return plane;
    }
}
