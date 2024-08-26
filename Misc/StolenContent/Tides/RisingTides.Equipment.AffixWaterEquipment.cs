// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Equipment.AffixWaterEquipment
using System;
using System.Collections.Generic;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RisingTides.Equipment;
using RoR2;
using UnityEngine;

public class AffixWaterEquipment : BaseEliteAffix
{
	public static ConfigurableValue<float> healAmount = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Aquamarine", "On Use Heal", 10f, 0f, 1000f, "How much health should this elite aspect's on-use heal regenerate? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		base.OnLoad();
		((BaseEquipment)this).equipmentDef.name = "RisingTides_AffixWater";
		((BaseEquipment)this).equipmentDef.cooldown = 30f;
		((BaseEquipment)this).equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Water/texAffixWaterEquipmentIcon.png");
		base.SetUpPickupModel();
		base.AdjustElitePickupMaterial(new Color32(2, 149, byte.MaxValue, byte.MaxValue), 7f, smoothFresnelRamp: true);
		((BaseItemLike)this).itemDisplayPrefab = ((BaseItemLike)this).PrepareItemDisplayModel(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/AffixWaterHeadParticles.prefab").InstantiateClone("RisingTidesAffixWaterHeadpiece", registerNetwork: false));
		BaseItemLike.onSetupIDRS += delegate
		{
			foreach (CharacterBody allBodyPrefabBodyBodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
			{
				CharacterModel componentInChildren = allBodyPrefabBodyBodyComponent.GetComponentInChildren<CharacterModel>();
				if ((bool)componentInChildren && componentInChildren.itemDisplayRuleSet != null)
				{
					DisplayRuleGroup equipmentDisplayRuleGroup = componentInChildren.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(RoR2Content.Equipment.AffixWhite.equipmentIndex);
					if (!equipmentDisplayRuleGroup.Equals(DisplayRuleGroup.empty))
					{
						string bodyName = BodyCatalog.GetBodyName(allBodyPrefabBodyBodyComponent.bodyIndex);
						ItemDisplayRule[] rules = equipmentDisplayRuleGroup.rules;
						for (int i = 0; i < rules.Length; i++)
						{
							ItemDisplayRule itemDisplayRule = rules[i];
							((BaseItemLike)this).AddDisplayRule(bodyName, itemDisplayRule.childName, itemDisplayRule.localPos, itemDisplayRule.localAngles, itemDisplayRule.localScale);
						}
					}
				}
			}
		};
	}

	public override bool OnUse(EquipmentSlot equipmentSlot)
	{
		if ((bool)equipmentSlot.characterBody)
		{
			EffectData effectData = new EffectData
			{
				origin = equipmentSlot.characterBody.corePosition
			};
			effectData.SetHurtBoxReference(equipmentSlot.characterBody.mainHurtBox);
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), effectData, transmit: true);
			Util.CleanseBody(equipmentSlot.characterBody, removeDebuffs: true, removeBuffs: false, removeCooldownBuffs: true, removeDots: true, removeStun: true, removeNearbyProjectiles: false);
			if ((bool)equipmentSlot.characterBody.healthComponent)
			{
				equipmentSlot.characterBody.healthComponent.HealFraction(ConfigurableValue<float>.op_Implicit(AffixWaterEquipment.healAmount) / 100f, default(ProcChainMask));
			}
			return true;
		}
		return false;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		((BaseEquipment)this).equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixWater;
	}
}
