// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// GrooveSaladSpikestripContent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// GrooveSaladSpikestripContent.Content.YeahElite
using System.Collections.Generic;
using System.Linq;
using GrooveSaladSpikestripContent;
using GrooveSaladSpikestripContent.Content;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

public class YeahElite : SpikestripEliteBase<YeahElite>
{
	[RequireComponent(typeof(RoR2.TeamFilter))]
	public class YeahElitePulseController : MonoBehaviour
	{
		private RoR2.TeamFilter teamFilter;

		private float pulseTimer;

		public float interval;

		public float buffDuration;

		public float radius;

		public BuffIndex buffIndex;

		private void Awake()
		{
			this.teamFilter = base.GetComponent<RoR2.TeamFilter>();
		}

		private void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				this.pulseTimer += Time.fixedDeltaTime;
				if (this.pulseTimer >= this.interval)
				{
					this.pulseTimer -= this.interval;
					GameObject gameObject = Object.Instantiate(YeahElite.yeahPulsePrefab, base.transform.position, base.transform.rotation);
					RoR2.PulseController component = gameObject.GetComponent<RoR2.PulseController>();
					component.performSearch += PerformSearch;
					component.onPulseHit += OnPulseHit;
					component.StartPulseServer();
					NetworkServer.Spawn(gameObject);
				}
			}
		}

		public void PerformSearch(RoR2.PulseController pulseController, Vector3 origin, float radius, List<RoR2.PulseController.PulseSearchResult> dest)
		{
			foreach (RoR2.TeamComponent teamMember in RoR2.TeamComponent.GetTeamMembers(this.teamFilter.teamIndex))
			{
				if ((teamMember.transform.position - origin).sqrMagnitude <= radius * radius)
				{
					RoR2.CharacterBody component = teamMember.GetComponent<RoR2.CharacterBody>();
					if ((bool)component)
					{
						dest.Add(new RoR2.PulseController.PulseSearchResult
						{
							hitObject = component,
							hitPos = teamMember.transform.position
						});
					}
				}
			}
		}

		public void OnPulseHit(RoR2.PulseController pulseController, RoR2.PulseController.PulseHit hitInfo)
		{
			RoR2.CharacterBody characterBody = (RoR2.CharacterBody)hitInfo.hitObject;
			if ((bool)characterBody)
			{
				characterBody.AddTimedBuff(this.buffIndex, this.buffDuration);
			}
		}
	}

	public static BuffIndex[] availableBuffIndices;

	public static GameObject yeahDeathWardPrefab;

	public static GameObject yeahPulsePrefab;

	public override string EliteName => "Yeah";

	public override Color EliteColor => SpikestripContentBase.ColorRGB(255f, 255f, 255f);

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<YeahElite>.tierOneEliteDefault;

	public override EliteTierDefinition[] ExtraEliteTierDefitions => new EliteTierDefinition[1] { SpikestripEliteBase<YeahElite>.honorEliteDefault };

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Yeahhhhhhhhh";

	public override string AffixDescriptionMainWord => "yeah";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteYeah.png");

	public override bool HookOnHitEnemy => false;

	public override bool IsEnabled => false;

	public bool BuffFilter(RoR2.BuffDef buffDef)
	{
		return buffDef.isDebuff && !buffDef.isCooldown && !buffDef.canStack && !buffDef.isHidden;
	}

	public override void Init()
	{
		base.Init();
		On.RoR2.DotController.InitDotCatalog += DotController_InitDotCatalog;
		RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
		YeahElite.yeahDeathWardPrefab = SpikestripContentBase.LegacyLoad<GameObject>("Prefabs/NetworkedObjects/WarbannerWard").InstantiateClone("YeahEliteDeathWard", registerNetwork: true);
		SpikestripContentBase.DestroyImmediate(YeahElite.yeahDeathWardPrefab.GetComponent<RoR2.BuffWard>());
		YeahElitePulseController yeahElitePulseController = YeahElite.yeahDeathWardPrefab.AddComponent<YeahElitePulseController>();
		yeahElitePulseController.radius = 15f;
		yeahElitePulseController.interval = 1f;
		yeahElitePulseController.buffDuration = 2f;
		SpikestripContentBase.networkedObjectContent.Add(YeahElite.yeahDeathWardPrefab);
		YeahElite.yeahPulsePrefab = SpikestripContentBase.LegacyLoad<GameObject>("Prefabs/NetworkedObjects/MoonBatteryDesignPulse").InstantiateClone("YeahElitePulse", registerNetwork: true);
		SpikestripContentBase.networkedObjectContent.Add(YeahElite.yeahPulsePrefab);
	}

	private void GlobalEventManager_onCharacterDeathGlobal(RoR2.DamageReport damageReport)
	{
		RoR2.CharacterBody characterBody = (damageReport.victim ? damageReport.victim.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)characterBody && characterBody.HasBuff(base.AffixBuff))
		{
			GameObject gameObject = Object.Instantiate(YeahElite.yeahDeathWardPrefab, characterBody.corePosition, Quaternion.identity);
			gameObject.GetComponent<YeahElitePulseController>().buffIndex = YeahElite.availableBuffIndices[Random.Range(0, YeahElite.availableBuffIndices.Length)];
			gameObject.GetComponent<RoR2.TeamFilter>().teamIndex = damageReport.victimTeamIndex;
			NetworkServer.Spawn(gameObject);
		}
	}

	private void DotController_InitDotCatalog(On.RoR2.DotController.orig_InitDotCatalog orig)
	{
		orig();
		List<BuffIndex> associatedWithDebuffs = new List<BuffIndex>();
		for (int i = 0; i < RoR2.DotController.dotDefs.Length; i++)
		{
			RoR2.BuffDef associatedBuff = RoR2.DotController.dotDefs[i].associatedBuff;
			if ((bool)associatedBuff)
			{
				associatedWithDebuffs.Add(associatedBuff.buffIndex);
			}
		}
		YeahElite.availableBuffIndices = (from buffDef in RoR2.BuffCatalog.buffDefs
			where !buffDef.isDebuff && !buffDef.isCooldown && !buffDef.isHidden && !buffDef.eliteDef && !associatedWithDebuffs.Contains(buffDef.buffIndex)
			select buffDef.buffIndex).ToArray();
	}

	public override void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(SpikestripContentBase.ColorRGB(178f, 98f, 200f));
		base.EquipmentDisplayModel = SpikestripContentBase.nullModel;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
