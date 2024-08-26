// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// GrooveSaladSpikestripContent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// GrooveSaladSpikestripContent.Content.WarpedElite
using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using GrooveSaladSpikestripContent.Content;
using On.RoR2;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

public class WarpedElite : SpikestripEliteBase<WarpedElite>
{
	public class GravityAffixBuffBehaviourServer : RoR2.CharacterBody.ItemBehavior
	{
		public static float influenceRadius = 12f;

		public static float baseRechargeTime = 1f;

		public static float baseAttemptFireTime = 0.05f;

		private float rechargeTimer;

		private float attemptFireTimer;

		private float currentRechargeTime => GravityAffixBuffBehaviourServer.baseRechargeTime / base.body.attackSpeed;

		public void FixedUpdate()
		{
			this.attemptFireTimer += Time.fixedDeltaTime;
			this.rechargeTimer -= Time.fixedDeltaTime;
			if (this.attemptFireTimer > GravityAffixBuffBehaviourServer.baseAttemptFireTime)
			{
				this.attemptFireTimer -= GravityAffixBuffBehaviourServer.baseAttemptFireTime;
				if (this.rechargeTimer <= 0f && this.DeflectNearbyProjectiles())
				{
					this.rechargeTimer = this.currentRechargeTime;
				}
			}
		}

		private bool DeflectNearbyProjectiles()
		{
			Vector3 corePosition = base.body.corePosition;
			TeamIndex teamIndex = base.body.teamComponent.teamIndex;
			float num = GravityAffixBuffBehaviourServer.influenceRadius * GravityAffixBuffBehaviourServer.influenceRadius;
			List<ProjectileController> instancesList = RoR2.InstanceTracker.GetInstancesList<ProjectileController>();
			for (int i = 0; i < instancesList.Count; i++)
			{
				ProjectileController projectileController = instancesList[i];
				if (projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - corePosition).sqrMagnitude < num && this.ScrewWithProjectile(projectileController))
				{
					RoR2.EffectManager.SpawnEffect(WarpedElite.bubblePopEffect, new RoR2.EffectData
					{
						origin = projectileController.transform.position,
						scale = 0.8f,
						rotation = RoR2.Util.QuaternionSafeLookRotation(projectileController.transform.up)
					}, transmit: true);
					return true;
				}
			}
			return false;
		}

