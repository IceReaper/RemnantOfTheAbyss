// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Text;
using Eiveo.Trenchbroom.SourceGenerator.Informations;

namespace Eiveo.Trenchbroom.SourceGenerator.FileBuilders;

/// <summary>Builds the entities loader class.</summary>
public class EntitiesLoaderClassBuilder
{
    /// <summary>Initializes a new instance of the <see cref="EntitiesLoaderClassBuilder"/> class.</summary>
    /// <param name="game">The game information.</param>
    /// <param name="entities">The entities information.</param>
    public EntitiesLoaderClassBuilder(GameInfo game, IReadOnlyCollection<EntityInfo> entities)
    {
        var sb = new StringBuilder();

        if (game.Header.Count != 0)
        {
            foreach (var line in game.Header)
                _ = sb.AppendLine(line);

            _ = sb.AppendLine();
        }

        var usings = game.Usings
            .Concat(entities.Select(e => e.NameSpace))
            .Distinct()
            .OrderBy(nameSpace => nameSpace)
            .ToList();

        if (usings.Count != 0)
        {
            foreach (var @using in usings)
                _ = sb.AppendLine($"using {@using};");

            _ = sb.AppendLine();
        }

        _ = sb
            .AppendLine($"namespace {game.NameSpace};")
            .AppendLine();

        if (game.ClassDocumentation.Count != 0)
        {
            foreach (var line in game.ClassDocumentation)
                _ = sb.AppendLine($"/// {line}");
        }

        _ = sb
            .AppendLine($"public partial class {game.ClassName}")
            .AppendLine("{");

        if (game.MethodDocumentation.Count != 0)
        {
            foreach (var line in game.MethodDocumentation)
                _ = sb.AppendLine($"    /// {line}");
        }

        _ = sb
            .AppendLine($"    public partial {game.ReturnType} Load(Entity entity)")
            .AppendLine("    {")
            .AppendLine("        _ = entity.Properties.TryGetValue(\"classname\", out var className);")
            .AppendLine("        return className switch")
            .AppendLine("        {");

        foreach (var entity in entities)
            _ = sb.AppendLine($"            \"{entity.Id}\" => {entity.ClassName}.Load(entity),");

        _ = sb
            .AppendLine("            _ => UnknownEntity(entity)")
            .AppendLine("        };")
            .AppendLine("    }")
            .AppendLine("}");

        Code = sb.ToString();
        Path = $"{game.NameSpace.Replace('.', '/')}/{game.ClassName}.generated.cs";
    }

    /// <summary>Gets the generated code.</summary>
    public string Code { get; }

    /// <summary>Gets the path to the generated file.</summary>
    public string Path { get; }
}
