// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// GrooveSaladSpikestripContent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// GrooveSaladSpikestripContent.Content.RadiantElite
using System;
using System.Collections.Generic;
using System.Linq;
using GrooveSaladSpikestripContent;
using GrooveSaladSpikestripContent.Content;
using On.RoR2;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

public class RadiantElite : SpikestripEliteBase<RadiantElite>
{
	public class RadiantBeamController : MonoBehaviour
	{
		private LineRenderer lineRenderer;

		private Rigidbody rb;

		private Ray ray;

		public Transform sunTransform;

		public RoR2.CharacterBody body;

		private Transform target;

		public void Start()
		{
			this.lineRenderer = base.GetComponent<LineRenderer>();
			this.lineRenderer.enabled = false;
			this.rb = base.GetComponent<Rigidbody>();
			this.ray = default(Ray);
		}

		public void FixedUpdate()
		{
			this.UpdateBeam();
		}

		public void UpdateBeam()
		{
			if (!this.target)
			{
				if ((bool)this.rb)
				{
					this.rb.velocity = Vector3.zero;
				}
			}
			else
			{
				if (!this.sunTransform || !this.body)
				{
					return;
				}
				Vector3 position = this.sunTransform.position;
				if ((bool)this.lineRenderer)
				{
					this.lineRenderer.SetPosition(0, position);
				}
				if (!this.rb || !this.body)
				{
					return;
				}
				Vector3 vector = this.target.position - this.rb.position;
				this.rb.AddForce(vector * 0.2f, ForceMode.VelocityChange);
				Vector3 direction = this.rb.position - position;
				this.ray.origin = position;
				this.ray.direction = direction;
				Vector3 point = this.ray.GetPoint(200f);
				if (Physics.SphereCast(this.ray, 1f, out var hitInfo, 200f, (int)RoR2.LayerIndex.entityPrecise.mask | (int)RoR2.LayerIndex.world.mask))
				{
					point = hitInfo.point;
					RoR2.HurtBox hurtBox = (hitInfo.collider ? hitInfo.collider.GetComponent<RoR2.HurtBox>() : null);
					if ((bool)hurtBox && NetworkServer.active)
					{
						RoR2.HealthComponent healthComponent = hurtBox.healthComponent;
						if ((bool)healthComponent)
						{
							BeamImmunity component = healthComponent.GetComponent<BeamImmunity>();
							if ((!component || ((bool)component && component.attacker != this)) && RoR2.FriendlyFireManager.ShouldDirectHitProceed(healthComponent, this.body.teamComponent.teamIndex))
							{
								RoR2.DamageInfo damageInfo = new RoR2.DamageInfo();
								damageInfo.attacker = this.body.gameObject;
								damageInfo.inflictor = this.body.gameObject;
								damageInfo.damage = this.body.damage * 1f;
								damageInfo.crit = this.body.RollCrit();
								damageInfo.force = Vector3.zero;
								damageInfo.procCoefficient = 1f;
								damageInfo.damageType = DamageType.Generic;
								damageInfo.damageColorIndex = DamageColorIndex.Default;
								damageInfo.position = point;
								healthComponent.TakeDamage(damageInfo);
								RoR2.GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
								RoR2.GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
								component = healthComponent.gameObject.AddComponent<BeamImmunity>();
								component.timer = 0.5f;
								component.attacker = this;
							}
						}
					}
				}
				if ((bool)this.lineRenderer)
				{
					this.lineRenderer.SetPosition(1, point);
				}
			}
		}

		public void SetNewTarget(Transform newTarget)
		{
			this.target = newTarget;
			this.lineRenderer.enabled = newTarget;
		}
	}

	public class BeamImmunity : MonoBehaviour
	{
		public float timer;

		public RadiantBeamController attacker;

		public void FixedUpdate()
		{
			this.timer -= Time.fixedDeltaTime;
			if (this.timer <= 0f)
			{
				UnityEngine.Object.Destroy(this);
			}
		}
	}

	public class RadiantAffixBuffBehaviour : RoR2.CharacterBody.ItemBehavior
	{
		public static float missileFireInterval = 0.25f;

		private float missileTimer;

		public List<float> missileQueueServer = new List<float>();

