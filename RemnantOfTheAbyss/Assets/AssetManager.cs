// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Data;
using Microsoft.Xna.Framework.Graphics;

namespace RemnantOfTheAbyss.Assets;

/// <summary>Manages assets and their owners.</summary>
public sealed class AssetManager : IDisposable
{
    private readonly Dictionary<string, (object Asset, HashSet<object> Owners)> _assets = [];
    private readonly Dictionary<object, HashSet<string>> _ownerAssets = [];

    /// <summary>Gets the graphics device.</summary>
    public required GraphicsDevice GraphicsDevice { get; init; }

    /// <summary>Gets the dummy texture.</summary>
    public required Texture2D DummyTexture { get; init; }

    /// <summary>Gets the dummy model.</summary>
    public required Model DummyModel { get; init; }

    /// <summary>Loads a texture from the content folder.</summary>
    /// <param name="name">The name of the texture to load.</param>
    /// <param name="owner">The owner object.</param>
    /// <returns>The loaded texture.</returns>
    public Texture2D LoadTexture(string name, object owner)
    {
        return LoadAsset<Texture2D>(name, owner, path =>
        {
            if (!File.Exists(path))
                return DummyTexture;

            using var stream = File.OpenRead(path);
            return Texture2D.FromStream(GraphicsDevice, stream);
        });
    }

    /// <summary>Loads a model from the content folder.</summary>
    /// <param name="name">The name of the model to load.</param>
    /// <param name="owner">The owner object.</param>
    /// <returns>The loaded model.</returns>
    public Model LoadModel(string name, object owner)
    {
        return LoadAsset<Model>(name, owner, path => !File.Exists(path) ? DummyModel : Model.FromPath(GraphicsDevice, this, path));
    }

    /// <summary>Removes the owner from all its assets and unloads asset without remaining owner.</summary>
    /// <param name="owner">The owner to remove.</param>
    public void Unload(object owner)
    {
        if (!_ownerAssets.Remove(owner, out var paths))
            return;

        foreach (var path in paths)
        {
            var (asset, owners) = _assets[path];

            _ = owners.Remove(owner);

            if (owners.Count != 0)
                continue;

            if (asset is IDisposable disposable)
                disposable.Dispose();

            _ = _assets.Remove(path);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var asset in _assets.Values.Select(entry => entry.Asset).OfType<IDisposable>())
            asset.Dispose();

        _ownerAssets.Clear();
        _assets.Clear();
    }

    private T LoadAsset<T>(string path, object owner, Func<string, T> loader)
        where T : class
    {
        if (!_ownerAssets.TryGetValue(owner, out var ownedAssets))
            _ownerAssets.Add(owner, ownedAssets = []);

        if (!_assets.TryGetValue(path, out var value))
            _assets.Add(path, value = (loader(path), []));

        var (asset, owners) = value;

        _ = owners.Add(owner);
        _ = ownedAssets.Add(path);

        return asset as T ?? throw new DataException("Asset is not of the expected type.");
    }
}
