// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.TrenchBroom.Attributes;

/// <summary>Makes this property a model scale handle.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModelScaleAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="ModelScaleAttribute"/> class.</summary>
    /// <param name="model">The linked model property.</param>
    public ModelScaleAttribute(string model)
    {
        Model = model;
    }

    /// <summary>Gets the linked model property.</summary>
    public string Model { get; }
}
