// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Equipment.BaseEliteAffix
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using UnityEngine;

public abstract class BaseEliteAffix : BaseEquipment
{
	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.equipmentDef.canDrop = false;
		base.equipmentDef.enigmaCompatible = false;
		base.equipmentDef.canBeRandomlyTriggered = false;
	}

	public void SetUpPickupModel()
	{
		base.equipmentDef.pickupModelPrefab = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Misc/GenericAffixPickup.prefab").InstantiateClone(base.equipmentDef.name + "Pickup", registerNetwork: false);
		Material material = new Material(Standard.shader);
		Standard.DisableEverything(material);
		material.name = "mat" + base.equipmentDef.pickupModelPrefab.name;
		material.EnableKeyword("FORCE_SPEC");
		material.EnableKeyword("FRESNEL_EMISSION");
		material.SetFloat("_AOON", 0f);
		material.SetFloat("_BlueChannelBias", 0f);
		material.SetFloat("_BlueChannelSmoothness", 0f);
		material.SetFloat("_BumpScale", 1f);
		material.SetFloat("_ColorsOn", 0f);
		material.SetFloat("_Cull", 2f);
		material.SetFloat("_Cutoff", 0.5f);
		material.SetFloat("_DecalLayer", 0f);
		material.SetFloat("_Depth", 0.2f);
		material.SetFloat("_DetailNormalMapScale", 1f);
		material.SetFloat("_DitherOn", 0f);
		material.SetFloat("_DstBlend", 0f);
		material.SetFloat("_EliteBrightnessMax", 1f);
		material.SetFloat("_EliteBrightnessMin", 0f);
		material.SetFloat("_EliteIndex", 0f);
		material.SetFloat("_EmPower", 0f);
		material.SetFloat("_EnableCutout", 0f);
		material.SetFloat("_FEON", 1f);
		material.SetFloat("_Fade", 1f);
		material.SetFloat("_FadeBias", 0f);
		material.SetFloat("_FadeDistance", 0f);
		material.SetFloat("_FlowDiffuseStrength", 1f);
		material.SetFloat("_FlowEmissionStrength", 1f);
		material.SetFloat("_FlowHeightBias", 0f);
		material.SetFloat("_FlowHeightPower", 1f);
		material.SetFloat("_FlowMaskStrength", 0f);
		material.SetFloat("_FlowNormalStrength", 1f);
		material.SetFloat("_FlowSpeed", 1f);
		material.SetFloat("_FlowTextureScaleFactor", 1f);
		material.SetFloat("_FlowmapOn", 0f);
		material.SetFloat("_ForceSpecOn", 1f);
		material.SetFloat("_FresnelBoost", 20f);
		material.SetFloat("_FresnelPower", 4.11f);
		material.SetFloat("_GlossMapScale", 1f);
		material.SetFloat("_Glossiness", 0.5f);
		material.SetFloat("_GlossyReflections", 1f);
		material.SetFloat("_GreenChannelBias", 0f);
		material.SetFloat("_GreenChannelSmoothness", 0f);
		material.SetFloat("_LimbPrimeMask", 1f);
		material.SetFloat("_LimbRemovalOn", 0f);
		material.SetFloat("_Metallic", 0f);
		material.SetFloat("_Mode", 0f);
		material.SetFloat("_NormalStrength", 1f);
		material.SetFloat("_OcclusionStrength", 1f);
		material.SetFloat("_Parallax", 0.02f);
		material.SetFloat("_PrintBias", 0f);
		material.SetFloat("_PrintBoost", 1f);
		material.SetFloat("_PrintDirection", 0f);
		material.SetFloat("_PrintEmissionToAlbedoLerp", 0f);
		material.SetFloat("_PrintOn", 0f);
		material.SetFloat("_RampInfo", 0f);
		material.SetFloat("_SliceAlphaDepth", 0.1f);
		material.SetFloat("_SliceBandHeight", 1f);
		material.SetFloat("_SliceHeight", 5f);
		material.SetFloat("_Smoothness", 0f);
		material.SetFloat("_SmoothnessTextureChannel", 0f);
		material.SetFloat("_SpecularExponent", 9.26f);
		material.SetFloat("_SpecularHighlights", 1f);
		material.SetFloat("_SpecularStrength", 0.258f);
		material.SetFloat("_SplatmapOn", 0f);
		material.SetFloat("_SplatmapTileScale", 1f);
		material.SetFloat("_SrcBlend", 1f);
		material.SetFloat("_UVSec", 0f);
		material.SetFloat("_ZWrite", 1f);
		Renderer[] componentsInChildren = base.equipmentDef.pickupModelPrefab.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].materials = new Material[1] { material };
		}
	}

	public void AdjustElitePickupMaterial(Color color, float fresnelPower, bool smoothFresnelRamp = true)
	{
		Material sharedMaterial = base.equipmentDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
		sharedMaterial.SetColor("_Color", color);
		sharedMaterial.SetFloat("_FresnelPower", fresnelPower);
		sharedMaterial.SetTexture("_FresnelRamp", RisingTidesPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/RisingTides/Misc/" + (smoothFresnelRamp ? "texElitePickupFresnelRampSmooth.png" : "texElitePickupFresnelRamp.png")));
	}

	public void AdjustElitePickupMaterial(Color color, float fresnelPower, Texture customFresnelRamp)
	{
		Material sharedMaterial = base.equipmentDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
		sharedMaterial.SetColor("_Color", color);
		sharedMaterial.SetFloat("_FresnelPower", fresnelPower);
		sharedMaterial.SetTexture("_FresnelRamp", customFresnelRamp);
	}

	public void AdjustElitePickupMaterial(Color color, float fresnelPower)
	{
		Material sharedMaterial = base.equipmentDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
		sharedMaterial.SetColor("_Color", color);
		sharedMaterial.SetFloat("_FresnelPower", fresnelPower);
	}
}
