// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Items.MirrorClone
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RoR2;
using UnityEngine;

public class MirrorClone : BaseItem
{
	public override void OnLoad()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		((BaseLoadableAsset)this).OnLoad();
		base.itemDef.name = "RisingTides_MirrorClone";
		((BaseItem)this).SetItemTierWhenAvailable(ItemTier.NoTier);
		base.itemDef.tags = new ItemTag[3]
		{
			ItemTag.WorldUnique,
			ItemTag.AIBlacklist,
			ItemTag.BrotherBlacklist
		};
		base.itemDef.hidden = true;
		base.itemDef.canRemove = false;
		RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
		GenericGameEvents.BeforeTakeDamage += new DamageAttackerVictimEventHandler(GenericGameEvents_BeforeTakeDamage);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if ((bool)sender.inventory && sender.inventory.GetItemCount(base.itemDef) > 0 && (bool)sender.healthComponent)
		{
			sender.healthComponent.globalDeathEventChanceCoefficient = 0f;
		}
	}

	private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)attackerInfo.inventory && attackerInfo.inventory.GetItemCount(base.itemDef) > 0)
		{
			damageInfo.damage = Mathf.Min(0.01f, damageInfo.procCoefficient);
			damageInfo.procCoefficient = Mathf.Min(0.01f, damageInfo.procCoefficient);
		}
	}
}
