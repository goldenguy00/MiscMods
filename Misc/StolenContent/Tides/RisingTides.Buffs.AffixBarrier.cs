// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.AffixBarrier
using System;
using System.Collections.Generic;
using IL.RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MysticsItems;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using On.RoR2;
using On.RoR2.UI;
using RisingTides;
using RisingTides.Buffs;
using RisingTides.Equipment;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

public class AffixBarrier : BaseBuff
{
	public static ConfigurableValue<float> healthReduction = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Health Reduction", 50f, 0f, 100f, "How much less health should this elite have? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> barrierDamageResistance = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Barrier Damage Resistance", 50f, 0f, 100f, "How much less damage should this elite take when barrier is active? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<bool> barrierKnockbackResistance = ConfigurableValue.CreateBool("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Barrier Knockback Resistance", true, "Should this elite ignore knockback when barrier is active?", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<bool>)null);

	public static ConfigurableValue<float> barrierDecayRate = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Barrier Decay Rate", 0f, 0f, 100f, "How quickly should this elite's barrier decay? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> startingBarrier = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Starting Barrier", 100f, 0f, 100f, "How much barrier should this elite spawn with? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> debuffDuration = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "Debuff Duration", 1f, 0f, 1000f, "How long should the on-hit debuff last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static Sprite barrierBarSprite;

	public override void OnLoad()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_AffixBarrier";
		base.buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierBuffIcon.png");
		Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Barrier/matAffixBarrierOverlay.mat"), (Func<RoR2.CharacterModel, bool>)((RoR2.CharacterModel model) => (bool)model.body && model.body.HasBuff(base.buffDef)));
		On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
		On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
		GenericGameEvents.OnApplyDamageReductionModifiers += new DamageModifierEventHandler(GenericGameEvents_OnApplyDamageReductionModifiers);
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
		IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats1;
		On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
		AffixBarrier.barrierBarSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierBarRecolor.png");
		On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;
		IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		GenericGameEvents.OnHitEnemy += new DamageAttackerVictimEventHandler(GenericGameEvents_OnHitEnemy);
		if (RisingTidesPlugin.mysticsItemsCompatibility)
		{
			RoR2.RoR2Application.onLoad = (Action)Delegate.Combine(RoR2.RoR2Application.onLoad, (Action)delegate
			{
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, AffixBarrierEquipment.selfBuffUseEffect, (RoR2.BuffDef)null, RoR2.DotController.DotIndex.Bleed, 0f, 0f, DamageType.Generic);
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, (GameObject)null, (RoR2.BuffDef)null, RoR2.DotController.DotIndex.Burn, 0f, 0f, DamageType.Generic);
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, (GameObject)null, RoR2.RoR2Content.Buffs.BeetleJuice, RoR2.DotController.DotIndex.None, 0f, 0f, DamageType.Generic);
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, (GameObject)null, (RoR2.BuffDef)null, RoR2.DotController.DotIndex.Poison, 0f, 0f, DamageType.Generic);
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, (GameObject)null, RoR2.RoR2Content.Buffs.Slow80, RoR2.DotController.DotIndex.None, 0f, 0f, DamageType.Generic);
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, (GameObject)null, RoR2.RoR2Content.Buffs.Cripple, RoR2.DotController.DotIndex.None, 0f, 0f, DamageType.Generic);
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, (GameObject)null, RoR2.RoR2Content.Buffs.LunarSecondaryRoot, RoR2.DotController.DotIndex.None, 0f, 0f, DamageType.Generic);
			});
		}
	}

	private void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, RoR2.HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
	{
		if (!ConfigurableValue<bool>.op_Implicit(AffixBarrier.barrierKnockbackResistance) || !self.body.HasBuff(base.buffDef) || !(self.barrier > 0f))
		{
			orig(self, force, alwaysApply, disableAirControlUntilCollision);
		}
	}

	private void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
	{
		if (!ConfigurableValue<bool>.op_Implicit(AffixBarrier.barrierKnockbackResistance) || !self.body.HasBuff(base.buffDef) || !(self.barrier > 0f))
		{
			orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
		}
	}

	private void GenericGameEvents_OnApplyDamageReductionModifiers(RoR2.DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo, ref float damage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)victimInfo.body && victimInfo.body.HasBuff(base.buffDef) && (bool)victimInfo.healthComponent && victimInfo.healthComponent.barrier > 0f)
		{
			damage *= 1f - ConfigurableValue<float>.op_Implicit(AffixBarrier.barrierDamageResistance) / 100f;
		}
	}

	public void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
	{
		orig(self);
		if (!self.HasBuff(base.buffDef))
		{
			return;
		}
		self.barrierDecayRate *= ConfigurableValue<float>.op_Implicit(AffixBarrier.barrierDecayRate) / 100f;
		if (NetworkServer.active && !self.HasBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained))
		{
			float num = self.maxBarrier * (ConfigurableValue<float>.op_Implicit(AffixBarrier.startingBarrier) / 100f);
			if ((bool)self.healthComponent && self.healthComponent.barrier < num)
			{
				self.healthComponent.Networkbarrier = num;
			}
			self.AddBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained);
		}
	}

	private void CharacterBody_RecalculateStats1(ILContext il)
	{
		ILCursor iLCursor = new ILCursor(il);
		if (!iLCursor.TryGotoNext(MoveType.After, (Instruction x) => x.MatchCallOrCallvirt(typeof(RoR2.CharacterBody), "set_maxShield")))
		{
			return;
		}
		iLCursor.Emit(OpCodes.Ldarg, 0);
		iLCursor.EmitDelegate<Action<RoR2.CharacterBody>>(delegate(RoR2.CharacterBody body)
		{
			if (body.HasBuff(base.buffDef))
			{
				body.maxHealth *= 1f - ConfigurableValue<float>.op_Implicit(AffixBarrier.healthReduction) / 100f;
			}
		});
	}

	private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
	{
		orig(self, buffDef);
		if (NetworkServer.active && buffDef == base.buffDef && self.HasBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained))
		{
			self.RemoveBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained);
		}
	}

	private void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
	{
		orig(self);
		if ((bool)self.source && (bool)self.source.body && self.source.body.HasBuff(base.buffDef))
		{
			if (self.source.barrier > 0f)
			{
				ref RoR2.UI.HealthBar.BarInfo trailingOverHealthbarInfo = ref self.barInfoCollection.trailingOverHealthbarInfo;
				trailingOverHealthbarInfo.color = Color.Lerp(trailingOverHealthbarInfo.color, Color.black, 0.45f * self.source.barrier / self.source.fullHealth);
			}
			ref RoR2.UI.HealthBar.BarInfo barrierBarInfo = ref self.barInfoCollection.barrierBarInfo;
			barrierBarInfo.color = Color.white;
			barrierBarInfo.sprite = AffixBarrier.barrierBarSprite;
			barrierBarInfo.sizeDelta += 2f;
		}
	}

	private void HealthComponent_TakeDamage(ILContext il)
	{
		ILCursor iLCursor = new ILCursor(il);
		if (iLCursor.TryGotoNext(MoveType.After, (Instruction x) => x.MatchLdsfld(typeof(RoR2.DLC1Content.Items), "ExplodeOnDeathVoid")) && iLCursor.TryGotoNext(MoveType.After, (Instruction x) => x.MatchCallOrCallvirt(typeof(RoR2.Inventory), "GetItemCount")))
		{
			iLCursor.Emit(OpCodes.Ldarg, 0);
			iLCursor.EmitDelegate<Func<int, RoR2.HealthComponent, int>>((int explodeOnDeathVoidItemCount, RoR2.HealthComponent hc) => (!hc.body || !hc.body.HasBuff(base.buffDef)) ? explodeOnDeathVoidItemCount : 0);
		}
	}

	private void GenericGameEvents_OnHitEnemy(RoR2.DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		if (damageInfo.rejected || !(damageInfo.procCoefficient > 0f) || !attackerInfo.body || !attackerInfo.body.HasBuff(base.buffDef) || !victimInfo.body)
		{
			return;
		}
		uint? maxStacksFromAttacker = null;
		if ((bool)damageInfo?.inflictor)
		{
			ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
			if ((bool)component && component.useDotMaxStacksFromAttacker)
			{
				maxStacksFromAttacker = component.dotMaxStacksFromAttacker;
			}
		}
		float duration = ConfigurableValue<float>.op_Implicit(AffixBarrier.debuffDuration) * damageInfo.procCoefficient;
		switch (RoR2.RoR2Application.rng.RangeInt(0, 7))
		{
		case 0:
		{
			InflictDotInfo inflictDotInfo = default(InflictDotInfo);
			inflictDotInfo.victimObject = victimInfo.gameObject;
			inflictDotInfo.attackerObject = attackerInfo.gameObject;
			inflictDotInfo.dotIndex = RoR2.DotController.DotIndex.Bleed;
			inflictDotInfo.damageMultiplier = 1f;
			inflictDotInfo.duration = duration;
			inflictDotInfo.maxStacksFromAttacker = maxStacksFromAttacker;
			InflictDotInfo inflictDotInfo2 = inflictDotInfo;
			RoR2.DotController.InflictDot(ref inflictDotInfo2);
			break;
		}
		case 1:
		{
			InflictDotInfo inflictDotInfo = default(InflictDotInfo);
			inflictDotInfo.victimObject = victimInfo.gameObject;
			inflictDotInfo.attackerObject = attackerInfo.gameObject;
			inflictDotInfo.totalDamage = damageInfo.damage * 0.5f;
			inflictDotInfo.dotIndex = RoR2.DotController.DotIndex.Burn;
			inflictDotInfo.damageMultiplier = 1f;
			inflictDotInfo.maxStacksFromAttacker = maxStacksFromAttacker;
			InflictDotInfo inflictDotInfo2 = inflictDotInfo;
			if ((bool)attackerInfo.inventory)
			{
				RoR2.StrengthenBurnUtils.CheckDotForUpgrade(attackerInfo.inventory, ref inflictDotInfo2);
			}
			RoR2.DotController.InflictDot(ref inflictDotInfo2);
			break;
		}
		case 2:
			victimInfo.body.AddTimedBuff(RoR2.RoR2Content.Buffs.BeetleJuice, duration);
			break;
		case 3:
		{
			InflictDotInfo inflictDotInfo = default(InflictDotInfo);
			inflictDotInfo.victimObject = victimInfo.gameObject;
			inflictDotInfo.attackerObject = attackerInfo.gameObject;
			inflictDotInfo.dotIndex = RoR2.DotController.DotIndex.Poison;
			inflictDotInfo.damageMultiplier = 1f;
			inflictDotInfo.duration = duration;
			inflictDotInfo.maxStacksFromAttacker = maxStacksFromAttacker;
			InflictDotInfo inflictDotInfo2 = inflictDotInfo;
			RoR2.DotController.InflictDot(ref inflictDotInfo2);
			break;
		}
		case 4:
			victimInfo.body.AddTimedBuff(RoR2.RoR2Content.Buffs.Slow80, duration);
			break;
		case 5:
			victimInfo.body.AddTimedBuff(RoR2.RoR2Content.Buffs.Cripple, duration);
			break;
		case 6:
			victimInfo.body.AddTimedBuff(RoR2.RoR2Content.Buffs.LunarSecondaryRoot, duration);
			break;
		}
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		base.buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Barrier;
	}
}
