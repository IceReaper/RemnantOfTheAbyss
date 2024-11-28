// Copyright (c) Eiveo GmbH. All rights reserved.

namespace Eiveo.Trenchbroom.SourceGenerator.Informations;

/// <summary>Represents a property of an entity that is a model scale.</summary>
public class ModelScalePropertyInfo : PropertyInfo
{
    /// <summary>Gets or sets the model property.</summary>
    public string Model { get; set; } = string.Empty;
}
