// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// GrooveSaladSpikestripContent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// GrooveSaladSpikestripContent.Content.Spread
using GrooveSaladSpikestripContent;
using GrooveSaladSpikestripContent.Content;
using On.RoR2.Projectile;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

public class Spread : SpikestripArtifactBase<Spread>
{
	public enum SplitType
	{
		None,
		Normal,
		Radius
	}

	public static float speedMultiplier = 0.5f;

	public static float coefficientMultiplier = 0.4f;

	public static float sizeMultiplier = 0.6f;

	public static int shardCount = 5;

	public bool CopyAetherium;

	public override string Name => "Spread";

	public override string Description => "Projectiles are split into weaker fragments.";

	public override Sprite EnabledIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texArtifactSpreadEnabled.png");

	public override Sprite DisabledIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texArtifactSpreadDisabled.png");

	public override void Init()
	{
		base.Init();
	}

	public override void SetHooks()
	{
		On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
		On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
		On.RoR2.Projectile.ProjectileGhostController.Awake += ProjectileGhostController_Awake;
	}

	private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, FireProjectileInfo fireProjectileInfo)
	{
		if (base.ArtifactActive() && !this.CopyAetherium)
		{
			SplitType splitType = this.CanProjectileSplit(fireProjectileInfo);
			if (splitType == SplitType.None)
			{
				return;
			}
			Vector3 aimDirection = fireProjectileInfo.rotation * Vector3.forward;
			Vector3 position = fireProjectileInfo.position;
			Vector3 normalized = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
			float num = 360f / (float)Spread.shardCount;
			for (int i = 0; i < Spread.shardCount; i++)
			{
				if (splitType == SplitType.Normal)
				{
					fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimDirection, 0f, 15f, 1f, 0.5f));
				}
				if (splitType == SplitType.Radius)
				{
					fireProjectileInfo.position = new Ray(position, Quaternion.AngleAxis(num * (float)i, Vector3.up) * normalized).GetPoint(5f);
				}
				try
				{
					this.CopyAetherium = true;
					RoR2.Projectile.ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				}
				finally
				{
					this.CopyAetherium = false;
				}
			}
		}
		else
		{
			orig(self, fireProjectileInfo);
		}
	}

	private void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, RoR2.Projectile.ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
	{
		orig(projectileController, fireProjectileInfo);
		SplitType splitType = this.CanProjectileSplit(fireProjectileInfo);
		GameObject gameObject = projectileController.gameObject;
		if (base.ArtifactActive() && (bool)gameObject && splitType != 0)
		{
			Transform transform = gameObject.transform;
			if ((bool)transform)
			{
				transform.localScale = new Vector3(transform.localScale.x * Spread.sizeMultiplier, transform.localScale.y * Spread.sizeMultiplier, transform.localScale.z * Spread.sizeMultiplier);
			}
			RoR2.Projectile.ProjectileDamage component = gameObject.GetComponent<RoR2.Projectile.ProjectileDamage>();
			if ((bool)component)
			{
				component.damage *= Spread.coefficientMultiplier;
				component.force *= Spread.coefficientMultiplier;
			}
			projectileController.procCoefficient *= Spread.coefficientMultiplier;
			RoR2.Projectile.ProjectileSimple component2 = gameObject.GetComponent<RoR2.Projectile.ProjectileSimple>();
			if ((bool)component2)
			{
				component2.desiredForwardSpeed *= Spread.speedMultiplier;
			}
			RoR2.Projectile.BoomerangProjectile component3 = gameObject.GetComponent<RoR2.Projectile.BoomerangProjectile>();
			if ((bool)component3)
			{
				component3.travelSpeed *= Spread.speedMultiplier;
			}
			RoR2.Projectile.MissileController component4 = gameObject.GetComponent<RoR2.Projectile.MissileController>();
			if ((bool)component4)
			{
				component4.acceleration *= Spread.speedMultiplier;
				component4.maxVelocity *= Spread.speedMultiplier;
				component4.rollVelocity *= Spread.speedMultiplier;
			}
			RoR2.Projectile.ProjectileExplosion[] components = gameObject.GetComponents<RoR2.Projectile.ProjectileExplosion>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].blastRadius *= Spread.sizeMultiplier;
			}
		}
	}

	private void ProjectileGhostController_Awake(On.RoR2.Projectile.ProjectileGhostController.orig_Awake orig, RoR2.Projectile.ProjectileGhostController self)
	{
		if (base.ArtifactActive())
		{
			ParticleSystem[] componentsInChildren = self.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Vector3 localScale = componentsInChildren[i].gameObject.transform.localScale;
				componentsInChildren[i].gameObject.transform.localScale = new Vector3(localScale.x * Spread.sizeMultiplier, localScale.y * Spread.sizeMultiplier, localScale.z * Spread.sizeMultiplier);
				ParticleSystem.MainModule main = componentsInChildren[i].main;
				main.scalingMode = ParticleSystemScalingMode.Local;
			}
		}
		orig(self);
		if (base.ArtifactActive() && (bool)self.transform)
		{
			self.transform.localScale = new Vector3(self.transform.localScale.x * Spread.sizeMultiplier, self.transform.localScale.y * Spread.sizeMultiplier, self.transform.localScale.z * Spread.sizeMultiplier);
		}
	}

	public override void UnsetHooks()
	{
		On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
		On.RoR2.Projectile.ProjectileManager.InitializeProjectile -= ProjectileManager_InitializeProjectile;
		On.RoR2.Projectile.ProjectileGhostController.Awake -= ProjectileGhostController_Awake;
	}

	private SplitType CanProjectileSplit(FireProjectileInfo fireProjectileInfo)
	{
		if ((bool)fireProjectileInfo.projectilePrefab)
		{
			RoR2.Projectile.ProjectileSimple component = fireProjectileInfo.projectilePrefab.GetComponent<RoR2.Projectile.ProjectileSimple>();
			RoR2.Projectile.BoomerangProjectile component2 = fireProjectileInfo.projectilePrefab.GetComponent<RoR2.Projectile.BoomerangProjectile>();
			if ((fireProjectileInfo.useSpeedOverride && fireProjectileInfo.speedOverride != 0f) | ((bool)component && !fireProjectileInfo.useSpeedOverride && component.desiredForwardSpeed != 0f) | ((bool)component2 && component2.travelSpeed != 0f))
			{
				return SplitType.Normal;
			}
			return SplitType.Radius;
		}
		return SplitType.None;
	}
}
