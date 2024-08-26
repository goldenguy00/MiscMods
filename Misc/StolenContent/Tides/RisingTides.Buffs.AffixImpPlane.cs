// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.AffixImpPlane
using System;
using System.Collections.Generic;
using MysticsItems;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using On.RoR2;
using R2API;
using RisingTides;
using RisingTides.Buffs;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AffixImpPlane : BaseBuff
{
	public class RisingTidesAffixImpPlaneBehaviour : MonoBehaviour
	{
		public RoR2.CharacterBody body;

		public GameObject riftObject;

		public float projectileTimer;

		public void Awake()
		{
			this.body = base.GetComponent<RoR2.CharacterBody>();
			this.CreateRift();
		}

		public void CreateRift()
		{
			if (!this.riftObject)
			{
				this.riftObject = UnityEngine.Object.Instantiate(AffixImpPlane.riftPrefab, base.transform.position + Vector3.up * 3f, Quaternion.identity);
			}
		}

		public void FixedUpdate()
		{
			if ((bool)this.body.healthComponent && this.body.healthComponent.alive)
			{
				this.projectileTimer -= Time.fixedDeltaTime;
				if (this.projectileTimer <= 0f)
				{
					this.projectileTimer += ConfigurableValue<float>.op_Implicit(AffixImpPlane.riftProjectileInterval);
					this.FireProjectile();
				}
			}
		}

		public void FireProjectile()
		{
			if ((bool)this.riftObject && this.body.hasEffectiveAuthority)
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.position = this.riftObject.transform.position;
				fireProjectileInfo.rotation = Quaternion.Euler(RoR2.RoR2Application.rng.RangeFloat(-60f, 60f), RoR2.RoR2Application.rng.RangeFloat(0f, 360f), 0f);
				fireProjectileInfo.crit = this.body.RollCrit();
				fireProjectileInfo.damage = this.body.damage * (ConfigurableValue<float>.op_Implicit(AffixImpPlane.riftProjectileDamage) / 100f);
				fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
				fireProjectileInfo.projectilePrefab = AffixImpPlane.riftProjectilePrefab;
				fireProjectileInfo.speedOverride = 12f;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public void OnEnable()
		{
			this.CreateRift();
		}

		public void OnDisable()
		{
			UnityEngine.Object.Destroy(this.riftObject);
		}
	}

	public static ConfigurableValue<float> riftProjectileInterval = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Realgar", "Rift Projectile Interval", 0.27f, 0f, 1000f, "How often should the rift shoot a projectile? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> riftProjectileDamage = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Realgar", "Rift Projectile Damage", 60f, 0f, 1000f, "How much damage should the rift's projectiles deal? (in %, relative to the owner's damage)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static ConfigurableValue<float> scarDuration = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Realgar", "Scar Duration", 4f, 0f, 1000f, "How long should this elite's debuff DoT last? (in seconds)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static GameObject riftPrefab;

	public static GameObject riftProjectilePrefab;

	public static DamageAPI.ModdedDamageType scarDamageType;

	public static DamageAPI.ModdedDamageType cannotScarDamageType;

	public static GameObject scarVFX;

	public override void OnPluginAwake()
	{
		((BaseLoadableAsset)this).OnPluginAwake();
		AffixImpPlane.scarDamageType = DamageAPI.ReserveDamageType();
		AffixImpPlane.cannotScarDamageType = DamageAPI.ReserveDamageType();
	}

	public override void OnLoad()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_AffixImpPlane";
		base.buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/ImpPlane/texAffixImpPlaneBuffIcon.png");
		Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/ImpPlane/matAffixImpPlaneOverlay.mat"), (Func<RoR2.CharacterModel, bool>)((RoR2.CharacterModel model) => (bool)model.body && model.body.HasBuff(base.buffDef)));
		On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
		On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
		GenericGameEvents.OnHitEnemy += new DamageAttackerVictimEventHandler(GenericGameEvents_OnHitEnemy);
		AffixImpPlane.riftPrefab = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/ImpPlane/ImpPlaneRift.prefab");
		AffixImpPlane.riftProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ArtifactShell/ArtifactShellSeekingSolarFlare.prefab").WaitForCompletion().InstantiateClone("RisingTidesAffixImpPlaneRiftProjectile", registerNetwork: true);
		AffixImpPlane.riftProjectilePrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(AffixImpPlane.cannotScarDamageType);
		RisingTidesContent.Resources.projectilePrefabs.Add(AffixImpPlane.riftProjectilePrefab);
		AffixImpPlane.scarVFX = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/ImpPlane/ImpPlaneScarVFX.prefab");
		RoR2.EffectComponent effectComponent = AffixImpPlane.scarVFX.AddComponent<RoR2.EffectComponent>();
		effectComponent.applyScale = true;
		effectComponent.soundName = "RisingTides_Play_realgar_scar";
		RoR2.VFXAttributes vFXAttributes = AffixImpPlane.scarVFX.AddComponent<RoR2.VFXAttributes>();
		vFXAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
		vFXAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Low;
		AffixImpPlane.scarVFX.AddComponent<RoR2.DestroyOnTimer>().duration = 2f;
		RisingTidesContent.Resources.effectPrefabs.Add(AffixImpPlane.scarVFX);
		if (RisingTidesPlugin.mysticsItemsCompatibility)
		{
			RoR2.RoR2Application.onLoad = (Action)Delegate.Combine(RoR2.RoR2Application.onLoad, (Action)delegate
			{
				OtherModCompat.ElitePotion_AddSpreadEffect(base.buffDef, Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Explosion.prefab").WaitForCompletion(), (RoR2.BuffDef)null, RoR2.DotController.DotIndex.None, 0f, 1f, AffixImpPlane.scarDamageType);
			});
		}
	}

	private void GenericGameEvents_OnHitEnemy(RoR2.DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		if (damageInfo.rejected || !(damageInfo.procCoefficient > 0f) || !attackerInfo.body || (!attackerInfo.body.HasBuff(base.buffDef) && !damageInfo.HasModdedDamageType(AffixImpPlane.scarDamageType)) || damageInfo.HasModdedDamageType(AffixImpPlane.cannotScarDamageType) || !victimInfo.body)
		{
			return;
		}
		if (!victimInfo.body.HasBuff(RisingTidesContent.Buffs.RisingTides_ImpPlaneScar))
		{
			RoR2.EffectData effectData = new RoR2.EffectData
			{
				origin = victimInfo.body.corePosition
			};
			effectData.SetNetworkedObjectReference(victimInfo.gameObject);
			RoR2.EffectManager.SpawnEffect(AffixImpPlane.scarVFX, effectData, transmit: true);
		}
		foreach (RoR2.TeamComponent teamMember in RoR2.TeamComponent.GetTeamMembers(victimInfo.teamIndex))
		{
			if ((bool)teamMember.body && !teamMember.body.HasBuff(RisingTidesContent.Buffs.RisingTides_ImpPlaneScar))
			{
				RoR2.DotController.InflictDot(teamMember.gameObject, attackerInfo.gameObject, ImpPlaneScar.dotIndex, ConfigurableValue<float>.op_Implicit(AffixImpPlane.scarDuration) * damageInfo.procCoefficient);
			}
		}
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		base.buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_ImpPlane;
	}

	private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
	{
		orig(self, buffDef);
		if (buffDef == base.buffDef)
		{
			RisingTidesAffixImpPlaneBehaviour component = self.GetComponent<RisingTidesAffixImpPlaneBehaviour>();
			if (!component)
			{
				component = self.gameObject.AddComponent<RisingTidesAffixImpPlaneBehaviour>();
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
		if (buffDef == base.buffDef)
		{
			RisingTidesAffixImpPlaneBehaviour component = self.GetComponent<RisingTidesAffixImpPlaneBehaviour>();
			if ((bool)component && component.enabled)
			{
				component.enabled = false;
			}
		}
	}
}
