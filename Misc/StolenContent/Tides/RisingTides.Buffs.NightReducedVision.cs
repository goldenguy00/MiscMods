// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.NightReducedVision
using System;
using System.Collections.Generic;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using On.RoR2;
using RisingTides;
using RisingTides.Buffs;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NightReducedVision : BaseBuff
{
	public static ConfigurableValue<float> visionDistance = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Nocturnal", "Debuff Vision Distance", 20f, 0f, 1000f, "How far should you be able to see when debuffed by this elite? (in metres)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_NightReducedVision";
		base.buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffCloakIcon.tif").WaitForCompletion();
		base.buffDef.buffColor = new Color32(64, 0, 155, byte.MaxValue);
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
	}

	private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
	{
		orig(self);
		if (self.HasBuff(base.buffDef))
		{
			self.visionDistance = Mathf.Min(self.visionDistance, ConfigurableValue<float>.op_Implicit(NightReducedVision.visionDistance));
		}
	}
}
