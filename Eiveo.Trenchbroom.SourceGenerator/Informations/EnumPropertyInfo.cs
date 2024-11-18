// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.Trenchbroom.SourceGenerator.Informations;

/// <summary>Represents an enum property of an entity.</summary>
public class EnumPropertyInfo : PropertyInfo
{
    /// <summary>Gets or sets the enum values of the property.</summary>
    public IReadOnlyDictionary<string, ulong> Values { get; set; } = new Dictionary<string, ulong>();
}
