using EntityStates.Wisp1Monster;
using MiscMods.Config;
using RoR2;
using UnityEngine.AddressableAssets;

namespace MiscMods.StolenContent.Wisp
{
    internal class WispChanges
    {
        internal SpawnCard GreaterWispCard => Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/GreaterWisp/cscGreaterWisp.asset").WaitForCompletion();
        internal EntityStateConfiguration WispEmbers => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Wisp/EntityStates.Wisp1Monster.FireEmbers.asset").WaitForCompletion();
        public static WispChanges Instance { get; private set; }

        public static void Init() => Instance ??= new WispChanges();

        private WispChanges()
        {
            if (PluginConfig.EnemiesPlusConfig.wispChanges.Value)
            {
                WispEmbers.TryModifyFieldValue(nameof(FireEmbers.damageCoefficient), 0.75f);
                WispEmbers.TryModifyFieldValue(nameof(FireEmbers.bulletCount), 6);
            }

            if (PluginConfig.EnemiesPlusConfig.greaterWispChanges.Value)
                GreaterWispCard.directorCreditCost = 120;
        }
    }
}
