// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// GrooveSaladSpikestripContent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// GrooveSaladSpikestripContent.Content.DeathElite
using System;
using System.Collections.Generic;
using System.Linq;
using HG;
using MiscMods;
using R2API;
using RoR2;
using UnityEngine;

public class DeathElite
{
	public class DeathAffixBuffBehaviourServer : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver
	{
		public SphereSearch sphereSearch;

		public void Start()
		{
			if (base.body.healthComponent && !base.body.healthComponent.onIncomingDamageReceivers.Contains(this))
				ArrayUtils.ArrayAppend(ref base.body.healthComponent.onIncomingDamageReceivers, this);

			this.sphereSearch = new SphereSearch();
		}

		public void OnDestroy()
		{
			if (!base.body.healthComponent || !base.body.healthComponent.onIncomingDamageReceivers.Contains(this))
				return;

            var i = Array.IndexOf(base.body.healthComponent.onIncomingDamageReceivers, this);
			if (i != -1)
				ArrayUtils.ArrayRemoveAtAndResize(ref base.body.healthComponent.onIncomingDamageReceivers, i);
		}

		public void OnIncomingDamageServer(DamageInfo damageInfo)
		{
			if (!damageInfo.rejected && this.SpreadDamageToAlly())
			{
				damageInfo.rejected = true;
				DamageNumberManager.instance.SpawnDamageNumber(damageInfo.damage, damageInfo.position, damageInfo.crit, TeamIndex.None, DamageColorIndex.Default);
			}
		}

		public bool SpreadDamageToAlly()
		{
            var list = HG.ListPool<HurtBox>.RentCollection();
			TeamMask mask = default;
			mask.AddTeam(base.body.teamComponent.teamIndex);
			this.sphereSearch.radius = 25f;
			this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
			this.sphereSearch.origin = base.transform.position;
			this.sphereSearch.RefreshCandidates();
            this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            this.sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
			this.sphereSearch.OrderCandidatesByDistance();
			this.sphereSearch.GetHurtBoxes(list);

			foreach (var hurtBox in list)
			{
				if (hurtBox.healthComponent.body && !hurtBox.healthComponent.body.HasBuff(DeathElite.AffixBuff))
				{
					return true;
				}
			}
			return false;
		}
	}

	public string EliteName => "Yeah";

	public Color EliteColor => Color.blue;

	public Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public string EquipmentName => "Their Madness";

	public string AffixDescriptionMainWord => "death";

	public Texture2D EliteRampTexture => Assets.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteYeah.png");

	public Type AffixBuffBehaviour => typeof(DeathAffixBuffBehaviourServer);

	public bool ServerOnlyAffixBuffBehaviour => true;

	public bool IsEnabled => false;

	public static void Init()
	{
		var component = LegacyResourcesAPI.Load<GameObject>("prefabs/temporaryvisualeffects/DoppelgangerEffect").transform.Find("Particles/PulseEffect, Tentacles").GetComponent<ParticleSystemRenderer>();
		var material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", Color.red);
		component.material = material;
	}

	public void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(Color.blue);
	}

	public void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
