// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Equipment.AffixImpPlaneEquipment
using System;
using System.Collections.Generic;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RisingTides.Buffs;
using RisingTides.Equipment;
using RoR2;
using UnityEngine;

public class AffixImpPlaneEquipment : BaseEliteAffix
{
	public static ConfigurableValue<float> duration = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Realgar", "On Use DoT Immunity Duration", 10f, 0f, 1000f, "How long should this elite aspect's on-use damage-over-time immunity last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		base.OnLoad();
		((BaseEquipment)this).equipmentDef.name = "RisingTides_AffixImpPlane";
		((BaseEquipment)this).equipmentDef.cooldown = 20f;
		((BaseEquipment)this).equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/ImpPlane/texAffixImpPlaneEquipmentIcon.png");
		base.SetUpPickupModel();
		base.AdjustElitePickupMaterial(new Color32(50, 50, 50, byte.MaxValue), 0.5f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/RisingTides/Elites/ImpPlane/texRampEliteImpPlane.png"));
		((BaseItemLike)this).itemDisplayPrefab = ((BaseItemLike)this).PrepareItemDisplayModel(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/ImpPlane/AffixImpPlaneHeadpiece.prefab").InstantiateClone("RisingTidesAffixImpPlaneHeadpiece", registerNetwork: false));
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
			effectData.SetNetworkedObjectReference(equipmentSlot.characterBody.gameObject);
			EffectManager.SpawnEffect(AffixImpPlane.scarVFX, effectData, transmit: true);
			equipmentSlot.characterBody.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_ImpPlaneDotImmunity, ConfigurableValue<float>.op_Implicit(AffixImpPlaneEquipment.duration));
			return true;
		}
		return false;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		((BaseEquipment)this).equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixImpPlane;
	}
}
