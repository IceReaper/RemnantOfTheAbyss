// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.Trenchbroom.SourceGenerator.Informations;

/// <summary>Represents an entity.</summary>
public class EntityInfo
{
    /// <summary>Gets or sets the header of the entity.</summary>
    public IReadOnlyCollection<string> Header { get; set; } = [];

    /// <summary>Gets or sets the name space of the entity.</summary>
    public string NameSpace { get; set; } = string.Empty;

    /// <summary>Gets or sets the documentation of the entity.</summary>
    public IReadOnlyCollection<string> Documentation { get; set; } = [];

    /// <summary>Gets or sets the class name of the entity.</summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>Gets or sets the id of the entity.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the entity is a brush.</summary>
    public bool IsBrush { get; set; }

    /// <summary>Gets or sets the description of the entity.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the properties of the entity.</summary>
    public IReadOnlyCollection<PropertyInfo> Properties { get; set; } = [];
}
