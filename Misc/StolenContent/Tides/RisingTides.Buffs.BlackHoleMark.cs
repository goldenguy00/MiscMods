// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.BlackHoleMark
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BlackHoleMark : BaseBuff
{
	public override void OnLoad()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_BlackHoleMark";
		base.buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Nullifier/texBuffNullifiedIcon.tif").WaitForCompletion();
		base.buffDef.buffColor = Color.black;
		base.buffDef.isDebuff = true;
		base.buffDef.canStack = true;
		((BaseBuff)this).refreshable = true;
		for (int i = 1; i <= 6; i++)
		{
			GameObject gameObject = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/Mark/BlackHoleMarkVFX" + i + ".prefab").InstantiateClone("RisingTidesBlackHoleMarkVFX" + i, registerNetwork: false);
			gameObject.AddComponent<MysticsRisky2UtilsTempVFX>().enterObjects = new GameObject[1] { gameObject.transform.Find("Particle").gameObject };
			int buffCount = i;
			CustomTempVFXManagement.allVFX.Add(new VFXInfo
			{
				prefab = gameObject,
				condition = (CharacterBody x) => x.GetBuffCount(base.buffDef) >= buffCount,
				radius = CustomTempVFXManagement.DefaultRadiusCall
			});
		}
	}
}
