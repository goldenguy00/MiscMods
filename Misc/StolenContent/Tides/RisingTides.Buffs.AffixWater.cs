// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.AffixWater
using System;
using System.Collections.Generic;
using EntityStates;
using MysticsItems;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using On.RoR2;
using RisingTides;
using RisingTides.Buffs;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

public class AffixWater : BaseBuff
{
	public class RisingTidesWaterBubbleTrap : MonoBehaviour
	{
		public RoR2.VehicleSeat vehicleSeat;

		public Rigidbody rigidbody;

		public int breakOutAttempts = 0;

		public float forcedBreakOutDelay;

		public float cooldownBuffDuration;

		public float initialUpwardsSpeed = 1.2f;

		public float age;

		public float fixedAge;

		public Wave shakeWaveX;

		public Wave shakeWaveZ;

		public float shakeTimer = 0f;

		public float shakeDuration = 0.16f;

		public void Awake()
		{
			this.forcedBreakOutDelay = ConfigurableValue<float>.op_Implicit(AffixWater.forcedBreakOutDelay);
			this.cooldownBuffDuration = ConfigurableValue<float>.op_Implicit(AffixWater.cooldownBuffDuration);
			this.vehicleSeat = base.GetComponent<RoR2.VehicleSeat>();
			this.vehicleSeat.onPassengerEnter += VehicleSeat_onPassengerEnter;
			this.vehicleSeat.handleVehicleExitRequestServer.AddCallback(HandleVehicleExitRequest);
			this.vehicleSeat.onPassengerExit += VehicleSeat_onPassengerExit;
			this.rigidbody = base.GetComponent<Rigidbody>();
			this.rigidbody.velocity = Vector3.up * this.initialUpwardsSpeed;
			this.shakeWaveX = new Wave
			{
				amplitude = RoR2.RoR2Application.rng.RangeFloat(0.03f, 0.1f),
				frequency = RoR2.RoR2Application.rng.RangeFloat(10f, 20f)
			};
			this.shakeWaveZ = new Wave
			{
				amplitude = RoR2.RoR2Application.rng.RangeFloat(0.03f, 0.1f),
				frequency = RoR2.RoR2Application.rng.RangeFloat(10f, 20f)
			};
		}

		private void VehicleSeat_onPassengerEnter(GameObject passengerObject)
		{
			if ((bool)passengerObject)
			{
				RoR2.CharacterBody body = passengerObject.GetComponent<RoR2.CharacterBody>();
				if ((bool)body && body.hasEffectiveAuthority && !body.skillLocator)
				{
				}
			}
		}

