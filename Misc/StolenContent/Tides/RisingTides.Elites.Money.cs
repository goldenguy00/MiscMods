// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Elites.Money
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using UnityEngine;

public class Money : BaseElite
{
	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.eliteDef.name = "RisingTides_Money";
		base.vanillaTier = 1;
		base.isHonor = true;
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Magnetic", "Health Boost Coefficient", 4f, 0f, 1000f, "How much health this elite should have? (e.g. 18 means it will have 18x health)", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<float>)delegate(float newValue)
		{
			base.eliteDef.healthBoostCoefficient = newValue;
		});
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Magnetic", "Damage Boost Coefficient", 2f, 0f, 1000f, "How much damage this elite should have? (e.g. 6 means it will have 6x damage)", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<float>)delegate(float newValue)
		{
			base.eliteDef.damageBoostCoefficient = newValue;
		});
		EliteRamp.AddRamp(base.eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Money/texMoneyWaterRecolorRamp.png"));
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		ConfigurableValue.CreateBool("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Enabled Elites", "Magnetic", true, "", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<bool>)delegate(bool newValue)
		{
			base.eliteDef.eliteEquipmentDef = (newValue ? RisingTidesContent.Equipment.RisingTides_AffixMoney : null);
		});
	}
}
