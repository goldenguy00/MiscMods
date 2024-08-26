using System;
using System.Linq;
using HG;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

public class RagingElite : ScriptableObject
{
	public class RageAffixBuffBehaviourServer : CharacterBody.ItemBehavior, IOnKilledServerReceiver, IOnTakeDamageServerReceiver
	{
		public float currentRadius;

		private GameObject affixRageWard;

		public void Start()
        {
            if (base.body.healthComponent && !base.body.healthComponent.onIncomingDamageReceivers.Contains(ref this))
                ArrayUtils.ArrayAppend(ref base.body.healthComponent.onIncomingDamageReceivers, this);

            Util.PlaySound("Play_AragoniteElite_Spawn", base.gameObject);
        }

		private void OnEnable()
		{
			CharacterBody component = base.GetComponent<CharacterBody>();
			if (NetworkServer.active)
			{
				this.affixRageWard = UnityEngine.Object.Instantiate(RagingElite.scriptableObject.MiscGameObjects[0]);
				this.affixRageWard.GetComponent<TeamFilter>().teamIndex = base.GetComponent<TeamComponent>().teamIndex;
				this.affixRageWard.GetComponent<BuffWard>().Networkradius = component.radius;
				this.affixRageWard.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
				NetworkServer.Spawn(this.affixRageWard);
			}
		}

		private void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				if (this.currentRadius > 0f)
				{
					this.currentRadius = Mathf.Min(Mathf.Max(this.currentRadius - Time.fixedDeltaTime * 5f, 0f), 75f);
				}
				if ((bool)this.affixRageWard)
				{
					this.affixRageWard.GetComponent<BuffWard>().Networkradius = this.currentRadius;
				}
			}
		}

		private void OnDisable()
		{
			if ((bool)this.affixRageWard)
			{
				UnityEngine.Object.Destroy(this.affixRageWard);
			}
		}

		public void OnKilledServer(DamageReport damageReport)
		{
			if ((bool)this.affixRageWard)
			{
				this.affixRageWard.GetComponent<BuffWard>().Networkradius = 0f;
			}
			if ((bool)base.body)
			{
				base.body.AddBuff(RagingElite.scriptableObject.MiscBuffs[1]);
				var component = UnityEngine.Object.Instantiate(RagingElite.scriptableObject.MiscGameObjects[3], damageReport.attackerBody ? damageReport.attackerBody.corePosition : base.body.corePosition, Quaternion.identity).GetComponent<RagingEliteProjectileSpawner>();
				component.isCrit = base.body.RollCrit();
				component.targetBody = damageReport.attackerBody;
				component.owner = base.gameObject;
				component.ownerBodyDamage = base.body.damage;
			}
		}

		public void OnTakeDamageServer(DamageReport damageReport)
		{
			if (!(damageReport.damageInfo.procCoefficient > 0f) || !damageReport.damageInfo.attacker)
			{
				return;
			}
			CharacterBody component = damageReport.damageInfo.attacker.GetComponent<CharacterBody>();
			CharacterBody component2 = damageReport.victim.GetComponent<CharacterBody>();
			if ((bool)component && (bool)component2 && component2.HasBuff(SpikestripContentBase<RagingElite>.instance.AffixBuff) && !component2.HasBuff(RagingElite.scriptableObject.MiscBuffs[1]))
			{
				Vector3 position = component.footPosition;
				if ((bool)component.characterMotor && component.characterMotor.isGrounded && (bool)component && Physics.Raycast(new Ray(component.footPosition, Vector3.down), out var hitInfo, 20f))
				{
					position = hitInfo.point;
				}
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.crit = component2.RollCrit();
				fireProjectileInfo.owner = component2.gameObject;
				fireProjectileInfo.position = position;
				fireProjectileInfo.projectilePrefab = RagingElite.deathProjectile;
				fireProjectileInfo.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
				fireProjectileInfo.damage = 5f * component2.damage;
				FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
				component2.AddTimedBuff(RagingElite.scriptableObject.MiscBuffs[1], 1.5f);
			}
			if (damageReport.damageDealt >= 0.1f * damageReport.victim.fullCombinedHealth)
			{
				this.currentRadius += 40f;
			}
			else
			{
				this.currentRadius += 10f;
			}
		}

		public void OnDestroy()
		{
		}
	}

	public static SSEliteBaseSO scriptableObject;

	public static GameObject deathProjectile;

	public string EliteName => "Aragonite";

	public Color EliteColor => RagingElite.scriptableObject.eliteColor;

	public Sprite AffixBuffSprite => RagingElite.scriptableObject.buffSprite;

	public Sprite EquipmentIcon => RagingElite.scriptableObject.EquipmentIcon;

	public string EquipmentName => RagingElite.scriptableObject.EquipmentName;

	public string AffixDescriptionMainWord => RagingElite.scriptableObject.AffixDescriptionMainWord;

	public Texture2D EliteRampTexture => RagingElite.scriptableObject.EliteRampTexture;

	public Type AffixBuffBehaviour => typeof(RageAffixBuffBehaviourServer);

	public bool ServerOnlyAffixBuffBehaviour => false;

	public static void Init()
    {
		RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
		PlasmaUtils.RegisterNetworkPrefab(RagingElite.scriptableObject.MiscGameObjects[0]);
		SpikestripContentBase.buffDefContent.Add(RagingElite.scriptableObject.MiscBuffs[0]);
		RagingElite.scriptableObject.MiscBuffs[0].iconSprite = SpikestripContentBase.AddressablesLoad<BuffDef>("aabb5fce0f91514429bfa91cb2f790da").iconSprite;
		SpikestripContentBase.buffDefContent.Add(RagingElite.scriptableObject.MiscBuffs[1]);
		RagingElite.deathProjectile = RagingElite.scriptableObject.MiscGameObjects[1];
		PlasmaUtils.RegisterNetworkPrefab(RagingElite.deathProjectile);
		SpikestripContentBase.projectilePrefabContent.Add(RagingElite.deathProjectile);
		SpikestripContentBase.effectDefContent.Add(new EffectDef(RagingElite.scriptableObject.MiscGameObjects[2]));
		SpikestripVisuals.RegisterOverlayMaterial(RagingElite.scriptableObject.OverlayMaterial, (CharacterModel self) => self.body.HasBuff(RagingElite.scriptableObject.MiscBuffs[0]) || self.body.HasBuff(base.AffixBuff));
		base.Init();
		base.EquipmentDef.dropOnDeathChance = 0.00025f;
	}

	public void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(RagingElite.scriptableObject.affixColor);
		base.EquipmentDisplayModel = RagingElite.scriptableObject.EquipmentDisplayModel;
	}

	public void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		base.RegisterDisplayParent(LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixHaunted"), base.EquipmentDisplayModel);
	}

	private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if (sender.HasBuff(RagingElite.AffixBuff))
		{
			if (sender.HasBuff(RagingElite.scriptableObject.MiscBuffs[0]))
			{
				sender.RemoveBuff(RagingElite.scriptableObject.MiscBuffs[0]);
			}
			args.attackSpeedMultAdd += 0.5f;
			args.moveSpeedMultAdd += 0.5f;
			args.cooldownMultAdd -= 0.5f;
		}
		else if (sender.HasBuff(RagingElite.scriptableObject.MiscBuffs[0]))
		{
			args.attackSpeedMultAdd += 0.5f;
			args.moveSpeedMultAdd += 0.5f;
			args.cooldownMultAdd -= 0.5f;
		}
	}
}
