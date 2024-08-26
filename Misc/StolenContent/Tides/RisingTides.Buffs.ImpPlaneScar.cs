// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.ImpPlaneScar
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

public class ImpPlaneScar : BaseBuff
{
	public static DotController.DotDef dotDef;

	public static DotController.DotIndex dotIndex;

	public override void OnPluginAwake()
	{
		((BaseLoadableAsset)this).OnPluginAwake();
		ImpPlaneScar.dotDef = new DotController.DotDef
		{
			resetTimerOnAdd = false,
			damageCoefficient = 0.05f,
			damageColorIndex = DamageColorIndex.SuperBleed,
			interval = 0.2f
		};
		ImpPlaneScar.dotIndex = DotAPI.RegisterDotDef(ImpPlaneScar.dotDef);
	}

	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_ImpPlaneScar";
		base.buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/ImpPlane/texAffixImpPlaneScarIcon.png");
		base.buffDef.buffColor = new Color32(188, 15, 52, byte.MaxValue);
		base.buffDef.canStack = false;
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Realgar", "Scar Damage Per Tick", 5f, 0f, 1000f, "How much damage should this elite's on-hit damage-over-time debuff deal? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)delegate(float newValue)
		{
			ImpPlaneScar.dotDef.damageCoefficient = newValue / 100f;
		});
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Realgar", "Scar Damage Interval", 0.2f, 0f, 1000f, "How often should this elite's on-hit damage-over-time debuff tick? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)delegate(float newValue)
		{
			ImpPlaneScar.dotDef.interval = newValue;
		});
		ImpPlaneScar.dotDef.associatedBuff = base.buffDef;
	}
}
