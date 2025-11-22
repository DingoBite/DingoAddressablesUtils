using DingoProjectAppStructure.Core.Config;
using UnityEngine;

namespace DingoAddressablesUtils
{
    [CreateAssetMenu(fileName = nameof(AssetsLibConfig), menuName = CREATE_MENU_PREFIX + nameof(AssetsLibConfig))]
    public class AssetsLibScriptableConfig : ScriptableConfig<AssetsLibConfig> {}
}