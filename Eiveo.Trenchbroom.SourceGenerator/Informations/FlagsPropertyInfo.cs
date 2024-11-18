// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.Trenchbroom.SourceGenerator.Informations;

/// <summary>Represents a flags property of an entity.</summary>
public class FlagsPropertyInfo : PropertyInfo
{
    /// <summary>Gets or sets the enum values of the property.</summary>
    public IReadOnlyDictionary<string, ulong> Values { get; set; } = new Dictionary<string, ulong>();

    /// <summary>Gets or sets the default values of the property.</summary>
    public IReadOnlyCollection<ulong> Defaults { get; set; } = [];
}
