using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DingoAddressablesUtils
{
    public static class AddressablesAssetsUtils
    {
        public static async Task<string> ResolveKeyAsync(this IKeyEvaluator reference)
        {
            var locations = await Addressables.LoadResourceLocationsAsync(reference.RuntimeKey, typeof(Object)).Task;
            if (locations != null && locations.Count > 0)
                return locations[0].PrimaryKey;
            return null;
        } 
    }
}