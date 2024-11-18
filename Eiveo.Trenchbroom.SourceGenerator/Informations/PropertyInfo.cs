// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.Trenchbroom.SourceGenerator.Informations;

/// <summary>Represents a property of an entity.</summary>
public class PropertyInfo
{
    /// <summary>Gets or sets the name of the property.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of the property.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the default value of the property.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Gets or sets the description of the property.</summary>
    public string Description { get; set; } = string.Empty;
}
