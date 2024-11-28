// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Data;
using System.Globalization;
using Eiveo.Trenchbroom.SourceGenerator.Informations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Eiveo.Trenchbroom.SourceGenerator;

/// <summary>Utility class to resolve all relevant data.</summary>
public class GameFetcher
{
    /// <summary>Initializes a new instance of the <see cref="GameFetcher"/> class.</summary>
    /// <param name="compilation">The compilation to get the data from.</param>
    public GameFetcher(Compilation compilation)
    {
        var entities = new List<EntityInfo>();
        GameInfo? gameInfo = null;

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is not ITypeSymbol classSymbol)
                    continue;

                var entityAttribute = classSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "EntityAttribute");
                var entitiesLoaderAttribute = classSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "EntitiesLoaderAttribute");

                var header = root.GetLeadingTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)).Select(trivia => trivia.ToString());
                var documentation = classSymbol.GetDocumentationCommentXml();

                if (documentation != null && documentation.StartsWith("<member", StringComparison.Ordinal))
                    documentation = documentation.Substring(0, documentation.LastIndexOf("</member>", StringComparison.Ordinal)).Substring(documentation.IndexOf("<summary>", StringComparison.Ordinal));

                if (entityAttribute != null)
                    entities.Add(FetchEntity(classSymbol, entityAttribute, header, documentation, semanticModel));
                else if (entitiesLoaderAttribute != null)
                    gameInfo = FetchGame(classSymbol, entitiesLoaderAttribute, header, documentation);
            }
        }

        Entities = entities;
        Game = gameInfo ?? throw new ConstraintException("No class with the EntitiesLoaderAttribute found.");
    }

    /// <summary>Gets all entities from the compilation.</summary>
    public IReadOnlyCollection<EntityInfo> Entities { get; }

    /// <summary>Gets the game from the compilation.</summary>
    public GameInfo Game { get; }

    private static EntityInfo FetchEntity(ITypeSymbol classSymbol, AttributeData entityAttribute, IEnumerable<string> header, string? documentation, SemanticModel semanticModel)
    {
        return new EntityInfo
        {
            Header = header.Select(line => line.Trim()).ToList(),
            NameSpace = classSymbol.ContainingNamespace.ToString(),
            Documentation = documentation?.Trim().Split('\n').Select(e => e.Trim()).ToList() ?? [],
            ClassName = classSymbol.Name,
            Id = entityAttribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty,
            IsBrush = (bool)(entityAttribute.ConstructorArguments[1].Value ?? false),
            Description = entityAttribute.ConstructorArguments[2].Value?.ToString() ?? string.Empty,
            Properties = classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(property => property.GetAttributes().Any(attr => attr.AttributeClass?.Name == "PropertyAttribute"))
                .Select(property => ResolveProperty(property, semanticModel))
                .ToList(),
        };
    }

    private static PropertyInfo ResolveProperty(IPropertySymbol property, SemanticModel semanticModel)
    {
        if (property.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "ModelAttribute") != null)
            return ResolveModelProperty(property);

        var modelScaleProperty = property.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "ModelScaleAttribute");

        if (modelScaleProperty != null)
            return ResolveModelScaleProperty(property, modelScaleProperty);

        if (property.Type.TypeKind == TypeKind.Enum)
        {
            var flags = property.Type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "FlagsAttribute");

            return flags ? ResolveFlagsProperty(property, semanticModel) : ResolveEnumProperty(property, semanticModel);
        }

        return new PropertyInfo
        {
            Name = property.Name,
            Type = property.Type.ToDisplayString(),
            Description = property.GetAttributes().First(attr => attr.AttributeClass?.Name == "PropertyAttribute").ConstructorArguments[0].Value?.ToString() ?? string.Empty,
            DefaultValue = GetDefaultValue(property),
        };
    }

    private static ModelPropertyInfo ResolveModelProperty(IPropertySymbol property)
    {
        return new ModelPropertyInfo
        {
            Name = property.Name,
            Type = property.Type.ToDisplayString(),
            Description = property.GetAttributes().First(attr => attr.AttributeClass?.Name == "PropertyAttribute").ConstructorArguments[0].Value?.ToString() ?? string.Empty,
            DefaultValue = GetDefaultValue(property),
        };
    }

    private static ModelScalePropertyInfo ResolveModelScaleProperty(IPropertySymbol property, AttributeData modelScaleProperty)
    {
        return new ModelScalePropertyInfo
        {
            Name = property.Name,
            Type = property.Type.ToDisplayString(),
            Description = property.GetAttributes().First(attr => attr.AttributeClass?.Name == "PropertyAttribute").ConstructorArguments[0].Value?.ToString() ?? string.Empty,
            DefaultValue = GetDefaultValue(property),
            Model = modelScaleProperty.ConstructorArguments[0].Value?.ToString() ?? string.Empty,
        };
    }

    private static FlagsPropertyInfo ResolveFlagsProperty(IPropertySymbol property, SemanticModel semanticModel)
    {
        var entries = new Dictionary<string, ulong>();

        foreach (var fieldSymbol in property.Type.GetMembers().OfType<IFieldSymbol>().Where(member => member.Kind == SymbolKind.Field))
        {
            if (!fieldSymbol.HasConstantValue) continue;

            var key = fieldSymbol.Name;

            var value = fieldSymbol.ConstantValue switch
            {
                byte b => b,
                sbyte sb => (ulong)sb,
                short s => (ulong)s,
                ushort us => us,
                int i => (ulong)i,
                uint ui => ui,
                long l => (ulong)l,
                ulong ul => ul,
                _ => throw new InvalidOperationException("Unsupported enum underlying type."),
            };

            if ((value & (value - 1)) == 0)
                entries.Add(key, value);
        }

        return new FlagsPropertyInfo
        {
            Name = property.Name,
            Type = property.Type.ToDisplayString(),
            Values = entries,
            Description = property.GetAttributes().First(attr => attr.AttributeClass?.Name == "PropertyAttribute").ConstructorArguments[0].Value?.ToString() ?? string.Empty,
            Defaults = GetDefaultFlags(property, semanticModel),
        };
    }

    private static EnumPropertyInfo ResolveEnumProperty(IPropertySymbol property, SemanticModel semanticModel)
    {
        var entries = new Dictionary<string, ulong>();

        foreach (var fieldSymbol in property.Type.GetMembers().OfType<IFieldSymbol>().Where(member => member.Kind == SymbolKind.Field))
        {
            if (!fieldSymbol.HasConstantValue) continue;

            var key = fieldSymbol.Name;

            var value = fieldSymbol.ConstantValue switch
            {
                byte b => b,
                sbyte sb => (ulong)sb,
                short s => (ulong)s,
                ushort us => us,
                int i => (ulong)i,
                uint ui => ui,
                long l => (ulong)l,
                ulong ul => ul,
                _ => throw new InvalidOperationException("Unsupported enum underlying type."),
            };

            entries.Add(key, value);
        }

        return new EnumPropertyInfo
        {
            Name = property.Name,
            Type = property.Type.ToDisplayString(),
            Values = entries,
            Description = property.GetAttributes().First(attr => attr.AttributeClass?.Name == "PropertyAttribute").ConstructorArguments[0].Value?.ToString() ?? string.Empty,
            DefaultValue = GetDefaultEnum(property, semanticModel),
        };
    }

    private static string? GetDefaultValue(IPropertySymbol property)
    {
        var syntaxReference = property.DeclaringSyntaxReferences.FirstOrDefault(e => e != null);

        return syntaxReference?.GetSyntax() is not PropertyDeclarationSyntax { Initializer: { } initializer }
            ? null
            : TryResolveDefaultValueBySpecialType(property, initializer, out var defaultValue)
                ? defaultValue
                : TryResolveDefaultValueByTypeName(property, initializer, out defaultValue)
                    ? defaultValue
                    : throw new NotSupportedException("Unsupported property default value type.");
    }

    private static bool TryResolveDefaultValueBySpecialType(IPropertySymbol property, EqualsValueClauseSyntax initializer, out string? result)
    {
        if (property.Type.SpecialType == SpecialType.System_String)
        {
            result = initializer.Value.ToString() == "string.Empty" ? "\"\"" : initializer.Value.ToString();
        }
        else if (property.Type.SpecialType is SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64)
        {
            result = initializer.Value.ToString();
        }
        else if (property.Type.SpecialType is SpecialType.System_Single or SpecialType.System_Double)
        {
            result = $"\"{initializer.Value}\"";
        }
        else if (property.Type.SpecialType == SpecialType.System_Boolean)
        {
            result = initializer.Value is LiteralExpressionSyntax literal ? literal.Token.Value?.ToString() == "True" ? "1" : "0" : throw new NotSupportedException("Unsupported bool initialization.");
        }
        else
        {
            result = null;
            return false;
        }

        return true;
    }

    private static bool TryResolveDefaultValueByTypeName(IPropertySymbol property, EqualsValueClauseSyntax initializer, out string? result)
    {
        if (property.Type.Name == "Half")
        {
            result = initializer.Value is CastExpressionSyntax cast ? cast.Expression.ToString() : throw new NotSupportedException("Unsupported Half initialization.");
        }
        else if (property.Type.Name == "Color")
        {
            result = initializer.Value is InvocationExpressionSyntax invocation ? $"\"{string.Join(" ", invocation.ArgumentList.Arguments)}\"" : throw new NotSupportedException("Unsupported Color initialization.");
        }
        else if (property.Type.Name == "Vector3")
        {
            result = initializer.Value is ImplicitObjectCreationExpressionSyntax implicitObjectCreation ? $"\"{string.Join(" ", implicitObjectCreation.ArgumentList.Arguments)}\"" : throw new NotSupportedException("Unsupported Vector3 initialization.");
        }
        else
        {
            result = null;
            return false;
        }

        return true;
    }

    private static string? GetDefaultEnum(IPropertySymbol property, SemanticModel semanticModel)
    {
        var propertySyntax = property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;
        if (propertySyntax?.Initializer?.Value == null)
            return null;

        var constantValue = semanticModel.GetConstantValue(propertySyntax.Initializer.Value);
        if (!constantValue.HasValue)
            return null;

        var enumValue = Convert.ToUInt64(constantValue.Value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        return enumValue;
    }

    private static IReadOnlyCollection<ulong> GetDefaultFlags(IPropertySymbol property, SemanticModel semanticModel)
    {
        var propertySyntax = property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;
        if (propertySyntax?.Initializer?.Value == null)
            return [];

        var constantValue = semanticModel.GetConstantValue(propertySyntax.Initializer.Value);
        if (!constantValue.HasValue)
            return [];

        if (property.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
            return [];

        var enumValue = Convert.ToUInt64(constantValue.Value, CultureInfo.InvariantCulture);

        return
        [
            ..enumType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.HasConstantValue)
                .Select(f => Convert.ToUInt64(f.ConstantValue, CultureInfo.InvariantCulture))
                .Where(flag => (enumValue & flag) == flag)
        ];
    }

    private static GameInfo FetchGame(ITypeSymbol classSymbol, AttributeData entitiesLoaderAttribute, IEnumerable<string> header, string? documentation)
    {
        var game = entitiesLoaderAttribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;

        var methodSymbol = classSymbol.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(method => method.Name == "Load") ?? throw new ConstraintException("No Load method found.");

        var methodDocumentation = methodSymbol.GetDocumentationCommentXml();

        if (methodDocumentation != null && methodDocumentation.StartsWith("<member", StringComparison.Ordinal))
            methodDocumentation = methodDocumentation.Substring(0, methodDocumentation.LastIndexOf("</member>", StringComparison.Ordinal)).Substring(methodDocumentation.IndexOf("<summary>", StringComparison.Ordinal));

        return new GameInfo
        {
            Name = game,
            Header = header.Select(line => line.Trim()).ToList(),
            Usings = ["Eiveo.TrenchBroom.Maps", methodSymbol.ReturnType.ContainingNamespace.ToString()],
            NameSpace = classSymbol.ContainingNamespace.ToString(),
            ClassDocumentation = documentation?.Trim().Split('\n').Select(e => e.Trim()).ToList() ?? [],
            ClassName = classSymbol.Name,
            MethodDocumentation = methodDocumentation?.Trim().Split('\n').Select(e => e.Trim()).ToList() ?? [],
            ReturnType = methodSymbol.ReturnType.Name,
        };
    }
}
