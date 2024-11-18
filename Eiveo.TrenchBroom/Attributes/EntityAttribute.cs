// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.TrenchBroom.Attributes;

/// <summary>Generates an entity in the .fgd file.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EntityAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="EntityAttribute"/> class.</summary>
    /// <param name="id">The id of the entity in the editor.</param>
    /// <param name="isBrush">A value indicating whether the entity is a brush.</param>
    /// <param name="description">The description of the entity.</param>
    public EntityAttribute(string id, bool isBrush, string description)
    {
        Id = id;
        IsBrush = isBrush;
        Description = description;
    }

    /// <summary>Gets the id of the entity in the editor.</summary>
    public string Id { get; }

    /// <summary>Gets a value indicating whether the entity is a brush.</summary>
    public bool IsBrush { get; }

    /// <summary>Gets the description of the entity.</summary>
    public string Description { get; }
}
