// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.ImpPlaneDotImmunity
using System;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using On.RoR2;
using RisingTides;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class ImpPlaneDotImmunity : BaseBuff
{
	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_ImpPlaneDotImmunity";
		base.buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif").WaitForCompletion();
		base.buffDef.buffColor = new Color32(230, 0, 60, byte.MaxValue);
		On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
		On.RoR2.DotController.AddDot += DotController_AddDot;
		Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/ImpPlane/matAffixImpPlaneBuffedOutline.mat"), (Func<RoR2.CharacterModel, bool>)((RoR2.CharacterModel model) => (bool)model.body && model.body.HasBuff(base.buffDef)));
	}

	private void DotController_AddDot(On.RoR2.DotController.orig_AddDot orig, RoR2.DotController self, GameObject attackerObject, float duration, RoR2.DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, RoR2.DotController.DotIndex? preUpgradeDotIndex)
	{
		if (!self.victimBody || !self.victimBody.HasBuff(base.buffDef))
		{
			orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
		}
	}

	private void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, RoR2.CharacterBody self, BuffIndex buffType, int newCount)
	{
		orig(self, buffType, newCount);
		if (NetworkServer.active && buffType == base.buffDef.buffIndex && newCount > 0)
		{
			RoR2.Util.CleanseBody(self, removeDebuffs: false, removeBuffs: false, removeCooldownBuffs: false, removeDots: true, removeStun: false, removeNearbyProjectiles: false);
		}
	}
}
