// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.WaterInvincibility
using System;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using RisingTides;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WaterInvincibility : BaseBuff
{
	public override void OnLoad()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_WaterInvincibility";
		base.buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif").WaitForCompletion();
		base.buffDef.buffColor = new Color32(2, 149, byte.MaxValue, byte.MaxValue);
		GenericGameEvents.BeforeTakeDamage += new DamageAttackerVictimEventHandler(GenericGameEvents_BeforeTakeDamage);
		Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Water/matAffixWaterInvincible.mat"), (Func<CharacterModel, bool>)((CharacterModel model) => (bool)model.body && model.body.HasBuff(base.buffDef)));
	}

	private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!damageInfo.rejected && (bool)victimInfo.body && victimInfo.body.HasBuff(base.buffDef))
		{
			EffectManager.SpawnEffect(HealthComponent.AssetReferences.damageRejectedPrefab, new EffectData
			{
				origin = damageInfo.position
			}, transmit: true);
			damageInfo.rejected = true;
		}
	}
}
