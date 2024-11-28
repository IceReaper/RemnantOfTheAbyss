// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Text;
using Eiveo.Trenchbroom.SourceGenerator.Informations;

namespace Eiveo.Trenchbroom.SourceGenerator.FileBuilders;

/// <summary>Represents an entity which can be written in FGD format.</summary>
public class EntityFgdBuilder
{
    /// <summary>Initializes a new instance of the <see cref="EntityFgdBuilder"/> class.</summary>
    /// <param name="name">The name of the entity.</param>
    /// <param name="description">The description of the entity.</param>
    /// <param name="isBrush">A value indicating whether the entity is a brush.</param>
    /// <param name="properties">The properties of the entity.</param>
    public EntityFgdBuilder(string name, string description, bool isBrush, IReadOnlyCollection<PropertyInfo> properties)
    {
        Code = BuildCode(name, description, isBrush, properties);
    }

    /// <summary>Initializes a new instance of the <see cref="EntityFgdBuilder"/> class.</summary>
    /// <param name="entity">The entity to build the loader for.</param>
    public EntityFgdBuilder(EntityInfo entity)
    {
        Code = BuildCode(entity.Id, entity.Description, entity.IsBrush, entity.Properties);
    }

    /// <summary>Gets the generated code.</summary>
    public string Code { get; }

    private static string BuildCode(string name, string description, bool isBrush, IReadOnlyCollection<PropertyInfo> properties)
    {
        var sb = new StringBuilder();

        _ = sb
            .AppendLine($"{(isBrush ? "@SolidClass" : "@PointClass size(-4 -4 -4, 4 4 4)")} = {name} : \"{description}.\"")
            .AppendLine("[");

        foreach (var property in properties)
        {
            var lowercaseName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);

            if (property is FlagsPropertyInfo flagsProperty)
            {
                _ = sb
                    .AppendLine($"    {lowercaseName}(flags) =")
                    .AppendLine("    [");

                foreach (var value in flagsProperty.Values)
                    _ = sb.AppendLine($"        {value.Value} : \"{value.Key}\" : {(flagsProperty.Defaults.Contains(value.Value) ? 1 : 0)}");

                _ = sb.AppendLine("    ]");
            }
            else if (property is EnumPropertyInfo enumProperty)
            {
                _ = sb
                    .AppendLine($"    {lowercaseName}(choices) : \"{property.Description}\"{(property.DefaultValue != null ? $" : {property.DefaultValue}" : string.Empty)} =")
                    .AppendLine("    [");

                foreach (var value in enumProperty.Values)
                    _ = sb.AppendLine($"        {value.Value} : \"{value.Key}\"");

                _ = sb.AppendLine("    ]");
            }
            else
            {
                _ = sb.AppendLine($"    {lowercaseName}({GetTypeName(property.Type)}) : \"{property.Description}\"{(property.DefaultValue != null ? $" : {property.DefaultValue}" : string.Empty)}");
            }
        }

        _ = sb.AppendLine("]");

        return sb.ToString();
    }

    private static string GetTypeName(string type)
    {
        return type switch
        {
            "string" => "string",
            "string?" => "string",
            "sbyte" or "byte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" => "integer",
            "System.Half" or "float" or "double" => "float",
            "bool" => "boolean",
            "System.Drawing.Color" => "color255",
            "System.Numerics.Vector3" => "vector",
            _ => throw new NotSupportedException("Unsupported property type name"),
        };
    }
}