		public static int beamCount = 3;

		private GameObject radiantOrbInstance;

		private GameObject radiantBeamInstance;

		private List<GameObject> beamObjects = new List<GameObject>();

		private RoR2.BullseyeSearch search = new RoR2.BullseyeSearch();

		private float targetUpdateStopwatch;

		public static float targetUpdateInterval = 0.2f;

		public Vector3 orbPosition => base.body.corePosition + Vector3.up * (base.body.bestFitRadius + 10f);

		public void Start()
		{
			this.radiantOrbInstance = UnityEngine.Object.Instantiate(RadiantElite.radiantOrb, this.orbPosition, Quaternion.identity);
			this.radiantOrbInstance.SetActive(value: true);
			this.radiantBeamInstance = UnityEngine.Object.Instantiate(RadiantElite.radiantBeam, base.body.corePosition, RoR2.Util.QuaternionSafeLookRotation(new Vector3(0f, 0f, 1f)));
			this.radiantBeamInstance.transform.RotateAround(base.transform.position, Vector3.forward, 180f);
			this.radiantBeamInstance.SetActive(value: true);
			for (int i = 0; i < RadiantAffixBuffBehaviour.beamCount; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(RadiantElite.radiantBeamControllerPrefab, base.body.corePosition + UnityEngine.Random.insideUnitSphere * 10f, Quaternion.identity);
				RadiantBeamController component = gameObject.GetComponent<RadiantBeamController>();
				component.sunTransform = this.radiantOrbInstance.transform;
				component.body = base.body;
				this.beamObjects.Add(gameObject);
			}
		}

		public void OnDestroy()
		{
			if ((bool)this.radiantOrbInstance)
			{
				UnityEngine.Object.Destroy(this.radiantOrbInstance);
			}
			if ((bool)this.radiantBeamInstance)
			{
				UnityEngine.Object.Destroy(this.radiantBeamInstance);
			}
			for (int i = 0; i < this.beamObjects.Count; i++)
			{
				if ((bool)this.beamObjects[i])
				{
					UnityEngine.Object.Destroy(this.beamObjects[i]);
				}
			}
		}

		public void FixedUpdate()
		{
			if ((bool)this.radiantOrbInstance)
			{
				this.radiantOrbInstance.transform.position = this.orbPosition;
			}
			if ((bool)this.radiantBeamInstance)
			{
				this.radiantBeamInstance.transform.position = base.body.corePosition;
			}
			if (NetworkServer.active)
			{
				this.UpdateMissilesServer();
			}
			this.targetUpdateStopwatch += Time.fixedDeltaTime;
			if (this.targetUpdateStopwatch >= RadiantAffixBuffBehaviour.targetUpdateInterval)
			{
				this.targetUpdateStopwatch -= RadiantAffixBuffBehaviour.targetUpdateInterval;
				Ray aimRay = new Ray(this.orbPosition, base.body.inputBank ? base.body.inputBank.aimDirection : base.body.transform.forward);
				this.UpdateTarget(aimRay);
			}
		}

		private void UpdateTarget(Ray aimRay)
		{
			this.search.teamMaskFilter = RoR2.TeamMask.GetUnprotectedTeams(base.body.teamComponent.teamIndex);
			this.search.filterByLoS = true;
			this.search.searchOrigin = aimRay.origin;
			this.search.searchDirection = aimRay.direction;
			this.search.sortMode = RoR2.BullseyeSearch.SortMode.Angle;
			this.search.maxDistanceFilter = 100f;
			this.search.maxAngleFilter = 180f;
			this.search.RefreshCandidates();
			this.search.FilterOutGameObject(base.gameObject);
			RoR2.HurtBox hurtBox = this.search.GetResults().FirstOrDefault();
			Transform newTarget = (hurtBox ? hurtBox.transform : null);
			for (int i = 0; i < RadiantAffixBuffBehaviour.beamCount; i++)
			{
				this.beamObjects[i].GetComponent<RadiantBeamController>().SetNewTarget(newTarget);
			}
		}

