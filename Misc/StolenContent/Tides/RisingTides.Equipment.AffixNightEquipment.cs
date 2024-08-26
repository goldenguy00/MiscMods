// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Equipment.AffixNightEquipment
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

public class AffixNightEquipment : BaseEliteAffix
{
	public static ConfigurableValue<float> duration = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Nocturnal", "On Use Invisibility Duration", 10f, 0f, 1000f, "How long should this elite aspect's on-use invisibility last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		base.OnLoad();
		((BaseEquipment)this).equipmentDef.name = "RisingTides_AffixNight";
		((BaseEquipment)this).equipmentDef.cooldown = 40f;
		((BaseEquipment)this).equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Night/texAffixNightEquipmentIcon.png");
		base.SetUpPickupModel();
		base.AdjustElitePickupMaterial(new Color32(100, 100, 100, byte.MaxValue), 1f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Night/texAffixNightRecolorRamp.png"));
		((BaseItemLike)this).itemDisplayPrefab = ((BaseItemLike)this).PrepareItemDisplayModel(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Night/AffixNightHeadpiece.prefab").InstantiateClone("RisingTidesAffixNightHeadpiece", registerNetwork: false));
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
			equipmentSlot.characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, ConfigurableValue<float>.op_Implicit(AffixNightEquipment.duration));
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ProcStealthkit"), new EffectData
			{
				origin = equipmentSlot.characterBody.corePosition,
				rotation = Quaternion.identity
			}, transmit: true);
			return true;
		}
		return false;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		((BaseEquipment)this).equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixNight;
	}
}
