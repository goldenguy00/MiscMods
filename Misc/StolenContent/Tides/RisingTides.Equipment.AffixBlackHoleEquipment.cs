// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Equipment.AffixBlackHoleEquipment
using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates.GlobalSkills.LunarDetonator;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RisingTides.Equipment;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

public class AffixBlackHoleEquipment : BaseEliteAffix
{
	public class DetonationController
	{
		public HurtBox[] detonationTargets;

		public CharacterBody characterBody;

		public float damageStat;

		public bool isCrit;

		public float interval;

		private int i;

		private float timer;

		private bool _active;

		public bool active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active != value)
				{
					this._active = value;
					if (this._active)
					{
						RoR2Application.onFixedUpdate += FixedUpdate;
					}
					else
					{
						RoR2Application.onFixedUpdate -= FixedUpdate;
					}
				}
			}
		}

		private void FixedUpdate()
		{
			if (!this.characterBody || !this.characterBody.healthComponent || !this.characterBody.healthComponent.alive)
			{
				this.active = false;
				return;
			}
			this.timer -= Time.deltaTime;
			if (!(this.timer <= 0f))
			{
				return;
			}
			this.timer = this.interval;
			while (this.i < this.detonationTargets.Length)
			{
				try
				{
					HurtBox a = null;
					Util.Swap(ref a, ref this.detonationTargets[this.i]);
					if (this.DoDetonation(a))
					{
						break;
					}
				}
				catch (Exception)
				{
				}
				this.i++;
			}
			if (this.i >= this.detonationTargets.Length)
			{
				this.active = false;
			}
		}

		private bool DoDetonation(HurtBox targetHurtBox)
		{
			if (!targetHurtBox)
			{
				return false;
			}
			HealthComponent healthComponent = targetHurtBox.healthComponent;
			if (!healthComponent)
			{
				return false;
			}
			CharacterBody body = healthComponent.body;
			if (!body)
			{
				return false;
			}
			int buffCount = body.GetBuffCount(RisingTidesContent.Buffs.RisingTides_BlackHoleMark);
			if (buffCount <= 0)
			{
				return false;
			}
			BlackHoleDetonatorOrb orb = new BlackHoleDetonatorOrb
			{
				origin = this.characterBody.corePosition,
				target = targetHurtBox,
				attacker = this.characterBody.gameObject,
				damageValue = this.damageStat * (ConfigurableValue<float>.op_Implicit(AffixBlackHoleEquipment.detonationDamagePerMark) / 100f) * (float)buffCount,
				damageColorIndex = DamageColorIndex.Default,
				isCrit = this.isCrit,
				procChainMask = default(ProcChainMask),
				procCoefficient = 0f
			};
			OrbManager.instance.AddOrb(orb);
			return true;
		}
	}

	public class BlackHoleDetonatorOrb : GenericDamageOrb
	{
		public override void Begin()
		{
			base.speed = 120f;
			base.Begin();
		}

		public override GameObject GetOrbEffect()
		{
			return Detonate.orbEffectPrefab;
		}

		public override void OnArrival()
		{
			base.OnArrival();
			if ((bool)base.target)
			{
				EffectManager.SpawnEffect(Detonate.detonationEffectPrefab, new EffectData
				{
					origin = base.target.transform.position,
					rotation = Quaternion.identity,
					scale = 1f
				}, transmit: true);
			}
		}
	}

	public static ConfigurableValue<float> detonationDamagePerMark = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Onyx", "On Use Detonation Damage Per Mark", 100f, 0f, 1000f, "How much damage should this elite aspect's on-use detonation deal to each enemy per their debuff stack? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public override void OnLoad()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		base.OnLoad();
		((BaseEquipment)this).equipmentDef.name = "RisingTides_AffixBlackHole";
		((BaseEquipment)this).equipmentDef.cooldown = 10f;
		((BaseEquipment)this).equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/BlackHole/texAffixBlackHoleEquipmentIcon.png");
		base.SetUpPickupModel();
		base.AdjustElitePickupMaterial(Color.black, 1.26f);
		((BaseItemLike)this).itemDisplayPrefab = ((BaseItemLike)this).PrepareItemDisplayModel(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/BlackHoleSharkFin.prefab").InstantiateClone("RisingTidesAffixBarrierHeadpiece", registerNetwork: false));
		Material sharedMaterial = ((BaseItemLike)this).itemDisplayPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
		Standard.Apply(sharedMaterial, (Properties)null);
		Standard.DisableEverything(sharedMaterial);
		BaseItemLike.onSetupIDRS += delegate
		{
			((BaseItemLike)this).AddDisplayRule("CommandoBody", "Chest", new Vector3(0f, 0.32207f, -0.17157f), new Vector3(61.58626f, 180f, 0f), new Vector3(0.06391f, 0.06391f, 0.06391f));
			((BaseItemLike)this).AddDisplayRule("HuntressBody", "Chest", new Vector3(1E-05f, 0.17123f, -0.12994f), new Vector3(84.38821f, 1E-05f, 180f), new Vector3(0.06639f, 0.08081f, 0.07337f));
			((BaseItemLike)this).AddDisplayRule("Bandit2Body", "Chest", new Vector3(0f, 0.2054f, -0.17354f), new Vector3(85.18521f, 1E-05f, 180f), new Vector3(0.06235f, 0.06235f, 0.06235f));
			((BaseItemLike)this).AddDisplayRule("ToolbotBody", "Chest", new Vector3(2E-05f, 2.46124f, -1.81533f), new Vector3(38.35899f, 180f, 0f), new Vector3(0.43989f, 0.43989f, 0.43989f));
			((BaseItemLike)this).AddDisplayRule("EngiBody", "Chest", new Vector3(0f, 0.2425f, -0.28941f), new Vector3(87.55103f, 180f, 0f), new Vector3(0.07816f, 0.07816f, 0.07816f));
			((BaseItemLike)this).AddDisplayRule("EngiTurretBody", "Head", new Vector3(-0.047f, 0.76482f, -0.91778f), new Vector3(333.2374f, 180f, 0f), new Vector3(0.2291f, 0.2291f, 0.2291f));
			((BaseItemLike)this).AddDisplayRule("EngiWalkerTurretBody", "Head", new Vector3(-0.009f, 1.37273f, -0.54161f), new Vector3(0f, 180f, 0f), new Vector3(0.21039f, 0.21039f, 0.21039f));
			((BaseItemLike)this).AddDisplayRule("MageBody", "Chest", new Vector3(0f, 0.0987f, -0.25746f), new Vector3(84.62386f, 180f, 0f), new Vector3(0.09645f, 0.09645f, 0.09645f));
			((BaseItemLike)this).AddDisplayRule("MercBody", "Chest", new Vector3(0f, 0.18503f, -0.28213f), new Vector3(89.38293f, 180f, 0f), new Vector3(0.05412f, 0.05412f, 0.05412f));
			((BaseItemLike)this).AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0f, -0.6238f, 0.0624f), new Vector3(-1E-05f, 180f, 180f), new Vector3(0.1368f, 0.1368f, 0.1368f));
			((BaseItemLike)this).AddDisplayRule("LoaderBody", "MechBase", new Vector3(0f, 0.19236f, -0.0823f), new Vector3(85.59611f, 1E-05f, 180f), new Vector3(0.07694f, 0.07694f, 0.07694f));
			((BaseItemLike)this).AddDisplayRule("CrocoBody", "SpineChest1", new Vector3(0f, 0.51759f, 0.59778f), new Vector3(327.5475f, 0f, 0f), new Vector3(0.58293f, 0.90288f, 0.91986f));
			((BaseItemLike)this).AddDisplayRule("CrocoBody", "Chest", new Vector3(0.33495f, 0.32714f, -2.0456f), new Vector3(79.5185f, 0f, 169.457f), new Vector3(0.41576f, 0.64396f, 0.65607f));
			((BaseItemLike)this).AddDisplayRule("CaptainBody", "Chest", new Vector3(0f, 0.2263f, -0.19729f), new Vector3(82.07296f, -2E-05f, 180f), new Vector3(0.12007f, 0.12007f, 0.12007f));
			((BaseItemLike)this).AddDisplayRule("WispBody", "Head", new Vector3(0f, 0.17172f, 0.67549f), new Vector3(71.77856f, 0f, 0f), new Vector3(0.15099f, 0.15099f, 0.15099f));
			((BaseItemLike)this).AddDisplayRule("JellyfishBody", "Hull2", new Vector3(-0.00275f, 0.52657f, -0.82924f), new Vector3(74.90458f, 180f, 0f), new Vector3(0.20951f, 0.20951f, 0.20951f));
			((BaseItemLike)this).AddDisplayRule("BeetleBody", "Chest", new Vector3(0f, 0.17605f, -0.66683f), new Vector3(76.58255f, 1E-05f, 180f), new Vector3(0.18442f, 0.18442f, 0.18442f));
			((BaseItemLike)this).AddDisplayRule("LemurianBody", "Chest", new Vector3(0f, -0.19474f, 1.38821f), new Vector3(71.18005f, 180f, 180f), new Vector3(0.97581f, 0.97581f, 0.97581f));
			((BaseItemLike)this).AddDisplayRule("HermitCrabBody", "Base", new Vector3(1E-05f, 0.68282f, -0.36087f), new Vector3(55.65141f, 180f, 0f), new Vector3(0.177f, 0.177f, 0.177f));
			((BaseItemLike)this).AddDisplayRule("ImpBody", "Chest", new Vector3(0f, 0.00037f, -0.06942f), new Vector3(71.90101f, 1E-05f, 180f), new Vector3(0.11643f, 0.11643f, 0.11643f));
			((BaseItemLike)this).AddDisplayRule("VultureBody", "Chest", new Vector3(-0.13905f, 0.40364f, -1.54408f), new Vector3(63.08728f, 1E-05f, 180f), new Vector3(0.86913f, 0.86913f, 0.86913f));
			((BaseItemLike)this).AddDisplayRule("RoboBallMiniBody", "ROOT", new Vector3(0f, 0.94821f, 0f), new Vector3(9.51164f, 180f, 0f), new Vector3(0.2675f, 0.2675f, 0.2675f));
			((BaseItemLike)this).AddDisplayRule("MiniMushroomBody", "Head", new Vector3(-0.23232f, -0.00838f, 0f), new Vector3(271.04f, 90f, 0f), new Vector3(0.29763f, 0.29763f, 0.29763f));
			((BaseItemLike)this).AddDisplayRule("BellBody", "Chain", new Vector3(-0.00488f, -0.01674f, -0.00297f), new Vector3(352.2896f, 56.70449f, 178.0383f), new Vector3(0.46573f, 0.46573f, 0.46573f));
			((BaseItemLike)this).AddDisplayRule("BeetleGuardBody", "Chest", new Vector3(-0.0651f, 1.12174f, -2.29832f), new Vector3(84.15964f, 47.39052f, 227.6368f), new Vector3(0.59316f, 0.59316f, 0.59316f));
			((BaseItemLike)this).AddDisplayRule("BisonBody", "Chest", new Vector3(0f, 0.08027f, 0.43315f), new Vector3(47.67143f, 180f, 180f), new Vector3(0.22756f, 0.22756f, 0.22756f));
			((BaseItemLike)this).AddDisplayRule("GolemBody", "Chest", new Vector3(0f, 0.56927f, -0.31342f), new Vector3(89.66428f, 180f, 0f), new Vector3(0.28954f, 0.28954f, 0.28954f));
			((BaseItemLike)this).AddDisplayRule("ParentBody", "Chest", new Vector3(-54.08593f, -0.00016f, 7E-05f), new Vector3(90f, 270f, 0f), new Vector3(30.77329f, 30.77329f, 30.77329f));
			((BaseItemLike)this).AddDisplayRule("ClayBruiserBody", "Chest", new Vector3(-2E-05f, 0.59846f, -0.56425f), new Vector3(72.47513f, 180f, 0f), new Vector3(0.14388f, 0.14388f, 0.14388f));
			((BaseItemLike)this).AddDisplayRule("ClayBruiserBody", "Muzzle", new Vector3(1E-05f, -0.35307f, -0.7658f), new Vector3(345.4555f, 180f, 180f), new Vector3(0.15894f, 0.15894f, 0.15894f));
			((BaseItemLike)this).AddDisplayRule("GreaterWispBody", "MaskBase", new Vector3(0f, 0.96585f, 0.5596f), new Vector3(314.1227f, 180f, 0f), new Vector3(0.14965f, 0.14965f, 0.14965f));
			((BaseItemLike)this).AddDisplayRule("LemurianBruiserBody", "Chest", new Vector3(0f, 1.29113f, 1.71805f), new Vector3(78.59312f, 0f, 0f), new Vector3(0.94043f, 0.94043f, 0.94043f));
			((BaseItemLike)this).AddDisplayRule("NullifierBody", "Muzzle", new Vector3(0f, 0.95026f, 0.34576f), new Vector3(356.5133f, 180f, 0f), new Vector3(0.50009f, 0.50009f, 0.50009f));
			((BaseItemLike)this).AddDisplayRule("BeetleQueen2Body", "Chest", new Vector3(0f, 1.2333f, 2.11659f), new Vector3(57.73215f, 180f, 180f), new Vector3(0.84575f, 0.84575f, 0.84575f));
			((BaseItemLike)this).AddDisplayRule("ClayBossBody", "PotLidTop", new Vector3(0f, 0.33325f, 1.12097f), new Vector3(0f, 180f, 0f), new Vector3(0.47481f, 0.47481f, 0.47481f));
			((BaseItemLike)this).AddDisplayRule("TitanBody", "Chest", new Vector3(0f, 3.78825f, -3.2316f), new Vector3(90f, 180f, 0f), new Vector3(1.35068f, 1.35068f, 1.35068f));
			((BaseItemLike)this).AddDisplayRule("TitanGoldBody", "Chest", new Vector3(0f, 3.78825f, -3.2316f), new Vector3(90f, 180f, 0f), new Vector3(1.35068f, 1.35068f, 1.35068f));
			((BaseItemLike)this).AddDisplayRule("VagrantBody", "Hull", new Vector3(0f, 0.68025f, -1.12448f), new Vector3(80.65052f, 180f, 0f), new Vector3(0.2448f, 0.2448f, 0.2448f));
			string[] array = new string[2] { "MagmaWormBody", "ElectricWormBody" };
			foreach (string text in array)
			{
				((BaseItemLike)this).AddDisplayRule(text, "UpperJaw", new Vector3(0f, 0f, -0.51749f), new Vector3(90f, 180f, 0f), new Vector3(0.33248f, 0.33248f, 0.33247f));
				for (int j = 1; j <= 16; j += 4)
				{
					Vector3 vector = Vector3.one * 0.33248f * Mathf.Pow(43f / 46f, j - 1);
					((BaseItemLike)this).AddDisplayRule(text, "Neck" + j, new Vector3(0.01423997f, 0.6766583f + 0.03293f * (float)(j - 1), -1.209069f + 0.02657f * (float)(j - 1)), new Vector3(270f, 0f, 0f), vector);
				}
			}
			((BaseItemLike)this).AddDisplayRule("RoboBallBossBody", "Shell", new Vector3(0f, 1.07915f, 0.01541f), new Vector3(0f, 180f, 0f), new Vector3(0.20578f, 0.20578f, 0.20578f));
			((BaseItemLike)this).AddDisplayRule("SuperRoboBallBossBody", "Shell", new Vector3(0f, 1.07915f, 0.01541f), new Vector3(0f, 180f, 0f), new Vector3(0.20578f, 0.20578f, 0.20578f));
			((BaseItemLike)this).AddDisplayRule("GravekeeperBody", "Neck1", new Vector3(0f, 2.76024f, 1.31344f), new Vector3(61.61956f, 0f, 0f), new Vector3(0.75697f, 0.75697f, 0.75697f));
			((BaseItemLike)this).AddDisplayRule("ImpBossBody", "Chest", new Vector3(0f, 0.28061f, -0.3357f), new Vector3(77.69851f, 1E-05f, 180f), new Vector3(0.64156f, 0.64156f, 0.64156f));
			((BaseItemLike)this).AddDisplayRule("GrandParentBody", "Chest", new Vector3(-1E-05f, 4.25656f, -4.74021f), new Vector3(70.63297f, 0f, 180f), new Vector3(1.26669f, 1.26669f, 1.26669f));
			((BaseItemLike)this).AddDisplayRule("ScavBody", "Chest", new Vector3(0f, 4.70675f, 1.65828f), new Vector3(22.40885f, 0f, 0f), new Vector3(2.4305f, 2.4305f, 2.4305f));
		};
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlBeetleQueen",
			transformLocation = "BeetleQueenArmature/ROOT/Base/Chest",
			childName = "Chest"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlGravekeeper",
			transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1",
			childName = "Neck1"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlGravekeeper",
			transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1/neck.2",
			childName = "Neck2"
		});
	}

	public override void OnUseClient(EquipmentSlot equipmentSlot)
	{
		((BaseEquipment)this).OnUseClient(equipmentSlot);
		if ((bool)equipmentSlot.characterBody)
		{
			EffectManager.SimpleImpactEffect(Detonate.enterEffectPrefab, equipmentSlot.characterBody.corePosition, Vector3.up, transmit: false);
			Util.PlaySound(Detonate.enterSoundString, equipmentSlot.gameObject);
		}
	}

	public override bool OnUse(EquipmentSlot equipmentSlot)
	{
		if ((bool)equipmentSlot.characterBody)
		{
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.filterByDistinctEntity = true;
			bullseyeSearch.filterByLoS = false;
			bullseyeSearch.maxDistanceFilter = float.PositiveInfinity;
			bullseyeSearch.minDistanceFilter = 0f;
			bullseyeSearch.minAngleFilter = 0f;
			bullseyeSearch.maxAngleFilter = 180f;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
			bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(equipmentSlot.teamComponent.teamIndex);
			bullseyeSearch.searchOrigin = equipmentSlot.characterBody.corePosition;
			bullseyeSearch.viewer = null;
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(equipmentSlot.gameObject);
			HurtBox[] detonationTargets = bullseyeSearch.GetResults().ToArray();
			DetonationController detonationController = new DetonationController();
			detonationController.characterBody = equipmentSlot.characterBody;
			detonationController.interval = Detonate.detonationInterval;
			detonationController.detonationTargets = detonationTargets;
			detonationController.damageStat = equipmentSlot.characterBody.damage;
			detonationController.isCrit = equipmentSlot.characterBody.RollCrit();
			detonationController.active = true;
			return true;
		}
		return false;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		((BaseEquipment)this).equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixBlackHole;
	}
}
