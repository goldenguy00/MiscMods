// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Elites.Barrier
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

public class Barrier : BaseElite
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
			Util.PlaySound("RisingTides_Play_elite_bismuth_spawn", effect);
		}
	}

	public override void OnLoad()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.eliteDef.name = "RisingTides_Barrier";
		base.vanillaTier = 2;
		base.isHonor = false;
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Health Boost Coefficient", 18f, 0f, 1000f, "How much health this elite should have? (e.g. 18 means it will have 18x health)", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<float>)delegate(float newValue)
		{
			base.eliteDef.healthBoostCoefficient = newValue;
		});
		ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Damage Boost Coefficient", 6f, 0f, 1000f, "How much damage this elite should have? (e.g. 6 means it will have 6x damage)", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<float>)delegate(float newValue)
		{
			base.eliteDef.damageBoostCoefficient = newValue;
		});
		EliteRamp.AddRamp(base.eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierRecolorRamp.png"));
		base.modelEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/AffixBarrierEffect.prefab");
		base.lightColorOverride = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		base.particleMaterialOverride = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Barrier/matAffixBarrierRainbowParticle.mat");
		object obj = _003C_003Ec._003C_003E9__0_2;
		if (obj == null)
		{
			OnModelEffectSpawn val = delegate(CharacterModel model, GameObject effect)
			{
				if ((bool)model.body)
				{
					effect.transform.localScale += Vector3.one * model.body.radius;
				}
				Util.PlaySound("RisingTides_Play_elite_bismuth_spawn", effect);
			};
			_003C_003Ec._003C_003E9__0_2 = val;
			obj = (object)val;
		}
		base.onModelEffectSpawn = (OnModelEffectSpawn)obj;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		ConfigurableValue.CreateBool("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Enabled Elites", "Bismuth", true, "", (List<string>)null, (ConfigEntry<bool>)null, false, (Action<bool>)delegate(bool newValue)
		{
			base.eliteDef.eliteEquipmentDef = (newValue ? RisingTidesContent.Equipment.RisingTides_AffixBarrier : null);
		});
	}
}
