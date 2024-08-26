// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// PlasmaCoreSpikestripContent, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlasmaCoreSpikestripContent.Content.Interactibles.CloakedShrine
using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using On.RoR2;
using PlasmaCoreSpikestripContent.Content.Interactibles;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using UnityEngine;

public class CloakedShrine : SpikestripInteractableBase<CloakedShrine>
{
	public static SSInteractibleBaseSO scriptableObject;

	public static RoR2.PickupDef hiddenPickup;

	public override string InteractableName => "Cloaked Shrine";

	public override void Init()
	{
		PlasmaCorePlugin.interactibleBaseScriptableObjects.TryGetValue(this.InteractableName, out CloakedShrine.scriptableObject);
		if ((bool)CloakedShrine.scriptableObject)
		{
			PlasmaLog.Log("Found associated ScriptableObject for InteractableBase " + this.InteractableName);
			LanguageAPI.Add("INTERACTIBLE_CLOAKEDSHRINE_NAME", "Cloaked Shrine");
			LanguageAPI.Add("INTERACTIBLE_CLOAKEDSHRINE_CONTEXT", "Offer to the Shrine");
			LanguageAPI.Add("PICKUP_HIDDENITEM", "???");
			LanguageAPI.Add("PICKUP_HIDDENITEM_CONTEXT", "Pickup");
		}
		else
		{
			PlasmaLog.LogWarn("Missing associated ScriptableObject for InteractableBase " + this.InteractableName);
		}
		base.Init();
	}

	public override void ModifySpawnCard()
	{
		base.InteractableSpawnCard = CloakedShrine.scriptableObject.InteractableSpawnCard;
		base.ModifySpawnCard();
	}

	public override void ModifyDirectorCardAndAddInteractable()
	{
		base.ModifyDirectorCardAndAddInteractable();
		base.DirectorCard.selectionWeight = CloakedShrine.scriptableObject.SelectionWeight;
		base.DirectorCard.minimumStageCompletions = CloakedShrine.scriptableObject.MinimumStageCompletion;
		base.AddInteractable(CloakedShrine.scriptableObject.interactableCategory, "");
	}

	public override void SetHooks()
	{
		RoR2.PickupCatalog.modifyPickups = (Action<List<RoR2.PickupDef>>)Delegate.Combine(RoR2.PickupCatalog.modifyPickups, new Action<List<RoR2.PickupDef>>(ModifyPickups));
		On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
	}

	private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, RoR2.GenericPickupController self, RoR2.CharacterBody body)
	{
		if (self.pickupIndex == CloakedShrine.hiddenPickup.pickupIndex)
		{
			if (RoR2.Run.instance.IsExpansionEnabled(RoR2.DLC1Content.Items.AttackSpeedAndMoveSpeed.requiredExpansion) && UnityEngine.Random.Range(0, 100) <= 15)
			{
				if (UnityEngine.Random.Range(0, 10) <= 5)
				{
					self.pickupIndex = RoR2.Run.instance.availableVoidTier1DropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableVoidTier1DropList.Count)];
				}
				else if ((double)UnityEngine.Random.Range(0, 5) <= 2.5)
				{
					self.pickupIndex = RoR2.Run.instance.availableVoidTier2DropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableVoidTier2DropList.Count)];
				}
				else if ((double)UnityEngine.Random.Range(0f, 2.5f) <= 1.8)
				{
					self.pickupIndex = RoR2.Run.instance.availableVoidBossDropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableVoidBossDropList.Count)];
				}
				else
				{
					self.pickupIndex = RoR2.Run.instance.availableVoidTier3DropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableVoidTier3DropList.Count)];
				}
			}
			else if (UnityEngine.Random.Range(0, 100) <= 25)
			{
				self.pickupIndex = RoR2.Run.instance.availableTier1DropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableTier1DropList.Count)];
			}
			else if (UnityEngine.Random.Range(0, 50) <= 15)
			{
				self.pickupIndex = RoR2.Run.instance.availableLunarItemDropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableLunarItemDropList.Count)];
			}
			else if ((double)UnityEngine.Random.Range(0, 25) <= 12.5)
			{
				self.pickupIndex = RoR2.Run.instance.availableTier2DropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableTier2DropList.Count)];
			}
			else if (UnityEngine.Random.Range(0f, 12.5f) <= 8f)
			{
				self.pickupIndex = RoR2.Run.instance.availableBossDropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableBossDropList.Count)];
			}
			else
			{
				self.pickupIndex = RoR2.Run.instance.availableTier3DropList[UnityEngine.Random.Range(0, RoR2.Run.instance.availableTier3DropList.Count)];
			}
		}
		orig(self, body);
	}

	public void ModifyPickups(List<RoR2.PickupDef> pickupDefs)
	{
		try
		{
			CloakedShrine.hiddenPickup = new RoR2.PickupDef();
			CloakedShrine.hiddenPickup.internalName = "CloakedPickup";
			CloakedShrine.hiddenPickup.coinValue = 0u;
			CloakedShrine.hiddenPickup.nameToken = "PICKUP_HIDDENITEM";
			CloakedShrine.hiddenPickup.displayPrefab = PlasmaCorePlugin.bundle.LoadAsset<GameObject>("HiddenPickup");
			CloakedShrine.hiddenPickup.dropletDisplayPrefab = SpikestripContentBase.LegacyLoad<GameObject>("Prefabs/ItemPickups/Tier1Orb");
			CloakedShrine.hiddenPickup.baseColor = RoR2.ColorCatalog.GetColor(RoR2.ColorCatalog.ColorIndex.Tier1Item);
			CloakedShrine.hiddenPickup.darkColor = RoR2.ColorCatalog.GetColor(RoR2.ColorCatalog.ColorIndex.Tier1ItemDark);
			CloakedShrine.hiddenPickup.interactContextToken = "PICKUP_HIDDENITEM_CONTEXT";
			pickupDefs.Add(CloakedShrine.hiddenPickup);
		}
		catch (Exception ex)
		{
			PlasmaLog.LogError("Error caught in " + this.InteractableName + " ModifyPickups: " + ex.ToString());
		}
	}

	public override void AssignPrefab()
	{
		base.InteractablePrefab = CloakedShrine.scriptableObject.InteractableSpawnCard.prefab;
		base.InteractablePrefab.transform.GetChild(3).GetComponent<MeshRenderer>().material = SpikestripContentBase.LegacyLoad<Material>("Materials/matCloakedEffect");
		PlasmaUtils.RegisterNetworkPrefab(CloakedShrine.scriptableObject.InteractableSpawnCard.prefab);
	}
}
