using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.GeneralUtils;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace DingoAddressablesUtils
{
    public class AssetsLib : HardLinkAppModelBase
    {
        private AssetsLibConfig _assetsLibConfig;
        
        private readonly Dictionary<string, AsyncOperationHandle> _loaded = new();
        
        public override Task PostInitialize(ExternalDependencies externalDependencies)
        {
            _assetsLibConfig = externalDependencies.Configs().Get<AssetsLibConfig>();
            PreloadAssets();
            return base.PostInitialize(externalDependencies);
        }

        public async Task<T> LoadAsync<T>(string key) where T : Object
        {
            key = key.NormalizePath();
            if (_loaded.TryGetValue(key, out var existingHandle))
            {
                if (existingHandle.IsValid())
                {
                    if (!existingHandle.IsDone)
                        await existingHandle.ToUniTask();

                    if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                        return existingHandle.Result as T;
                }

                _loaded.Remove(key);
            }

            var handle = Addressables.LoadAssetAsync<T>(key);
            _loaded[key] = handle;
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loaded[key] = handle;
                return handle.Result;
            }

            Debug.LogError($"Failed to load asset with key: {key}");
            _loaded.Remove(key);
            return null;
        }

        public void Release(string key)
        {
            key = key.NormalizePath();
            if (_loaded.TryGetValue(key, out var handle))
            {
                if (handle.IsValid())
                    handle.ReleaseHandleOnCompletion();
                _loaded.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var handle in _loaded.Values)
            {
                if (handle.IsValid())
                    handle.ReleaseHandleOnCompletion();
            }
            _loaded.Clear();
        }

        public void WarmupAsset(string key)
        {
            key = key.NormalizePath();
            if (_loaded.ContainsKey(key)) 
                return;
            var handle = Addressables.LoadAssetAsync<Object>(key);
            _loaded[key] = handle;
            handle.Completed += h =>
            {
                if (h.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogWarning($"Failed to warmup asset by key: {key}");
                    _loaded.Remove(key);
                }
            };
        }

        public void WarmupAssets(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                WarmupAsset(key);
            }
        }

        public void WarmupLabel(string label)
        {
            Addressables.LoadResourceLocationsAsync(label).Completed += locHandle =>
            {
                if (locHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogWarning($"Label '{label}' not found");
                    return;
                }

                foreach (var loc in locHandle.Result)
                {
                    var key = loc.PrimaryKey;
                    WarmupAsset(key);
                }

                Addressables.Release(locHandle);
            };
        }

        private void PreloadAssets()
        {
            if (_assetsLibConfig.PreloadAssets != null)
                WarmupAssets(_assetsLibConfig.PreloadAssets);
            
            if (_assetsLibConfig.PreloadLabels != null)
            {
                foreach (var label in _assetsLibConfig.PreloadLabels)
                {
                    WarmupLabel(label);
                }
            }
        }
    }
}