		private void VehicleSeat_onPassengerExit(GameObject passengerObject)
		{
			if (NetworkServer.active && (bool)passengerObject)
			{
				RoR2.CharacterBody component = passengerObject.GetComponent<RoR2.CharacterBody>();
				if ((bool)component)
				{
					component.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_WaterBubbleTrapCooldown, this.cooldownBuffDuration);
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void Start()
		{
			RoR2.Util.PlaySound("Play_item_use_tonic", base.gameObject);
		}

		public void Update()
		{
			this.age += Time.deltaTime;
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= Time.deltaTime;
				Vector3 position = this.rigidbody.position;
				position += Vector3.left * this.shakeWaveX.Evaluate(this.age);
				position += Vector3.forward * this.shakeWaveZ.Evaluate(this.age);
				this.rigidbody.MovePosition(position);
			}
		}

		public void FixedUpdate()
		{
			this.fixedAge += Time.fixedDeltaTime;
			if (this.fixedAge >= this.forcedBreakOutDelay && NetworkServer.active)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void HandleVehicleExitRequest(GameObject gameObject, ref bool? result)
		{
			this.breakOutAttempts++;
			this.shakeTimer = this.shakeDuration;
			if (this.breakOutAttempts < ConfigurableValue<int>.op_Implicit(AffixWater.breakOutAttemptsRequired))
			{
				result = true;
			}
		}

		public void OnDestroy()
		{
			if (NetworkServer.active)
			{
				RoR2.EffectManager.SpawnEffect(AffixWater.bubbleBurstVFX, new RoR2.EffectData
				{
					origin = base.transform.position,
					scale = base.transform.localScale.x
				}, transmit: true);
			}
		}
	}

	public class RisingTidesAffixWaterBehaviour : MonoBehaviour
	{
		public RoR2.CharacterBody body;

		public RoR2.SkillLocator skillLocator;

		public float vulnerableTimer = 0f;

		public float vulnerableDuration = 0.2f;

		public float failsafeVulnerabilityForceTimer = 0f;

		public float failsafeVulnerabilityForceThreshold = 30f;

		public void Awake()
		{
			this.body = base.GetComponent<RoR2.CharacterBody>();
			this.skillLocator = base.GetComponent<RoR2.SkillLocator>();
		}

		public void FixedUpdate()
		{
			this.failsafeVulnerabilityForceTimer += Time.fixedDeltaTime;
			bool flag = false;
			bool flag2 = false;
			if ((bool)this.skillLocator)
			{
				if ((bool)this.skillLocator.primary && !this.skillLocator.primary.stateMachine.IsInMainState())
				{
					flag2 = true;
				}
				else if ((bool)this.skillLocator.secondary && !this.skillLocator.secondary.stateMachine.IsInMainState())
				{
					flag2 = true;
				}
				else if ((bool)this.skillLocator.utility && !this.skillLocator.utility.stateMachine.IsInMainState())
				{
					flag2 = true;
				}
				else if ((bool)this.skillLocator.special && !this.skillLocator.special.stateMachine.IsInMainState())
				{
					flag2 = true;
				}
				else if (!this.skillLocator.primary && !this.skillLocator.secondary && !this.skillLocator.utility && !this.skillLocator.special)
				{
					flag = true;
				}
			}
			if (flag2)
			{
				this.failsafeVulnerabilityForceTimer = 0f;
			}
			if (this.failsafeVulnerabilityForceTimer >= this.failsafeVulnerabilityForceThreshold && !this.body.isPlayerControlled)
			{
				flag = true;
			}
			if (this.vulnerableTimer <= 0f)
			{
				if (!flag2 && !flag && !this.body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility))
				{
					this.body.AddBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility);
				}
				else if ((flag2 || flag) && this.body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility))
				{
					this.body.RemoveBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility);
					this.vulnerableTimer = this.vulnerableDuration;
				}
			}
			else if (!flag2 && !flag)
			{
				this.vulnerableTimer -= Time.fixedDeltaTime;
			}
		}

		public void OnDisable()
		{
			if ((bool)this.body && this.body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility))
			{
				this.body.RemoveBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility);
				this.vulnerableTimer = 0f;
			}
		}
	}

	public static ConfigurableValue<float> forcedBreakOutDelay = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Aquamarine", "Bubble Duration", 5f, 0f, 1000f, "How long should this elite's on-hit bubble trap last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> cooldownBuffDuration = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Aquamarine", "Bubble Cooldown", 10f, 0f, 1000f, "Targets cannot be trapped again until this time passes. How long should this immunity last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<int> breakOutAttemptsRequired = ConfigurableValue.CreateInt("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Aquamarine", "Bubble Break Outs Required", 5, 0, 1000, "How many times should you press Interact to leave the bubble early?", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<int>)null);

	public static GameObject bubbleVehiclePrefab;

	public static GameObject bubbleBurstVFX;

	public override void OnPluginAwake()
	{
		((BaseLoadableAsset)this).OnPluginAwake();
		AffixWater.bubbleVehiclePrefab = Utils.CreateBlankPrefab("RisingTidesAffixWaterBubbleVehicle", true);
		AffixWater.bubbleVehiclePrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
	}

	public override void OnLoad()
	{
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_AffixWater";
		base.buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Water/texAffixWaterBuffIcon.png");
		On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
		On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
		Utils.CopyChildren(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/WaterBubbleTrap.prefab"), AffixWater.bubbleVehiclePrefab, true);
		AffixWater.bubbleVehiclePrefab.AddComponent<NetworkTransform>();
		AffixWater.bubbleVehiclePrefab.AddComponent<RisingTidesWaterBubbleTrap>();
		RoR2.VehicleSeat vehicleSeat = AffixWater.bubbleVehiclePrefab.AddComponent<RoR2.VehicleSeat>();
		vehicleSeat.passengerState = new SerializableEntityStateType(typeof(GenericCharacterVehicleSeated));
		vehicleSeat.seatPosition = AffixWater.bubbleVehiclePrefab.transform.Find("Scaler/SeatPosition");
		vehicleSeat.exitPosition = AffixWater.bubbleVehiclePrefab.transform.Find("Scaler/SeatPosition");
		vehicleSeat.ejectOnCollision = false;
		vehicleSeat.hidePassenger = true;
		vehicleSeat.exitVelocityFraction = 2f;
		vehicleSeat.enterVehicleContextString = "";
		vehicleSeat.exitVehicleContextString = "RISINGTIDES_WATERBUBBLE_VEHICLE_EXIT_CONTEXT";
		vehicleSeat.disablePassengerMotor = true;
		vehicleSeat.isEquipmentActivationAllowed = false;
		RoR2.ObjectScaleCurve objectScaleCurve = AffixWater.bubbleVehiclePrefab.transform.Find("Scaler").gameObject.AddComponent<RoR2.ObjectScaleCurve>();
		objectScaleCurve.useOverallCurveOnly = true;
		objectScaleCurve.overallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		objectScaleCurve.timeMax = 0.12f;
		GenericGameEvents.OnHitEnemy += new DamageAttackerVictimEventHandler(GenericGameEvents_OnHitEnemy);
		AffixWater.bubbleBurstVFX = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/BubbleBurstVFX.prefab");
		RoR2.EffectComponent effectComponent = AffixWater.bubbleBurstVFX.AddComponent<RoR2.EffectComponent>();
		effectComponent.applyScale = true;
		effectComponent.soundName = "Play_item_proc_squidTurret_shotExplode";
		RoR2.VFXAttributes vFXAttributes = AffixWater.bubbleBurstVFX.AddComponent<RoR2.VFXAttributes>();
		vFXAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Medium;
		vFXAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;
		AffixWater.bubbleBurstVFX.AddComponent<RoR2.DestroyOnTimer>().duration = 0.5f;
		RisingTidesContent.Resources.effectPrefabs.Add(AffixWater.bubbleBurstVFX);
		if (RisingTidesPlugin.mysticsItemsCompatibility)
		{
			RoR2.RoR2Application.onLoad = (Action)Delegate.Combine(RoR2.RoR2Application.onLoad, (Action)delegate
			{
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, AffixWater.bubbleBurstVFX, RoR2.RoR2Content.Buffs.Slow80, RoR2.DotController.DotIndex.None, 0f, 0f, DamageType.Generic);
			});
		}
	}

	private void GenericGameEvents_OnHitEnemy(RoR2.DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && (bool)attackerInfo.body && attackerInfo.body.HasBuff(base.buffDef) && (bool)victimInfo.body && !victimInfo.body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterBubbleTrapCooldown) && !victimInfo.body.currentVehicle)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(AffixWater.bubbleVehiclePrefab, victimInfo.body.corePosition, Quaternion.identity);
			gameObject.transform.localScale = Vector3.one * (1.5f + victimInfo.body.radius);
			gameObject.GetComponent<RoR2.VehicleSeat>().AssignPassenger(victimInfo.gameObject);
			RisingTidesWaterBubbleTrap component = gameObject.GetComponent<RisingTidesWaterBubbleTrap>();
			component.forcedBreakOutDelay *= damageInfo.procCoefficient;
			component.cooldownBuffDuration *= damageInfo.procCoefficient;
			NetworkServer.Spawn(gameObject);
		}
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		base.buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Water;
	}

	private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
	{
		orig(self, buffDef);
		if (NetworkServer.active && buffDef == base.buffDef)
		{
			RisingTidesAffixWaterBehaviour component = self.GetComponent<RisingTidesAffixWaterBehaviour>();
			if (!component)
			{
				component = self.gameObject.AddComponent<RisingTidesAffixWaterBehaviour>();
			}
			else if (!component.enabled)
			{
				component.enabled = true;
			}
		}
	}

	private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
	{
		orig(self, buffDef);
		if (NetworkServer.active && buffDef == base.buffDef)
		{
			RisingTidesAffixWaterBehaviour component = self.GetComponent<RisingTidesAffixWaterBehaviour>();
			if ((bool)component && component.enabled)
			{
				component.enabled = false;
			}
		}
	}
}
