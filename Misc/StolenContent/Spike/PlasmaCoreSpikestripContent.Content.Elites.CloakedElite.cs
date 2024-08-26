// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// PlasmaCoreSpikestripContent, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlasmaCoreSpikestripContent.Content.Elites.CloakedElite
using System;
using GrooveSaladSpikestripContent;
using On.RoR2;
using PlasmaCoreSpikestripContent.Content.Elites;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

public class CloakedElite : SpikestripEliteBase<CloakedElite>
{
	public class CloakedAffixBuffBehaviour : RoR2.CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
	{
		public void Start()
		{
			base.body.AddTimedBuff(RoR2.RoR2Content.Buffs.Cloak, 30f);
			base.body.AddBuff(CloakedElite.scriptableObject.MiscBuffs[0]);
			base.body.AddBuff(CloakedElite.scriptableObject.MiscBuffs[0]);
			ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref base.body.healthComponent.onTakeDamageReceivers;
			IOnTakeDamageServerReceiver value = this;
			SpikestripContentBase.AppendArrayIfMissing(ref onTakeDamageReceivers, in value);
		}

		public void FixedUpdate()
		{
			if (NetworkServer.active && base.body.outOfCombatStopwatch >= 5f && !base.body.HasBuff(RoR2.RoR2Content.Buffs.Cloak) && base.body.healthComponent.timeSinceLastHit >= 10f)
			{
				for (int i = 0; i < 2; i++)
				{
					if (base.body.HasBuff(CloakedElite.scriptableObject.MiscBuffs[0]))
					{
						base.body.RemoveBuff(CloakedElite.scriptableObject.MiscBuffs[0]);
					}
				}
				base.body.AddTimedBuff(RoR2.RoR2Content.Buffs.Cloak, 15f);
				base.body.AddBuff(CloakedElite.scriptableObject.MiscBuffs[0]);
				base.body.AddBuff(CloakedElite.scriptableObject.MiscBuffs[0]);
				RoR2.EffectManager.SpawnEffect(CloakedElite.SmokebombEffect, new RoR2.EffectData
				{
					_origin = base.body.transform.position,
					rotation = base.body.transform.rotation,
					scale = base.body.bestFitRadius * 0.4f
				}, transmit: true);
			}
			else if (!base.body.HasBuff(CloakedElite.scriptableObject.MiscBuffs[0]) && base.body.HasBuff(RoR2.RoR2Content.Buffs.Cloak))
			{
				base.body.RemoveOldestTimedBuff(RoR2.RoR2Content.Buffs.Cloak);
			}
		}

		public void OnDestroy()
		{
			ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref base.body.healthComponent.onTakeDamageReceivers;
			IOnTakeDamageServerReceiver value = this;
			SpikestripContentBase.RemoveFromArray(ref onTakeDamageReceivers, in value);
		}

		public void OnTakeDamageServer(RoR2.DamageReport damageReport)
		{
			if (base.body.HasBuff(CloakedElite.scriptableObject.MiscBuffs[0]))
			{
				base.body.RemoveBuff(CloakedElite.scriptableObject.MiscBuffs[0]);
				RoR2.EffectManager.SpawnEffect(CloakedElite.SmokebombEffect, new RoR2.EffectData
				{
					_origin = damageReport.damageInfo.position,
					rotation = damageReport.victimBody.transform.rotation,
					scale = base.body.bestFitRadius * 0.3f
				}, transmit: true);
			}
		}
	}

	public static SSEliteBaseSO scriptableObject;

	public static GameObject CloakedProjectile;

	public static GameObject CloakedTracer;

	public static GameObject SmokebombEffect;

	public override string EliteName => "Veiled";

	public override Color EliteColor => CloakedElite.scriptableObject.eliteColor;

	public override EliteTierDefinition MainEliteTierDefinition => CloakedElite.scriptableObject.tier switch
	{
		SSEliteBaseSO.EliteTierType.TierTwo => SpikestripEliteBase<CloakedElite>.tierTwoEliteDefault, 
		SSEliteBaseSO.EliteTierType.Honor => SpikestripEliteBase<CloakedElite>.honorEliteDefault, 
		_ => SpikestripEliteBase<CloakedElite>.tierOneEliteDefault, 
	};

	public override EliteTierDefinition[] ExtraEliteTierDefitions
	{
		get
		{
			if (CloakedElite.scriptableObject.tier == SSEliteBaseSO.EliteTierType.TierOne)
			{
				return new EliteTierDefinition[1] { SpikestripEliteBase<CloakedElite>.honorEliteDefault };
			}
			return null;
		}
	}

	public override Sprite AffixBuffSprite => CloakedElite.scriptableObject.buffSprite;

	public override Sprite EquipmentIcon => CloakedElite.scriptableObject.EquipmentIcon;

	public override string EquipmentName => CloakedElite.scriptableObject.EquipmentName;

	public override string AffixDescriptionMainWord => CloakedElite.scriptableObject.AffixDescriptionMainWord;

	public override Texture2D EliteRampTexture => CloakedElite.scriptableObject.EliteRampTexture;

	public override Type AffixBuffBehaviour => typeof(CloakedAffixBuffBehaviour);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override void Init()
	{
		PlasmaCorePlugin.eliteBaseScriptableObjects.TryGetValue(this.EliteName, out CloakedElite.scriptableObject);
		CloakedElite.CloakedTracer = SpikestripContentBase.AddressablesLoad<GameObject>("3f67c04a8c335bb4a81a0fe80f4b074e").InstantiateClone("CloakedTracer", registerNetwork: false);
		SpikestripContentBase.effectDefContent.Add(new RoR2.EffectDef(CloakedElite.CloakedTracer));
		CloakedElite.CloakedTracer.GetComponent<LineRenderer>().material = SpikestripContentBase.AddressablesLoad<Material>("cc20cbe750bfc654e871cd6df71758df");
		CloakedElite.CloakedTracer.GetComponentInChildren<ParticleSystemRenderer>().material = SpikestripContentBase.AddressablesLoad<Material>("6c979f58a0f01b14f981c739264f8280");
		CloakedElite.CloakedTracer.GetComponentInChildren<ParticleSystem>().startSize *= 0.5f;
		CloakedElite.CloakedProjectile = CloakedElite.scriptableObject.MiscGameObjects[0];
		CloakedElite.SmokebombEffect = SpikestripContentBase.AddressablesLoad<GameObject>("533b5c10469c1314ab21bea8fac7462e");
		SpikestripContentBase.buffDefContent.Add(CloakedElite.scriptableObject.MiscBuffs[0]);
		base.Init();
		base.EquipmentDef.dropOnDeathChance = 0.00025f;
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		orig(self, damageInfo, victim);
		if (damageInfo.procCoefficient > 0f && (bool)damageInfo.attacker)
		{
			RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
			if ((bool)component && !component.HasBuff(RoR2.RoR2Content.Buffs.Cloak) && component.HasBuff(base.AffixBuff))
			{
				component.AddTimedBuff(RoR2.RoR2Content.Buffs.Cloak, 5f * damageInfo.procCoefficient);
			}
		}
	}

	public override void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(CloakedElite.scriptableObject.affixColor);
		base.EquipmentDisplayModel = CloakedElite.scriptableObject.EquipmentDisplayModel;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		base.RegisterDisplayParent(LegacyResourcesAPI.Load<RoR2.EquipmentDef>("EquipmentDefs/AffixHaunted"), base.EquipmentDisplayModel);
	}
}
