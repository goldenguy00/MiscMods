// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.AffixNight
using System;
using System.Collections.Generic;
using MysticsItems;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using On.RoR2;
using RisingTides;
using RisingTides.Buffs;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class AffixNight : BaseBuff
{
	public static ConfigurableValue<float> debuffDuration = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Nocturnal", "Debuff Duration", 4f, 0f, 1000f, "How long should this elite's on-hit debuff last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_AffixNight";
		base.buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Night/texAffixNightBuffIcon.png");
		On.RoR2.CharacterBody.OnOutOfDangerChanged += CharacterBody_OnOutOfDangerChanged;
		On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
		On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
		GenericGameEvents.OnHitEnemy += new DamageAttackerVictimEventHandler(GenericGameEvents_OnHitEnemy);
		if (RisingTidesPlugin.mysticsItemsCompatibility)
		{
			RoR2.RoR2Application.onLoad = (Action)Delegate.Combine(RoR2.RoR2Application.onLoad, (Action)delegate
			{
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/ArchWisp/OmniExplosionVFXArchWispCannonImpact.prefab").WaitForCompletion(), (RoR2.BuffDef)null, RoR2.DotController.DotIndex.None, 0f, 1f, DamageType.Stun1s);
			});
		}
	}

	private void CharacterBody_OnOutOfDangerChanged(On.RoR2.CharacterBody.orig_OnOutOfDangerChanged orig, RoR2.CharacterBody self)
	{
		orig(self);
		if (!NetworkServer.active || !self.HasBuff(base.buffDef))
		{
			return;
		}
		if (self.outOfDanger && !self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
		{
			self.AddBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
			RoR2.EffectData effectData = new RoR2.EffectData
			{
				origin = self.corePosition
			};
			if ((bool)self.characterDirection && self.characterDirection.moveVector != Vector3.zero)
			{
				effectData.rotation = RoR2.Util.QuaternionSafeLookRotation(self.characterDirection.moveVector);
			}
			else
			{
				effectData.rotation = self.transform.rotation;
			}
			RoR2.EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/SprintActivate"), effectData, transmit: true);
		}
		else if (!self.outOfDanger && self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
		{
			self.RemoveBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
		}
	}

	private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
	{
		orig(self, buffDef);
		if (buffDef == base.buffDef && self.outOfDanger && !self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
		{
			self.AddBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
		}
	}

	private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
	{
		orig(self, buffDef);
		if (buffDef == base.buffDef && self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
		{
			self.RemoveBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
		}
	}

	private void GenericGameEvents_OnHitEnemy(RoR2.DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && (bool)attackerInfo.body && attackerInfo.body.HasBuff(base.buffDef) && (bool)victimInfo.body)
		{
			victimInfo.body.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_NightReducedVision, ConfigurableValue<float>.op_Implicit(AffixNight.debuffDuration) * damageInfo.procCoefficient);
		}
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		base.buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Night;
	}
}
