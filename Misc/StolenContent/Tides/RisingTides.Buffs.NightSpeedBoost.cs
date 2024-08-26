// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.NightSpeedBoost
using System;
using System.Collections.Generic;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RisingTides.Buffs;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NightSpeedBoost : BaseBuff
{
	public static ConfigurableValue<float> movementSpeed = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Nocturnal", "Movement Speed", 100f, 0f, 1000f, "How much faster should this elite move out of danger? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> attackSpeed = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Nocturnal", "Attack Speed", 30f, 0f, 1000f, "How much faster should this elite attack out of danger? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_NightSpeedBoost";
		base.buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion();
		base.buffDef.buffColor = new Color32(64, 0, 155, byte.MaxValue);
		RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if (sender.HasBuff(base.buffDef))
		{
			args.moveSpeedMultAdd += ConfigurableValue<float>.op_Implicit(NightSpeedBoost.movementSpeed) / 100f;
			args.attackSpeedMultAdd += ConfigurableValue<float>.op_Implicit(NightSpeedBoost.attackSpeed) / 100f;
		}
	}
}