		public bool ScrewWithProjectile(ProjectileController controller)
		{
			ProjectileSimple component = controller.GetComponent<ProjectileSimple>();
			if ((bool)component && component.desiredForwardSpeed > 0f)
			{
				component.desiredForwardSpeed = 0f - component.desiredForwardSpeed;
				component.SetForwardSpeed(component.desiredForwardSpeed);
				return true;
			}
			ProjectileTargetComponent component2 = controller.GetComponent<ProjectileTargetComponent>();
			if ((bool)component2 && (bool)controller.owner && component2.target != controller.owner.transform)
			{
				component2.target = controller.owner.transform;
				return true;
			}
			BoomerangProjectile component3 = controller.GetComponent<BoomerangProjectile>();
			if ((bool)component3 && component3.boomerangState != BoomerangProjectile.BoomerangState.FlyBack)
			{
				component3.NetworkboomerangState = BoomerangProjectile.BoomerangState.FlyBack;
				component3.onFlyBack?.Invoke();
				return true;
			}
			return false;
		}
	}

	public class GravityBuffBehaviour : RoR2.CharacterBody.ItemBehavior
	{
		public static float antiGravCoef = 100f;

		public ICharacterGravityParameterProvider characterGravityParameterProvider;

		public ICharacterFlightParameterProvider characterFlightParameterProvider;

		public GameObject vfxInstance;

		public void Start()
		{
			this.characterGravityParameterProvider = base.GetComponent<ICharacterGravityParameterProvider>();
			this.characterFlightParameterProvider = base.GetComponent<ICharacterFlightParameterProvider>();
			if (this.characterGravityParameterProvider != null)
			{
				RoR2.CharacterGravityParameters gravityParameters = this.characterGravityParameterProvider.gravityParameters;
				gravityParameters.channeledAntiGravityGranterCount++;
				this.characterGravityParameterProvider.gravityParameters = gravityParameters;
			}
			if (this.characterFlightParameterProvider != null)
			{
				RoR2.CharacterFlightParameters flightParameters = this.characterFlightParameterProvider.flightParameters;
				flightParameters.channeledFlightGranterCount++;
				this.characterFlightParameterProvider.flightParameters = flightParameters;
			}
			this.vfxInstance = UnityEngine.Object.Instantiate(WarpedElite.gravityBubbleVFXPrefab, base.body.corePosition, Quaternion.identity);
			Vector3 vector = Vector3.one * base.body.bestFitRadius * 0.7f;
			this.vfxInstance.transform.localScale = vector;
			this.vfxInstance.GetComponent<RoR2.ObjectScaleCurve>().baseScale = vector;
		}

		public void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				this.ServerFixedUpdate();
			}
		}

		public void ServerFixedUpdate()
		{
			RoR2.HealthComponent healthComponent = base.body.healthComponent;
			if ((bool)healthComponent)
			{
				healthComponent.TakeDamageForce(-Physics.gravity * Time.fixedDeltaTime * GravityBuffBehaviour.antiGravCoef, alwaysApply: true);
			}
		}

		public void LateUpdate()
		{
			if ((bool)this.vfxInstance)
			{
				this.vfxInstance.transform.position = base.body.corePosition;
			}
		}

		public void OnDestroy()
		{
			if (this.characterGravityParameterProvider != null)
			{
				RoR2.CharacterGravityParameters gravityParameters = this.characterGravityParameterProvider.gravityParameters;
				gravityParameters.channeledAntiGravityGranterCount--;
				this.characterGravityParameterProvider.gravityParameters = gravityParameters;
			}
			if (this.characterFlightParameterProvider != null)
			{
				RoR2.CharacterFlightParameters flightParameters = this.characterFlightParameterProvider.flightParameters;
				flightParameters.channeledFlightGranterCount--;
				this.characterFlightParameterProvider.flightParameters = flightParameters;
			}
			if ((bool)this.vfxInstance)
			{
				RoR2.EffectManager.SpawnEffect(WarpedElite.bubblePopEffect, new RoR2.EffectData
				{
					origin = base.body.corePosition,
					scale = this.vfxInstance.transform.localScale.magnitude * 0.4f
				}, transmit: false);
				UnityEngine.Object.Destroy(this.vfxInstance);
			}
		}
	}

	public static Color BubblesTint = SpikestripContentBase.ColorRGB(165f, 72f, 191f);

	public static RoR2.BuffDef gravityBuff;

	public static GameObject gravityBubbleVFXPrefab;

	public static GameObject bubblePopEffect;

	public override string EliteName => "Warped";

	public override Color EliteColor => SpikestripContentBase.ColorRGB(203f, 141f, 239f);

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<WarpedElite>.tierOneEliteDefault;

	public override EliteTierDefinition[] ExtraEliteTierDefitions => new EliteTierDefinition[1] { SpikestripEliteBase<WarpedElite>.honorEliteDefault };

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Misplaced Faith";

	public override string AffixDescriptionMainWord => "gravity";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteGravity.png");

	public override Type AffixBuffBehaviour => typeof(GravityAffixBuffBehaviourServer);

	public override bool ServerOnlyAffixBuffBehaviour => true;

	public override void Init()
	{
		base.Init();
		WarpedElite.gravityBuff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		WarpedElite.gravityBuff.name = "GravityBuff";
		WarpedElite.gravityBuff.buffColor = SpikestripContentBase.ColorRGB(219f, 176f, 252f);
		WarpedElite.gravityBuff.canStack = false;
		WarpedElite.gravityBuff.iconSprite = Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffGravity.png");
		WarpedElite.gravityBuff.isDebuff = true;
		SpikestripContentBase.buffDefContent.Add(WarpedElite.gravityBuff);
		SpikestripBuffBehaviours.RegisterBuffBehaviour(WarpedElite.gravityBuff, typeof(GravityBuffBehaviour), serverOnly: false);
		WarpedElite.gravityBubbleVFXPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/BlueprintStation").transform.Find("mdlBazaarBlueprintTable/BubbleMesh").gameObject.InstantiateClone("GravityBubbleVFX", registerNetwork: false);
		RoR2.ObjectScaleCurve objectScaleCurve = WarpedElite.gravityBubbleVFXPrefab.AddComponent<RoR2.ObjectScaleCurve>();
		objectScaleCurve.overallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		objectScaleCurve.timeMax = 0.3f;
		objectScaleCurve.useOverallCurveOnly = true;
		MeshRenderer component = WarpedElite.gravityBubbleVFXPrefab.GetComponent<MeshRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", WarpedElite.BubblesTint);
		component.material = material;
		WarpedElite.bubblePopEffect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Bandit2SmokeBomb").InstantiateClone("GravityBubblePop", registerNetwork: false);
		Renderer[] componentsInChildren = WarpedElite.bubblePopEffect.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].material = material;
		}
		ParticleSystem[] componentsInChildren2 = WarpedElite.bubblePopEffect.GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			ParticleSystem.MainModule main = componentsInChildren2[j].main;
			main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		}
		WarpedElite.bubblePopEffect.GetComponentInChildren<Light>().color = WarpedElite.BubblesTint;
		RoR2.EffectComponent component2 = WarpedElite.bubblePopEffect.GetComponent<RoR2.EffectComponent>();
		component2.soundName = "Play_item_lunar_secondaryReplace_explode";
		component2.applyScale = true;
		SpikestripContentBase.effectDefContent.Add(new RoR2.EffectDef(WarpedElite.bubblePopEffect));
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
	}

	private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
	{
		if (self.HasBuff(WarpedElite.gravityBuff))
		{
			self._isSprinting = false;
		}
		orig(self);
	}

	public override void OnHitEnemyServer(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
		RoR2.CharacterBody characterBody = (victim ? victim.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)component && component.HasBuff(base.AffixBuff) && (bool)characterBody)
		{
			characterBody.AddTimedBuff(WarpedElite.gravityBuff, 4f * damageInfo.procCoefficient);
		}
	}

	public override void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(SpikestripContentBase.ColorRGB(178f, 98f, 200f));
		base.EquipmentDisplayModel = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/BlueprintStation").transform.Find("mdlBazaarBlueprintTable").gameObject.InstantiateClone("AffixWarpedDisplay", registerNetwork: false);
		UnityEngine.Object.DestroyImmediate(base.EquipmentDisplayModel.GetComponent<MeshRenderer>());
		UnityEngine.Object.DestroyImmediate(base.EquipmentDisplayModel.GetComponent<MeshFilter>());
		UnityEngine.Object.DestroyImmediate(base.EquipmentDisplayModel.GetComponent<MeshCollider>());
		UnityEngine.Object.DestroyImmediate(base.EquipmentDisplayModel.GetComponent<RoR2.EntityLocator>());
		UnityEngine.Object.DestroyImmediate(base.EquipmentDisplayModel.transform.Find("Display").gameObject);
		Transform transform = base.EquipmentDisplayModel.transform.Find("BubbleParticles");
		ParticleSystemRenderer component = transform.GetComponent<ParticleSystemRenderer>();
		ParticleSystem.MainModule main = transform.GetComponent<ParticleSystem>().main;
		main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", WarpedElite.BubblesTint);
		material.EnableKeyword("DITHER");
		component.material = material;
		transform.transform.localScale = Vector3.one * 3.2f;
		int num = 0;
		List<Transform> list = new List<Transform>();
		MeshRenderer[] componentsInChildren = base.EquipmentDisplayModel.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (num >= 3)
			{
				UnityEngine.Object.DestroyImmediate(meshRenderer.gameObject);
				continue;
			}
			num++;
			meshRenderer.material = material;
			list.Add(meshRenderer.transform);
		}
		list[0].localScale = Vector3.one * 2.5f;
		list[0].localPosition = new Vector3(0f, 1f, 0f);
		list[1].localScale = Vector3.one * 1.75f;
		list[1].localPosition = new Vector3(0.6f, -0.5f, 0f);
		list[2].localScale = Vector3.one * 1.25f;
		list[2].localPosition = new Vector3(-0.5f, 0.5f);
		base.EquipmentDisplayModel.AddComponent<RoR2.ItemDisplay>().rendererInfos = SpikestripContentBase.GenerateRendererInfos(base.EquipmentDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		base.RegisterDisplayParent(LegacyResourcesAPI.Load<RoR2.EquipmentDef>("EquipmentDefs/AffixPoison"));
	}
}