		public void UpdateMissilesServer()
		{
			if (this.missileTimer <= 0f)
			{
				if (this.missileQueueServer.Count != 0)
				{
					Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
					FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
					fireProjectileInfo.projectilePrefab = RadiantElite.missileProjectilePrefab;
					fireProjectileInfo.position = this.orbPosition;
					fireProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(onUnitSphere);
					fireProjectileInfo.owner = base.gameObject;
					fireProjectileInfo.damage = this.missileQueueServer[0];
					fireProjectileInfo.crit = base.body.RollCrit();
					fireProjectileInfo.force = 200f;
					FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
					ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
					this.missileQueueServer.RemoveAt(0);
				}
			}
			else
			{
				this.missileTimer -= Time.fixedDeltaTime;
			}
		}
	}

	public static Material matTracerBright = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerCaptainDefenseMatrix").transform.Find("TracerHead/Flash").gameObject.GetComponent<ParticleSystemRenderer>().material;

	public static Material goldenMaterial;

	public static Material radiantParticleReplacement;

	public static GameObject missileProjectilePrefab;

	public static GameObject missileProjectileGhost;

	public static GameObject spawnMissileEffect;

	public static GameObject radiantOrb;

	public static GameObject radiantBeam;

	public static GameObject radiantBeamControllerPrefab;

	public override string EliteName => "Radiant";

	public override Color EliteColor => Color.white;

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<RadiantElite>.tierTwoEliteDefault;

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Their Guidance";

	public override string AffixDescriptionMainWord => "brilliance";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteRadiant.png");

	public override Type AffixBuffBehaviour => typeof(RadiantAffixBuffBehaviour);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override bool IsEnabled => false;

