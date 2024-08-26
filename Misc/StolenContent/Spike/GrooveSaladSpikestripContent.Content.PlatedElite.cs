// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// GrooveSaladSpikestripContent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// GrooveSaladSpikestripContent.Content.PlatedElite
using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using GrooveSaladSpikestripContent.Content;
using On.RoR2;
using On.RoR2.UI;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class PlatedElite : SpikestripEliteBase<PlatedElite>
{
	public class PlatedAffixBuffBehaviour : RoR2.CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver, IOnTakeDamageServerReceiver
	{
		public List<Image> platingImages = new List<Image>();

		private float lastRecordedHealthServer;

		private float lastRecordedShieldServer;

		private int lastRecordedPlatingCountServer;

		public int remainingPlatingCountSynced;

		public int maxPlatingCount => Mathf.Min(Math.Max(1, (int)(base.body.bestFitRadius * 3f)), 12);

		public float platingDefenseFraction => 1f / (float)this.maxPlatingCount;

		public float combinedHealthFraction => base.body.healthComponent ? ((base.body.healthComponent.shield + base.body.healthComponent.health) / base.body.healthComponent.fullCombinedHealth) : 0f;

		public int currentPlatingCount => Mathf.Min(Mathf.CeilToInt((float)this.maxPlatingCount * this.combinedHealthFraction), this.remainingPlatingCountSynced);

		public void Start()
		{
			if (NetworkServer.active)
			{
				this.SetRemainingPlatingCount(this.maxPlatingCount);
			}
			if ((bool)base.body.healthComponent)
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref base.body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				SpikestripContentBase.AppendArrayIfMissing(ref onIncomingDamageReceivers, in value);
				ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref base.body.healthComponent.onTakeDamageReceivers;
				IOnTakeDamageServerReceiver value2 = this;
				SpikestripContentBase.AppendArrayIfMissing(ref onTakeDamageReceivers, in value2);
			}
		}

		public void OnDestroy()
		{
			if ((bool)base.body && (bool)base.body.healthComponent)
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref base.body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				SpikestripContentBase.RemoveFromArray(ref onIncomingDamageReceivers, in value);
				ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref base.body.healthComponent.onTakeDamageReceivers;
				IOnTakeDamageServerReceiver value2 = this;
				SpikestripContentBase.RemoveFromArray(ref onTakeDamageReceivers, in value2);
			}
			for (int num = this.platingImages.Count - 1; num >= 0; num--)
			{
				if ((bool)this.platingImages[num])
				{
					UnityEngine.Object.Destroy(this.platingImages[num].gameObject);
				}
				this.platingImages.RemoveAt(num);
			}
		}

		public void OnIncomingDamageServer(RoR2.DamageInfo damageInfo)
		{
			this.lastRecordedHealthServer = base.body.healthComponent.health;
			this.lastRecordedShieldServer = base.body.healthComponent.shield;
			this.lastRecordedPlatingCountServer = this.currentPlatingCount;
		}

		public void OnTakeDamageServer(RoR2.DamageReport damageReport)
		{
			if (this.currentPlatingCount < this.lastRecordedPlatingCountServer && this.currentPlatingCount != 0)
			{
				float num = ((float)(this.lastRecordedPlatingCountServer - 1) * this.platingDefenseFraction - this.combinedHealthFraction) * base.body.healthComponent.fullCombinedHealth;
				float a = this.lastRecordedHealthServer - base.body.healthComponent.Networkhealth;
				float a2 = this.lastRecordedShieldServer - base.body.healthComponent.Networkshield;
				damageReport.damageDealt -= num;
				float num2 = Math.Max(Mathf.Min(a, num), 0f);
				num -= num2;
				base.body.healthComponent.Networkhealth += num2;
				if (num > 0f)
				{
					float num3 = Math.Max(Mathf.Min(a2, num), 0f);
					base.body.healthComponent.Networkshield += num3;
				}
				base.body.healthComponent.ospTimer = 0.5f;
				this.SetRemainingPlatingCount(this.remainingPlatingCountSynced - 1);
				RoR2.DamageInfo damageInfo = damageReport.damageInfo;
				RoR2.EffectManager.SpawnEffect(effectData: new RoR2.EffectData
				{
					origin = damageInfo.position,
					rotation = RoR2.Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
				}, effectPrefab: PlatedElite.platedBlockEffectPrefab, transmit: true);
			}
		}

		public void UpdateHealthbarPlates(RoR2.UI.HealthBar healthBar)
		{
			if (!healthBar.source)
			{
				return;
			}
			this.AllocateImages(healthBar, this.currentPlatingCount);
			for (int i = 0; i < this.platingImages.Count; i++)
			{
				if ((bool)this.platingImages[i])
				{
					Image image = this.platingImages[i];
					image.type = healthBar.barInfoCollection.cullBarInfo.imageType;
					image.sprite = healthBar.barInfoCollection.cullBarInfo.sprite;
					image.color = ((i % 2 == 0) ? PlatedElite.healthBarColorTan : PlatedElite.healthBarColorGray);
					float num = (float)i * this.platingDefenseFraction;
					PlatedAffixBuffBehaviour.SetRectPosition(xMax: Mathf.Min(num + this.platingDefenseFraction, this.combinedHealthFraction), rectTransform: (RectTransform)image.transform, xMin: num, sizeDelta: 1f);
				}
			}
		}

		public void AllocateImages(RoR2.UI.HealthBar healthBar, int desiredCount)
		{
			if (desiredCount < 0 || !healthBar.barAllocator.containerTransform.gameObject.scene.IsValid())
			{
				return;
			}
			for (int num = this.platingImages.Count - 1; num >= 0; num--)
			{
				if (num >= desiredCount && (bool)this.platingImages[num])
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(this.platingImages[num].gameObject);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(this.platingImages[num].gameObject);
					}
					this.platingImages.RemoveAt(num);
				}
				else if (!this.platingImages[num])
				{
					this.platingImages.RemoveAt(num);
				}
			}
			for (int i = this.platingImages.Count; i < desiredCount; i++)
			{
				Image component = UnityEngine.Object.Instantiate(healthBar.barAllocator.elementPrefab, healthBar.barAllocator.containerTransform).GetComponent<Image>();
				this.platingImages.Add(component);
				GameObject gameObject = component.gameObject;
				if (healthBar.barAllocator.markElementsUnsavable)
				{
					gameObject.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
				}
				gameObject.SetActive(value: true);
			}
		}

		public static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
		{
			rectTransform.anchorMin = new Vector2(xMin, 0f);
			rectTransform.anchorMax = new Vector2(xMax, 1f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
		}

		public void SetRemainingPlatingCount(int count)
		{
			this.remainingPlatingCountSynced = count;
			NetworkIdentity component = base.body.GetComponent<NetworkIdentity>();
			if ((bool)component)
			{
				new SyncRemaingPlatingCount(this.remainingPlatingCountSynced, component.netId).Send(NetworkDestination.Clients);
			}
		}
	}

	public class SyncRemaingPlatingCount : INetMessage, ISerializableObject
	{
		private int NewCount;

		private NetworkInstanceId GameObjectID;

		public SyncRemaingPlatingCount()
		{
		}

		public SyncRemaingPlatingCount(int newCount, NetworkInstanceId gameObjectID)
		{
			this.NewCount = newCount;
			this.GameObjectID = gameObjectID;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(this.NewCount);
			writer.Write(this.GameObjectID);
		}

		public void Deserialize(NetworkReader reader)
		{
			this.NewCount = reader.ReadInt32();
			this.GameObjectID = reader.ReadNetworkId();
		}

		public void OnReceived()
		{
			if (NetworkServer.active)
			{
				return;
			}
			GameObject gameObject = RoR2.Util.FindNetworkObject(this.GameObjectID);
			if ((bool)gameObject)
			{
				PlatedAffixBuffBehaviour component = gameObject.GetComponent<PlatedAffixBuffBehaviour>();
				if ((bool)component)
				{
					component.remainingPlatingCountSynced = this.NewCount;
				}
			}
		}
	}

	public static RoR2.BuffDef damageReductionBuff;

	public static GameObject zeroDamageEffectPrefab;

	public static GameObject platedBlockEffectPrefab;

	public static GameObject damageReductionVFXPrefab;

	public static Color healthBarColorGray = SpikestripContentBase.ColorRGB(119f, 113f, 97f);

	public static Color healthBarColorTan = SpikestripContentBase.ColorRGB(224f, 206f, 161f);

	public static GameObject ShieldDisplay;

	public override string EliteName => "Plated";

	public override Color EliteColor => SpikestripContentBase.ColorRGB(155f, 144f, 122f);

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<PlatedElite>.tierOneEliteDefault;

	public override EliteTierDefinition[] ExtraEliteTierDefitions => new EliteTierDefinition[1] { SpikestripEliteBase<PlatedElite>.honorEliteDefault };

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixPlated.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixPlatedIcon.png");

	public override string EquipmentName => "Alloy of Subservience";

	public override string AffixDescriptionMainWord => "endurance";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampElitePlated.png");

	public override Type AffixBuffBehaviour => typeof(PlatedAffixBuffBehaviour);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override void Init()
	{
		base.Init();
		NetworkingAPI.RegisterMessageType<SyncRemaingPlatingCount>();
		On.RoR2.UI.HealthBar.UpdateHealthbar += HealthBar_UpdateHealthbar;
		On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		PlatedElite.damageReductionBuff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		PlatedElite.damageReductionBuff.name = "DamageReductionBuff";
		PlatedElite.damageReductionBuff.buffColor = this.EliteColor;
		PlatedElite.damageReductionBuff.canStack = true;
		PlatedElite.damageReductionBuff.iconSprite = Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffDamageReduction.png");
		PlatedElite.damageReductionBuff.isDebuff = true;
		PlatedElite.damageReductionBuff.startSfx = LegacyResourcesAPI.Load<RoR2.NetworkSoundEventDef>("networksoundeventdefs/nsePulverizeBuildupBuffApplied");
		SpikestripContentBase.buffDefContent.Add(PlatedElite.damageReductionBuff);
		PlatedElite.zeroDamageEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc").InstantiateClone("ZeroDamageEffect", registerNetwork: false);
		UnityEngine.Object.Destroy(PlatedElite.zeroDamageEffectPrefab.transform.Find("Fluff").gameObject);
		PlatedElite.zeroDamageEffectPrefab.GetComponent<RoR2.EffectComponent>().soundName = "";
		PlatedElite.zeroDamageEffectPrefab.GetComponent<RoR2.VFXAttributes>().vfxPriority = RoR2.VFXAttributes.VFXPriority.Always;
		TextMeshPro componentInChildren = PlatedElite.zeroDamageEffectPrefab.GetComponentInChildren<TextMeshPro>();
		componentInChildren.color = Color.gray;
		componentInChildren.text = "INFO_ZERO_DAMAGE";
		PlatedElite.zeroDamageEffectPrefab.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>().token = "INFO_ZERO_DAMAGE";
		PlatedElite.zeroDamageEffectPrefab.transform.localScale = Vector3.one * 1.5f;
		SpikestripContentBase.effectDefContent.Add(new RoR2.EffectDef(PlatedElite.zeroDamageEffectPrefab));
		LanguageAPI.Add("INFO_ZERO_DAMAGE", "0!");
		Color color = SpikestripContentBase.ColorRGB(181f, 159f, 105f);
		PlatedElite.platedBlockEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/CaptainBodyArmorBlockEffect").InstantiateClone("PlatingBlockEffect", registerNetwork: false);
		Renderer[] componentsInChildren = PlatedElite.platedBlockEffectPrefab.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.gameObject.activeSelf)
			{
				Material material = UnityEngine.Object.Instantiate(renderer.material);
				material.SetColor("_TintColor", color);
				renderer.material = material;
			}
		}
		PlatedElite.platedBlockEffectPrefab.GetComponentInChildren<TextMeshPro>().color = color;
		PlatedElite.platedBlockEffectPrefab.GetComponent<RoR2.EffectComponent>().soundName = "Play_item_proc_armorReduction_shatter";
		SpikestripContentBase.effectDefContent.Add(new RoR2.EffectDef(PlatedElite.platedBlockEffectPrefab));
		PostProcessProfile postProcessProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
		Vignette vignette = postProcessProfile.AddSettings<Vignette>();
		vignette.mode.Override(VignetteMode.Classic);
		vignette.color.Override(SpikestripContentBase.ColorRGB(255f, 207f, 156f));
		vignette.intensity.Override(0.2f);
		vignette.smoothness.Override(1f);
		PlatedElite.damageReductionVFXPrefab = SpikestripContentBase.LegacyLoad<GameObject>("Prefabs/TemporaryVisualEffects/HealingDisabledEffect").InstantiateClone("DamageReductionEffect", registerNetwork: false);
		SpikestripContentBase.DestroyImmediate(PlatedElite.damageReductionVFXPrefab.transform.Find("Visual").gameObject);
		PlatedElite.damageReductionVFXPrefab.GetComponentInChildren<PostProcessVolume>().sharedProfile = postProcessProfile;
		PlatedElite.damageReductionVFXPrefab.GetComponentInChildren<RoR2.PostProcessDuration>().maxDuration = 0.4f;
		SpikestripVisuals.RegisterTemporaryVFX(PlatedElite.damageReductionVFXPrefab, (RoR2.CharacterBody body) => body.radius, (RoR2.CharacterBody body) => body.HasBuff(PlatedElite.damageReductionBuff));
	}

	private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
	{
		RoR2.CharacterBody characterBody = (damageInfo.attacker ? damageInfo.attacker.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)characterBody && characterBody.HasBuff(PlatedElite.damageReductionBuff))
		{
			damageInfo.damage -= characterBody.damage * (float)characterBody.GetBuffCount(PlatedElite.damageReductionBuff);
			if (damageInfo.damage <= 0f)
			{
				damageInfo.rejected = true;
				if (RoR2.SettingsConVars.enableDamageNumbers.value)
				{
					RoR2.EffectManager.SpawnEffect(effectData: new RoR2.EffectData
					{
						origin = damageInfo.position,
						rotation = RoR2.Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
					}, effectPrefab: PlatedElite.zeroDamageEffectPrefab, transmit: true);
				}
			}
		}
		orig(self, damageInfo);
	}

	public override void OnHitEnemyServer(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
		RoR2.CharacterBody characterBody = (victim ? victim.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)component && component.HasBuff(base.AffixBuff) && (bool)characterBody)
		{
			characterBody.AddTimedBuff(PlatedElite.damageReductionBuff, 8f * damageInfo.procCoefficient);
		}
	}

	private void HealthBar_UpdateHealthbar(On.RoR2.UI.HealthBar.orig_UpdateHealthbar orig, RoR2.UI.HealthBar self, float deltaTime)
	{
		orig(self, deltaTime);
		if ((bool)self.source)
		{
			PlatedAffixBuffBehaviour component = self.source.GetComponent<PlatedAffixBuffBehaviour>();
			if ((bool)component)
			{
				component.UpdateHealthbarPlates(self);
			}
		}
	}

	public override void AssignEquipmentValues()
	{
		base.EquipmentPickupModel = base.CreateAffixModel(SpikestripContentBase.ColorRGB(114f, 99f, 61f));
		PlatedElite.ShieldDisplay = LegacyResourcesAPI.Load<GameObject>("prefabs/temporaryvisualeffects/ElephantDefense").InstantiateClone("PlatedShieldDisplay", registerNetwork: false);
		SpikestripContentBase.DestroyImmediate(PlatedElite.ShieldDisplay.GetComponent<RoR2.TemporaryVisualEffect>());
		SpikestripContentBase.DestroyImmediate(PlatedElite.ShieldDisplay.GetComponentInChildren<RoR2.VFXAttributes>());
		SpikestripContentBase.DestroyImmediate(PlatedElite.ShieldDisplay.GetComponentInChildren<RoR2.ObjectScaleCurve>());
		SpikestripContentBase.DestroyImmediate(PlatedElite.ShieldDisplay.GetComponentInChildren<RoR2.ObjectScaleCurve>());
		PlatedElite.ShieldDisplay.GetComponentInChildren<RotateObject>().rotationSpeed = new Vector3(0f, 40f, 0f);
		Color color = SpikestripContentBase.ColorRGB(144f, 110f, 75f);
		Color color2 = SpikestripContentBase.ColorRGB(109f, 86f, 68f);
		MeshRenderer[] componentsInChildren = PlatedElite.ShieldDisplay.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshRenderer meshRenderer = componentsInChildren[i];
			Material material = meshRenderer.material;
			material.color = ((i % 2 == 0) ? color2 : color);
			material.DisableKeyword("FRESNEL_EMISSION");
			meshRenderer.material = material;
			meshRenderer.gameObject.transform.localScale = Vector3.one * 0.6f;
		}
		Transform transform = PlatedElite.ShieldDisplay.transform.Find("MeshHolder");
		transform.localScale = Vector3.one * 3f;
		transform.rotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.down);
		transform.localPosition = new Vector3(0f, 0f, 0f);
		SpikestripContentBase.DestroyImmediate(transform.Find("Glow").gameObject);
		SpikestripContentBase.DestroyImmediate(transform.Find("Bright Glow").gameObject);
		PlatedElite.ShieldDisplay.AddComponent<RoR2.ItemDisplay>().rendererInfos = SpikestripContentBase.GenerateRendererInfos(PlatedElite.ShieldDisplay);
		base.EquipmentDisplayModel = Base.GroovyAssetBundle.LoadAsset<GameObject>("ElitePlatingDisplay.prefab");
		base.EquipmentDisplayModel.GetComponentInChildren<MeshRenderer>().material = PlatedElite.ShieldDisplay.GetComponentInChildren<MeshRenderer>().material;
		Transform transform2 = base.EquipmentDisplayModel.transform.Find("mdlElitePlating");
		transform2.localScale = Vector3.one * 1f;
		transform2.rotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.forward);
		transform2.localPosition = new Vector3(0f, -1f, 0f);
		base.EquipmentDisplayModel.AddComponent<RoR2.ItemDisplay>().rendererInfos = SpikestripContentBase.GenerateRendererInfos(base.EquipmentDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		base.RegisterDisplayParent(LegacyResourcesAPI.Load<RoR2.EquipmentDef>("EquipmentDefs/AffixWhite"), PlatedElite.ShieldDisplay);
	}
}
