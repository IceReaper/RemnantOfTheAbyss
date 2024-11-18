// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Text;

namespace Eiveo.Trenchbroom.SourceGenerator.FileBuilders;

/// <summary>Builds the game configuration.</summary>
public class GameConfigurationBuilder
{
    /// <summary>Initializes a new instance of the <see cref="GameConfigurationBuilder"/> class.</summary>
    /// <param name="game">The name of the game.</param>
    public GameConfigurationBuilder(string game)
    {
        var sb = new StringBuilder();

        sb = sb
            .AppendLine("{")
            .AppendLine("    \"version\": 9,")
            .AppendLine($"    \"name\": \"{game}\",")
            .AppendLine("    \"fileformats\": [ { \"format\": \"Valve\" } ],")
            .AppendLine("    \"filesystem\": { \"searchpath\": \".\", \"packageformat\": { \"extension\": \".zip\", \"format\": \"zip\" } },")
            .AppendLine("    \"materials\": { \"root\": \"Textures\", \"format\": { \"extensions\": [ \".png\" ], \"format\": \"image\" } },")
            .AppendLine("    \"entities\": { \"definitions\": [ \"Entities.fgd\" ], \"defaultcolor\": \"0.5 0.5 0.5 1.0\" }")
            .AppendLine("}");

        Code = sb.ToString();
    }

    /// <summary>Gets the generated code.</summary>
    public string? Code { get; }
}
