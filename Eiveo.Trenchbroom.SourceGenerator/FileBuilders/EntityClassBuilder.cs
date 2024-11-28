// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Text;
using Eiveo.Trenchbroom.SourceGenerator.Informations;

namespace Eiveo.Trenchbroom.SourceGenerator.FileBuilders;

/// <summary>Builds the entity loader class.</summary>
public class EntityClassBuilder
{
    /// <summary>Initializes a new instance of the <see cref="EntityClassBuilder"/> class.</summary>
    /// <param name="entity">The entity to build the loader for.</param>
    public EntityClassBuilder(EntityInfo entity)
    {
        var sb = new StringBuilder();

        if (entity.Header.Count != 0)
        {
            foreach (var line in entity.Header)
                _ = sb.AppendLine(line);

            _ = sb.AppendLine();
        }

        var usings = new List<string> { "Eiveo.TrenchBroom.Maps" };

        foreach (var property in entity.Properties)
        {
            if (property.Type is not ("bool" or "string"))
                usings.Add("System.Globalization");

            if (property.Type.Contains('.'))
                usings.Add(property.Type.Substring(0, property.Type.LastIndexOf('.')));
        }

        foreach (var nameSpace in usings.Distinct().OrderBy(e => e))
            _ = sb.AppendLine($"using {nameSpace};");

        _ = sb
            .AppendLine()
            .AppendLine($"namespace {entity.NameSpace};")
            .AppendLine();

        if (entity.Documentation.Count != 0)
        {
            foreach (var line in entity.Documentation)
                _ = sb.AppendLine($"/// {line.Trim()}");
        }

        _ = sb
            .AppendLine($"public partial class {entity.ClassName}")
            .AppendLine("{")
            .AppendLine($"    /// <summary>Constructs a {entity.ClassName} from a {entity.Id}.</summary>")
            .AppendLine($"    /// <param name=\"entity\">The {entity.Id} to load.</param>")
            .AppendLine($"    /// <returns>The constructed {entity.ClassName}.</returns>")
            .AppendLine($"    public static {entity.ClassName} Load(Entity entity)")
            .AppendLine("    {")
            .AppendLine($"        var result = new {entity.ClassName}();")
            .AppendLine();

        foreach (var property in entity.Properties)
        {
            var lowercaseName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);

            _ = sb.AppendLine($"        if (entity.Properties.TryGetValue(\"{lowercaseName}\", out var {lowercaseName}))");

            if (property.Type is "string" or "string?")
            {
                _ = sb.AppendLine($"            result.{property.Name} = {lowercaseName};");
            }
            else if (property.Type is "sbyte" or "byte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong")
            {
                _ = sb.AppendLine($"            result.{property.Name} = ({property.Type})long.Parse({lowercaseName}, CultureInfo.InvariantCulture);");
            }
            else if (property.Type is "float" or "double")
            {
                _ = sb.AppendLine($"            result.{property.Name} = ({property.Type})double.Parse({lowercaseName}, CultureInfo.InvariantCulture);");
            }
            else if (property.Type is "System.Half")
            {
                _ = sb.AppendLine($"            result.{property.Name} = (Half)double.Parse({lowercaseName}, CultureInfo.InvariantCulture);");
            }
            else if (property.Type is "bool")
            {
                _ = sb.AppendLine($"            result.{property.Name} = {lowercaseName} != \"0\";");
            }
            else if (property.Type is "System.Drawing.Color")
            {
                _ = sb
                    .AppendLine("        {")
                    .AppendLine($"            var segments = {lowercaseName}.Split(' ');")
                    .AppendLine()
                    .AppendLine("            if (segments.Length == 3)")
                    .AppendLine($"                result.{property.Name} = Color.FromArgb(byte.Parse(segments[0], CultureInfo.InvariantCulture), byte.Parse(segments[1], CultureInfo.InvariantCulture), byte.Parse(segments[2], CultureInfo.InvariantCulture));")
                    .AppendLine("            else")
                    .AppendLine($"                result.{property.Name} = Color.FromArgb(byte.Parse(segments[0], CultureInfo.InvariantCulture), byte.Parse(segments[1], CultureInfo.InvariantCulture), byte.Parse(segments[2], CultureInfo.InvariantCulture), byte.Parse(segments[3], CultureInfo.InvariantCulture));")
                    .AppendLine("        }");
            }
            else if (property.Type is "System.Numerics.Vector3")
            {
                _ = sb
                    .AppendLine("        {")
                    .AppendLine($"            var segments = {lowercaseName}.Split(' ');")
                    .AppendLine($"            result.{property.Name} = new Vector3(float.Parse(segments[0], CultureInfo.InvariantCulture), float.Parse(segments[1], CultureInfo.InvariantCulture), float.Parse(segments[2], CultureInfo.InvariantCulture));")
                    .AppendLine("        }");
            }
            else if (property is EnumPropertyInfo enumProperty)
            {
                _ = sb.AppendLine($"            result.{property.Name} = ({enumProperty.Type.Split('.').Last()})ulong.Parse({lowercaseName}, CultureInfo.InvariantCulture);");
            }
            else if (property is FlagsPropertyInfo flagsProperty)
            {
                _ = sb.AppendLine($"            result.{property.Name} = ({flagsProperty.Type.Split('.').Last()})ulong.Parse({lowercaseName}, CultureInfo.InvariantCulture);");
            }

            _ = sb.AppendLine();
        }

        _ = sb
            .AppendLine("        return result;")
            .AppendLine("    }")
            .AppendLine("}");

        Code = sb.ToString();
        Path = $"{entity.NameSpace.Replace('.', '/')}/{entity.ClassName}.generated.cs";
    }

    /// <summary>Gets the generated code.</summary>
    public string Code { get; }

    /// <summary>Gets the path to the generated file.</summary>
    public string Path { get; }
}
