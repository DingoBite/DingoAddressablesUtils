using System;
using System.Collections.Generic;
using DingoProjectAppStructure.Core.Config;
using UnityEngine.Scripting;

namespace DingoAddressablesUtils
{
    [Serializable, Preserve]
    public class AssetsLibConfig : ConfigBase
    {
        public List<string> PreloadAssets;
        public List<string> PreloadLabels;
    }
}