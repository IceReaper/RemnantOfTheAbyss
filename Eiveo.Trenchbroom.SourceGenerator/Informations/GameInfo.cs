// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.Trenchbroom.SourceGenerator.Informations;

/// <summary>Represents the game information.</summary>
public class GameInfo
{
    /// <summary>Gets or sets the name of the game.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the header of the entities loader.</summary>
    public IReadOnlyCollection<string> Header { get; set; } = [];

    /// <summary>Gets or sets the required usings of the map parser.</summary>
    public IReadOnlyCollection<string> Usings { get; set; } = [];

    /// <summary>Gets or sets the name space of the entities loader.</summary>
    public string NameSpace { get; set; } = string.Empty;

    /// <summary>Gets or sets the documentation of the entities loader.</summary>
    public IReadOnlyCollection<string> ClassDocumentation { get; set; } = [];

    /// <summary>Gets or sets the class name of the entities loader.</summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>Gets or sets the documentation of the entities loader load method.</summary>
    public IReadOnlyCollection<string> MethodDocumentation { get; set; } = [];

    /// <summary>Gets or sets the return type of the entities loader load method.</summary>
    public string ReturnType { get; set; } = string.Empty;
}
