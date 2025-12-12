# DingoAddressablesUtils

A small set of utilities for Unity Addressables: a simple asset loading “library” with cached `AsyncOperationHandle`s, asset warmup (preload) by keys and labels, and a preload config.

## What’s inside

### `AssetsLib`
A service/model for working with Addressables:

- `LoadAsync<T>(string key)` loads an asset by key and caches the handle.
- `WarmupAsset(string key)` / `WarmupAssets(IEnumerable<string> keys)` warms up assets without awaiting the result.
- `WarmupLabel(string label)` warms up all assets found by a label (via `LoadResourceLocationsAsync`).
- `Release(string key)` / `ReleaseAll()` releases cached handles.

> Internally, keys are normalized (`NormalizePath()`), and handle releasing is done via `ReleaseHandleOnCompletion()` (both come from dependencies).

### `AssetsLibConfig` and `AssetsLibScriptableConfig`
Preload configuration:

- `PreloadAssets: List<string>` list of Addressables keys.
- `PreloadLabels: List<string>` list of Addressables labels.

In `AssetsLib.PostInitialize(...)` the config is taken from `externalDependencies.Configs().Get<AssetsLibConfig>()`, then preloading runs for both lists.

### `AddressablesAssetsUtils`
An extension helper to get `PrimaryKey` from a `RuntimeKey` using `LoadResourceLocationsAsync(...)`.

## Dependencies

This repository is intended to be used together with:

- Unity Addressables (`com.unity.addressables`)
- UniTask (`Cysharp.Threading.Tasks`)
- `DingoProjectAppStructure` (base classes / Service Locator / configs)
- `DingoUnityExtensions` (e.g. `NormalizePath`, `ReleaseHandleOnCompletion`)

## Installation

### Option 1. Copy into the project (simplest)
Copy the `.cs` and `.meta` files into any folder under `Assets/`, for example:
`Assets/DingoAddressablesUtils/`

### Option 2. Git submodule
Add this repository as a submodule under `Assets/` and commit the submodule reference.

## Quick start

### 1) Preload config
Create a ScriptableObject `AssetsLibConfig` via the Unity menu (the menu item comes from the base `ScriptableConfig`).

Fill in:
- `PreloadAssets` (Addressables keys)
- `PreloadLabels` (Addressables labels)

### 2) Load an asset by key

```csharp
using UnityEngine;

public class Example : MonoBehaviour
{
    public DingoAddressablesUtils.AssetsLib Assets;

    private async void Start()
    {
        // Example: load a sprite by key
        var icon = await Assets.LoadAsync<Sprite>("UI/Icons/MyIcon");

        // ... use icon ...

        Assets.Release("UI/Icons/MyIcon");
    }
}
```

`AssetsLib` caches handles, so repeated requests for the same key do not recreate the load.

### 3) Warm up by label

```csharp
Assets.WarmupLabel("ui_icons");
```

`WarmupLabel` first gets locations by label, then warms up each location by its `PrimaryKey`.

## Notes on memory and lifecycle

- If you warm up many assets, remember to call `ReleaseAll()` (or `Release(key)`) when changing context to free Addressables handles.
- On load error, `LoadAsync` logs `Debug.LogError` and removes the key from the cache.
