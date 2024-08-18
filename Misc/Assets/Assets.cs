using System.Reflection;
using UnityEngine;

namespace MiscMods
{
    internal class Assets
    {
        internal static Sprite smallIconDeselectedSprite, smallIconSelectedSprite;

        internal static void Init()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MiscMods.Assets.riskyartifactsbundle");
            if (stream != null)
            {
                var riskyAssets = AssetBundle.LoadFromStream(stream);
                if (riskyAssets != null)
                {
                    smallIconDeselectedSprite = riskyAssets.LoadAsset<Sprite>("texCrueltyDisabled.png");
                    smallIconSelectedSprite = riskyAssets.LoadAsset<Sprite>("texCrueltyEnabled.png");
                    riskyAssets.Unload(false);
                }
            }
        }
    }
}
