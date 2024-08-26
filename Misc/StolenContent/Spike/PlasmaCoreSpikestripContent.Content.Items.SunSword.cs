// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// PlasmaCoreSpikestripContent, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlasmaCoreSpikestripContent.Content.Items.SunSword
using GrooveSaladSpikestripContent;
using On.RoR2;
using PlasmaCoreSpikestripContent.Content.Items;
using PlasmaCoreSpikestripContent.Core;
using PlasmaCoreSpikestripContent.Misc;
using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SunSword : SpikestripItemBase<SunSword>
{
	public static SSItemBaseSO scriptableObject;

	public static RoR2.BuffDef buff;

	public static RoR2.BuffDef cooldown;

	public static GameObject effect;

	public static GameObject effect2;

	public static Material fireMaterial;

	public override string ItemName => "Sun Blade";

	public override string ItemPickup => SunSword.scriptableObject.ItemPickup;

	public override string ItemDescription => SunSword.scriptableObject.ItemDescription;

	public override string ItemLore => "Order: Antique Sword\r\nTracking Number: 34******\r\nEstimated Delivery: 01/34/2056\r\nShipping Method: High Priority\r\nShipping Address: 1313 Step st, New London, Earth\r\nShipping Details:\r\n\r\nTo Albert \r\nThis is it. The sword used by the Sun Empress Priscilla. It was designed so, in the hands of one radiant like the sun, even a small scratch results in the total immolation of the one injured. My brother was kind enough to give it to me after her death, but to be honest, this thing is a piece of history. It really belongs in a museum. Please take good care of it, and be careful - it's still sharp.";

	public override string ItemToken => SunSword.scriptableObject.ItemToken;

	public override ItemTier Tier => SunSword.scriptableObject.ItemTier;

	public override Sprite ItemIconSprite => SunSword.scriptableObject.Icon;

	public override ItemTag[] ItemTags => SunSword.scriptableObject.tags;

	public override void Init()
	{
		PlasmaCorePlugin.itemBaseScriptableObjects.TryGetValue(this.ItemName, out SunSword.scriptableObject);
		if ((bool)SunSword.scriptableObject)
		{
			base.ItemPickupModel = SunSword.scriptableObject.pickupObject;
			base.ItemDisplayModel = SunSword.scriptableObject.displayObject;
			PlasmaLog.Log("Found associated ScriptableObject for ItemBase " + this.ItemName);
			On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
			SunSword.effect = SunSword.scriptableObject.extraDisplayObjects[0];
			SunSword.effect.GetComponentInChildren<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("a9204cb0cc395ad449c95cf03920056f").WaitForCompletion();
			SpikestripContentBase.effectDefContent.Add(new RoR2.EffectDef(SunSword.effect));
			SunSword.fireMaterial = Addressables.LoadAssetAsync<GameObject>("a3fe70c460a83a34991cfd64a74006f0").WaitForCompletion().transform.GetChild(0).GetChild(0).GetChild(1)
				.GetChild(0)
				.GetChild(0)
				.GetChild(3)
				.GetComponent<ParticleSystemRenderer>()
				.material;
			SunSword.effect2 = SunSword.scriptableObject.extraDisplayObjects[1];
			SunSword.effect2.GetComponentInChildren<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("a9204cb0cc395ad449c95cf03920056f").WaitForCompletion();
			ParticleSystemRenderer[] componentsInChildren = SunSword.effect2.GetComponentsInChildren<ParticleSystemRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = SunSword.fireMaterial;
			}
			SpikestripContentBase.effectDefContent.Add(new RoR2.EffectDef(SunSword.effect2));
			SunSword.buff = SunSword.scriptableObject.buffs[0];
			SunSword.cooldown = SunSword.scriptableObject.buffs[1];
			SpikestripContentBase.buffDefContent.Add(SunSword.buff);
			SpikestripContentBase.buffDefContent.Add(SunSword.cooldown);
		}
		else
		{
			PlasmaLog.LogWarn("Missing associated ScriptableObject for ItemBase " + this.ItemName);
		}
		base.Init();
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		if ((bool)damageInfo.attacker && damageInfo.procCoefficient > 0f && damageInfo.damage > 0f)
		{
			RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
			if ((bool)component && (bool)component.master && (bool)component.master.inventory)
			{
				RoR2.CharacterBody component2 = victim.GetComponent<RoR2.CharacterBody>();
				if (component.teamComponent.teamIndex != component2.teamComponent.teamIndex)
				{
					if (!component2.HasBuff(SunSword.buff))
					{
						if (!component.HasBuff(SunSword.cooldown) && component.master.inventory.GetItemCount(base.ItemDef) > 0)
						{
							SunSwordOrb sunSwordOrb = new SunSwordOrb();
							sunSwordOrb.origin = damageInfo.attacker.transform.position + Vector3.up * (8f + component.bestFitRadius);
							sunSwordOrb.target = component2.mainHurtBox;
							sunSwordOrb.attackerBody = component;
							OrbManager.instance.AddOrb(sunSwordOrb);
							component2.AddTimedBuff(SunSword.buff, 30f);
							for (int i = 0; i <= 7; i++)
							{
								component.AddTimedBuff(SunSword.cooldown, i);
							}
						}
					}
					else
					{
						InflictDotInfo inflictDotInfo = default(InflictDotInfo);
						inflictDotInfo.victimObject = victim;
						inflictDotInfo.attackerObject = component.gameObject;
						inflictDotInfo.totalDamage = (float)component.inventory.GetItemCount(SpikestripContentBase<SunSword>.instance.ItemDef) * 0.5f * component.damage;
						inflictDotInfo.dotIndex = RoR2.DotController.DotIndex.Burn;
						inflictDotInfo.damageMultiplier = 1f;
						inflictDotInfo.duration = 1f;
						InflictDotInfo dotInfo = inflictDotInfo;
						RoR2.StrengthenBurnUtils.CheckDotForUpgrade(component.inventory, ref dotInfo);
						if (dotInfo.dotIndex == RoR2.DotController.DotIndex.StrongerBurn)
						{
							dotInfo.damageMultiplier /= 2f;
						}
						RoR2.DotController.InflictDot(ref dotInfo);
					}
				}
			}
		}
		orig(self, damageInfo, victim);
	}

	public override void AssignItemModels()
	{
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.25f, 0.6f, -0.4f),
			localAngles = new Vector3(0f, 2f, 30f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.08843f, 0.22026f, -0.35714f),
			localAngles = new Vector3(0f, 340f, 30f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-2.72331f, 3.3146f, -4.02416f),
			localAngles = new Vector3(0f, 0f, 50f),
			localScale = new Vector3(1.8f, 1.8f, 1.8f)
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.28859f, 0.78992f, -0.88803f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.3422f, 0.60399f, -0.50002f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.2355f, 0.5074f, -0.27364f),
			localAngles = new Vector3(13.38629f, 359.9199f, 29.12837f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Base",
			localPos = new Vector3(-0.25f, -0.96433f, -1.06098f),
			localAngles = new Vector3(270f, 0f, 0f),
			localScale = new Vector3(2f, 2f, 2f)
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.25f, 0.5f, -0.5f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(1.8f, 1.8f, 1.8f)
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-2.47781f, 1.9072f, 8.00141f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(2f, 2f, 2f)
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.24543f, 0.56557f, -0.1198f),
			localAngles = new Vector3(10.48684f, 15.11777f, 30.60336f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.18489f, 0.51411f, -0.22578f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Backpack",
			localPos = new Vector3(-5.12407f, 13.03345f, -5.74198f),
			localAngles = new Vector3(0f, 0f, 45f),
			localScale = new Vector3(9f, 9f, 9f)
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Head",
			localPos = new Vector3(-0.79686f, 1.13713f, -1.80047f),
			localAngles = new Vector3(0f, 0f, 45f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
		itemDisplayRuleDict.Add("mdlMEL-T2", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Body",
			localPos = new Vector3(-0.25f, 0.5f, -0.5f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(1.8f, 1.8f, 1.8f)
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Backpack",
			localPos = new Vector3(-0.24341f, 0.42156f, -0.27348f),
			localAngles = new Vector3(0f, 0f, 30f),
			localScale = new Vector3(1.8f, 1.8f, 1.8f)
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule
		{
			ruleType = ItemDisplayRuleType.ParentedPrefab,
			followerPrefab = base.ItemDisplayModel,
			childName = "Chest",
			localPos = new Vector3(-0.23807f, 0.32376f, -0.45572f),
			localAngles = new Vector3(346.8757f, 348.7179f, 41.30199f),
			localScale = new Vector3(1.5f, 1.5f, 1.5f)
		});
	}
}
