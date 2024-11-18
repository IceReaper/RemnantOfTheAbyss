// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.TrenchBroom.Attributes;

/// <summary>Generates an entity property in the .fgd file.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="PropertyAttribute"/> class.</summary>
    /// <param name="description">The description of the property.</param>
    public PropertyAttribute(string description)
    {
        Description = description;
    }

    /// <summary>Gets the description of the property.</summary>
    public string Description { get; }
}
