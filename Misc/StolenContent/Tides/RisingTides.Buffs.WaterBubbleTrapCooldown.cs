// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Buffs.WaterBubbleTrapCooldown
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;

public class WaterBubbleTrapCooldown : BaseBuff
{
	public override void OnLoad()
	{
		((BaseLoadableAsset)this).OnLoad();
		base.buffDef.name = "RisingTides_WaterBubbleTrapCooldown";
		base.buffDef.isHidden = true;
	}
}