	public override void Init()
	{
		base.Init();
		On.RoR2.CharacterModel.UpdateLights += CharacterModel_UpdateLights;
		RadiantElite.goldenMaterial = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matOnFire"));
		RadiantElite.goldenMaterial.mainTexture = null;
		RadiantElite.goldenMaterial.DisableKeyword("USE_CLOUDS");
		RadiantElite.goldenMaterial.DisableKeyword("USE_UV1");
		SpikestripVisuals.RegisterOverlayMaterial(RadiantElite.goldenMaterial, (RoR2.CharacterModel model) => model.body.HasBuff(base.AffixBuff));
		RadiantElite.radiantParticleReplacement = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matEliteLunarParticleReplacement"));
		RadiantElite.radiantParticleReplacement.SetTexture("_RemapTex", RadiantElite.goldenMaterial.GetTexture("_RemapTex"));
		RadiantElite.radiantOrb = LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/GrandparentGravSphereGhost").InstantiateClone("RadiantOrb", registerNetwork: false);
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantOrb.GetComponent<ProjectileGhostController>());
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantOrb.transform.Find("Particles/Goo, Drip").gameObject);
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantOrb.transform.Find("Particles/GlowParticles, Fast").gameObject);
		RadiantElite.radiantOrb.transform.Find("Particles/GlowParticles, Fast (1)").localScale = 0.05f * Vector3.one;
		RadiantElite.radiantOrb.transform.Find("Particles/SoftGlow, Backdrop").localScale = 0.8f * Vector3.one;
		ParticleSystemRenderer[] componentsInChildren = RadiantElite.radiantOrb.GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer particleSystemRenderer in componentsInChildren)
		{
			if (particleSystemRenderer.gameObject.name != "SoftGlow, Backdrop" && particleSystemRenderer.gameObject.name != "Sphere")
			{
				particleSystemRenderer.material = RadiantElite.radiantParticleReplacement;
			}
		}
		Color color = SpikestripContentBase.ColorRGB(255f, 221f, 73f);
		ParticleSystemRenderer component = RadiantElite.radiantOrb.transform.Find("Particles/Sphere").GetComponent<ParticleSystemRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetTexture("_RemapTex", LegacyResourcesAPI.Load<Texture>("textures/texWhite"));
		material.SetColor("_TintColor", color);
		component.material = material;
		RadiantElite.radiantOrb.GetComponentInChildren<Light>().color = color;
		RoR2.FlickerLight componentInChildren = RadiantElite.radiantOrb.GetComponentInChildren<RoR2.FlickerLight>();
		componentInChildren.sinWaves[0].period = 1f;
		componentInChildren.sinWaves[0].frequency = 1f;
		componentInChildren.sinWaves[1].period = 2f;
		componentInChildren.sinWaves[1].frequency = 0.5f;
		Color value = SpikestripContentBase.ColorRGB(255f, 203f, 73f);
		RadiantElite.radiantBeam = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/TimeCrystalBody").transform.Find("ModelBase/Mesh/Beam").gameObject.InstantiateClone("RadiantBeam", registerNetwork: false);
		ParticleSystemRenderer component2 = RadiantElite.radiantBeam.GetComponent<ParticleSystemRenderer>();
		Material material2 = UnityEngine.Object.Instantiate(RadiantElite.matTracerBright);
		material2.SetColor("_TintColor", value);
		material2.mainTexture = null;
		component2.material = material2;
		RadiantElite.radiantBeamControllerPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/BleedEffect").InstantiateClone("RadiantBeamControllerPrefab", registerNetwork: false);
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantBeamControllerPrefab.transform.Find("Burst").gameObject);
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantBeamControllerPrefab.GetComponent<RoR2.LoopSound>());
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantBeamControllerPrefab.GetComponent<AkEvent>());
		SpikestripContentBase.DestroyImmediate(RadiantElite.radiantBeamControllerPrefab.GetComponent<AkGameObj>());
		Rigidbody component3 = RadiantElite.radiantBeamControllerPrefab.GetComponent<Rigidbody>();
		component3.isKinematic = false;
		component3.mass = 600f;
		LineRenderer lineRenderer = RadiantElite.radiantBeamControllerPrefab.AddComponent<LineRenderer>();
		lineRenderer.material = material2;
		lineRenderer.startColor = Color.white;
		lineRenderer.startWidth = 1f;
		lineRenderer.endColor = Color.white;
		lineRenderer.endWidth = 1f;
		lineRenderer.positionCount = 2;
		RadiantBeamController radiantBeamController = RadiantElite.radiantBeamControllerPrefab.AddComponent<RadiantBeamController>();
		RadiantElite.missileProjectileGhost = LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/MissileGhost").InstantiateClone("RadiantMissileGhost", registerNetwork: false);
		TrailRenderer componentInChildren2 = RadiantElite.missileProjectileGhost.GetComponentInChildren<TrailRenderer>();
		componentInChildren2.material = material2;
		componentInChildren2.startColor = Color.white;
		componentInChildren2.startWidth = 1f;
		componentInChildren2.endColor = Color.white;
		componentInChildren2.endWidth = 0f;
		RadiantElite.missileProjectilePrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/MissileProjectile").InstantiateClone("RadiantMissile", registerNetwork: true);
		ProjectileController component4 = RadiantElite.missileProjectilePrefab.GetComponent<ProjectileController>();
		component4.ghostPrefab = RadiantElite.missileProjectileGhost;
		component4.procCoefficient = 0f;
		SpikestripContentBase.projectilePrefabContent.Add(RadiantElite.missileProjectilePrefab);
		RadiantElite.missileProjectilePrefab.RegisterNetworkPrefab();
	}

	private void CharacterModel_UpdateLights(On.RoR2.CharacterModel.orig_UpdateLights orig, RoR2.CharacterModel self)
	{
		if (self.myEliteIndex == base.MainEliteDef.eliteIndex)
		{
			self.lightColorOverride = SpikestripContentBase.ColorRGB(255f, 221f, 73f);
			self.particleMaterialOverride = RadiantElite.radiantParticleReplacement;
		}
		orig(self);
	}

	public override void OnHitEnemyServer(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		GameObject attacker = damageInfo.attacker;
		RoR2.CharacterBody characterBody = (attacker ? attacker.GetComponent<RoR2.CharacterBody>() : null);
		if (!attacker || !characterBody)
		{
			return;
		}
		RadiantAffixBuffBehaviour component = attacker.GetComponent<RadiantAffixBuffBehaviour>();
		if ((bool)component && characterBody.HasBuff(base.AffixBuff))
		{
			for (int i = 0; i < 3; i++)
			{
				component.missileQueueServer.Add(RoR2.Util.OnHitProcDamage(damageInfo.damage, characterBody.damage, 0.2f));
			}
		}
	}

	public override void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(Color.yellow);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
