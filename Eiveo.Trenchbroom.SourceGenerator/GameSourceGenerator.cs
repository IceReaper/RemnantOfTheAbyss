// Copyright (c) Eiveo GmbH. All rights reserved.

#pragma warning disable RS1035

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Eiveo.Trenchbroom.SourceGenerator.FileBuilders;
using Eiveo.Trenchbroom.SourceGenerator.Informations;
using Microsoft.CodeAnalysis;

namespace Eiveo.Trenchbroom.SourceGenerator;

/// <summary>Generates the methods to load an entity from a trenchbroom map file. Also outputs the trenchbroom game setup.</summary>
[Generator]
public class GameSourceGenerator : ISourceGenerator
{
    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        var gameFetcher = new GameFetcher(context.Compilation);

        GenerateLoaderClasses(context, gameFetcher.Entities, gameFetcher.Game);
        GenerateGameSetup(gameFetcher.Game.Name, gameFetcher.Entities);
    }

    private static void GenerateLoaderClasses(GeneratorExecutionContext context, IReadOnlyCollection<EntityInfo> entities, GameInfo game)
    {
        foreach (var entity in entities)
        {
            var entityLoader = new EntityClassBuilder(entity);
            context.AddSource(entityLoader.Path, entityLoader.Code);
        }

        var entitiesLoader = new EntitiesLoaderClassBuilder(game, entities);
        context.AddSource(entitiesLoader.Path, entitiesLoader.Code);
    }

    private static void GenerateGameSetup(string game, IReadOnlyCollection<EntityInfo> entities)
    {
        var gameDirectory = GetGameDirectory(game);

        if (gameDirectory == null)
            return;

        var gameConfiguration = new GameConfigurationBuilder(game);
        File.WriteAllText(Path.Combine(gameDirectory, "GameConfig.cfg"), gameConfiguration.Code);

        var sb = new StringBuilder();

        sb = sb
            .Append(new EntityFgdBuilder("worldspawn", "World entity", true, []).Code)
            .AppendLine()
            .Append(new EntityFgdBuilder("target", "A marker location", false, []).Code);

        sb = entities.Aggregate(sb, (current, entity) => current.AppendLine().Append(new EntityFgdBuilder(entity).Code));

        File.WriteAllText(Path.Combine(gameDirectory, "Entities.fgd"), sb.ToString());
    }

    private static string? GetGameDirectory(string game)
    {
        var trenchBroomDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".TrenchBroom")
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrenchBroom")
                : RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrenchBroom")
                    : throw new PlatformNotSupportedException();

        var gamesDirectory = Path.Combine(trenchBroomDirectory, "games");

        if (!Directory.Exists(gamesDirectory))
            return null;

        var gameDirectory = Path.Combine(gamesDirectory, Regex.Replace(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(game), "[^a-zA-Z0-9]", string.Empty));

        if (!Directory.Exists(gameDirectory))
            _ = Directory.CreateDirectory(gameDirectory);

        return gameDirectory;
    }
}
