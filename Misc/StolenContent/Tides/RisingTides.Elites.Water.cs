// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Elites.Water
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RisingTides.Elites;
using RoR2;
using UnityEngine;

public class Water : BaseElite
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnModelEffectSpawn _003C_003E9__0_2;

		internal void _003COnLoad_003Eb__0_2(CharacterModel model, GameObject effect)
		{
			if ((bool)model.body)
			{
				effect.transform.localScale += Vector3.one * model.body.radius;
			}
			Util.PlaySound("RisingTides_Play_elite_aquamarine_spawn", effect);
		}
	}

	public override void OnLoad()
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.eliteDef.name = "RisingTides_Water";
		base.vanillaTier = 2;
		base.isHonor = false;
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Aquamarine", "Health Boost Coefficient", 18f, 0f, 1000f, "How much health this elite should have? (e.g. 18 means it will have 18x health)", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<float>)delegate(float newValue)
		{
			base.eliteDef.healthBoostCoefficient = newValue;
		});
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Aquamarine", "Damage Boost Coefficient", 6f, 0f, 1000f, "How much damage this elite should have? (e.g. 6 means it will have 6x damage)", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<float>)delegate(float newValue)
		{
			base.eliteDef.damageBoostCoefficient = newValue;
		});
		EliteRamp.AddRamp(base.eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Water/texAffixWaterRecolorRamp.png"));
		base.modelEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/AffixWaterEffect.prefab");
		base.lightColorOverride = new Color32(122, byte.MaxValue, 241, byte.MaxValue);
		base.particleMaterialOverride = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Water/matWaterMist.mat");
		object obj = _003C_003Ec._003C_003E9__0_2;
		if (obj == null)
		{
			OnModelEffectSpawn val = delegate(CharacterModel model, GameObject effect)
			{
				if ((bool)model.body)
				{
					effect.transform.localScale += Vector3.one * model.body.radius;
				}
				Util.PlaySound("RisingTides_Play_elite_aquamarine_spawn", effect);
			};
			_003C_003Ec._003C_003E9__0_2 = val;
			obj = (object)val;
		}
		base.onModelEffectSpawn = (OnModelEffectSpawn)obj;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		ConfigurableValue.CreateBool("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Enabled Elites", "Aquamarine", true, "", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<bool>)delegate(bool newValue)
		{
			base.eliteDef.eliteEquipmentDef = (newValue ? RisingTidesContent.Equipment.RisingTides_AffixWater : null);
		});
	}
}
