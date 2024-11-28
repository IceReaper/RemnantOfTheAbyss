// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Numerics;
using Microsoft.Xna.Framework;
using RemnantOfTheAbyss.Graphics;

namespace RemnantOfTheAbyss.Nodes;

/// <summary>Represents a node in the scene graph.</summary>
public class Node : IDisposable
{
    private readonly List<Node> _children = [];

    private Matrix4x4 _localTransform = Matrix4x4.Identity;
    private Matrix4x4 _globalTransform = Matrix4x4.Identity;

    /// <summary>Gets or sets the parent node.</summary>
    public Node? Parent { get; set; }

    /// <summary>Gets the world node.</summary>
    public World? World
    {
        get
        {
            for (var current = this; current != null; current = current.Parent)
            {
                if (current is World world)
                    return world;
            }

            return null;
        }
    }

    /// <summary>Gets the children of the node.</summary>
    public IReadOnlyCollection<Node> Children => _children;

    /// <summary>Gets or sets the transformation matrix of the node.</summary>
    public Matrix4x4 LocalTransform
    {
        get => _localTransform;
        set
        {
            _localTransform = value;
            UpdateGlobalTransform();
        }
    }

    /// <summary>Gets or sets the transformation matrix of the node.</summary>
    public Matrix4x4 GlobalTransform
    {
        get => _globalTransform;
        set
        {
            _globalTransform = value;
            UpdateLocalTransform();
        }
    }

    /// <summary>Makes a node a child of this node.</summary>
    /// <param name="child">The child node.</param>
    /// <param name="realign">If true keep the local transform and recalculate the global transform against the new parent node, else keep the global transform and recalculate the local transform..</param>
    public void Add(Node child, bool realign = false)
    {
        child.Parent?.Remove(child);
        _children.Add(child);
        child.Parent = this;

        if (realign)
            child.UpdateGlobalTransform();
        else
            child.UpdateLocalTransform();
    }

    /// <summary>Removes a child node from this node.</summary>
    /// <param name="child">The child node.</param>
    /// <param name="realign">If true keep the local transform and recalculate the global transform against the new parent node, else keep the global transform and recalculate the local transform..</param>
    public void Remove(Node child, bool realign = false)
    {
        _ = _children.Remove(child);
        child.Parent = null;

        if (realign)
            child.UpdateGlobalTransform();
        else
            child.UpdateLocalTransform();
    }

    /// <summary>Updates the node.</summary>
    /// <param name="gameTime">Snapshot of the game's timing state.</param>
    public virtual void Update(GameTime gameTime)
    {
        foreach (var child in _children)
            child.Update(gameTime);
    }

    /// <summary>Draws the node.</summary>
    /// <param name="renderer">The renderer.</param>
    public virtual void Draw(DeferredRenderer renderer)
    {
        foreach (var child in _children)
        {
            child.Draw(renderer);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes the node.</summary>
    /// <param name="disposing">Whether the node is disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        while (_children.Count > 0)
            _children[0].Dispose();

        Parent?.Remove(this);
    }

    private void UpdateGlobalTransform()
    {
        _globalTransform = Parent != null ? Parent.GlobalTransform * _localTransform : _localTransform;

        foreach (var child in _children)
            child.UpdateGlobalTransform();
    }

    private void UpdateLocalTransform()
    {
        if (Parent != null)
        {
            _ = Matrix4x4.Invert(Parent.GlobalTransform, out var inverseParentGlobalTransform);
            _localTransform = inverseParentGlobalTransform * _globalTransform;
        }
        else
        {
            _localTransform = _globalTransform;
        }

        foreach (var child in _children)
            child.UpdateGlobalTransform();
    }
}
