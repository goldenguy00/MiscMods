// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// RisingTides, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// RisingTides.Equipment.AffixBarrierEquipment
using System;
using System.Collections.Generic;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using RisingTides;
using RisingTides.Equipment;
using RoR2;
using UnityEngine;

public class AffixBarrierEquipment : BaseEliteAffix
{
	public static ConfigurableValue<float> barrierRecharge = ConfigurableValue.CreateFloat("com.themysticsword.risingtides", "Rising Tides", RisingTidesPlugin.config, "Elites: Bismuth", "On Use Barrier Recharge", 25f, 0f, 100f, "How much barrier should this elite aspect's on-use effect regen? (in %)", (List<string>)null, RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry, false, (Action<float>)null);

	public static GameObject selfBuffUseEffect;

	public override void OnLoad()
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		base.OnLoad();
		((BaseEquipment)this).equipmentDef.name = "RisingTides_AffixBarrier";
		((BaseEquipment)this).equipmentDef.cooldown = 30f;
		((BaseEquipment)this).equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierEquipmentIcon.png");
		base.SetUpPickupModel();
		base.AdjustElitePickupMaterial(Color.white, 4f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Barrier/texRampAffixBarrierEquipment.png"));
		((BaseItemLike)this).itemDisplayPrefab = ((BaseItemLike)this).PrepareItemDisplayModel(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/AffixBarrierShieldDisplay.prefab").InstantiateClone("RisingTidesAffixBarrierShieldDisplay", registerNetwork: false));
		Renderer[] componentsInChildren = ((BaseItemLike)this).itemDisplayPrefab.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material sharedMaterial = componentsInChildren[i].sharedMaterial;
			sharedMaterial.SetTexture("_EmTex", sharedMaterial.GetTexture("_EmissionMap"));
			sharedMaterial.SetColor("_EmColor", sharedMaterial.GetColor("_EmissionColor"));
			sharedMaterial.SetFloat("_EmPower", 2f);
		}
		((BaseItemLike)this).itemDisplayPrefabs["halo"] = ((BaseItemLike)this).PrepareItemDisplayModel(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/AffixBarrierHeadpiece.prefab").InstantiateClone("RisingTidesAffixBarrierHeadpiece", registerNetwork: false));
		BaseItemLike.onSetupIDRS += delegate
		{
			((BaseItemLike)this).AddDisplayRule("CommandoBody", "LowerArmL", new Vector3(-0.0146f, 0.10142f, -0.09926f), new Vector3(80.81804f, 73.87079f, 352.0796f), new Vector3(0.09507f, 0.09507f, 0.09507f));
			((BaseItemLike)this).AddDisplayRule("HuntressBody", "LowerArmL", new Vector3(0.00453f, 0.18608f, -0.0794f), new Vector3(74.06459f, 247.7539f, 163.7938f), new Vector3(0.07896f, 0.07896f, 0.07896f));
			((BaseItemLike)this).AddDisplayRule("Bandit2Body", "LowerArmL", new Vector3(0.07384f, 0.02959f, -0.0921f), new Vector3(54.54504f, 309.9253f, 201.8698f), new Vector3(0.09507f, 0.09507f, 0.09507f));
			((BaseItemLike)this).AddDisplayRule("ToolbotBody", "LowerArmR", new Vector3(-0.05586f, 1.84315f, 0.81928f), new Vector3(282.2477f, 259.3919f, 1.76163f), new Vector3(1.04621f, 1.04621f, 1.04621f));
			((BaseItemLike)this).AddDisplayRule("EngiBody", "LowerArmR", new Vector3(-0.02242f, 0.11966f, -0.07493f), new Vector3(343.2251f, 261.9322f, 178.5142f), new Vector3(0.109f, 0.109f, 0.109f));
			((BaseItemLike)this).AddDisplayRule("EngiTurretBody", "Head", new Vector3(0f, 0.68505f, 0f), new Vector3(0f, 270f, 90f), new Vector3(0.29954f, 0.29954f, 0.29954f));
			((BaseItemLike)this).AddDisplayRule("EngiWalkerTurretBody", "Head", new Vector3(0f, 1.41667f, -0.21911f), new Vector3(0f, 270f, 90f), new Vector3(0.31666f, 0.31666f, 0.31666f));
			((BaseItemLike)this).AddDisplayRule("MageBody", "LowerArmR", new Vector3(-0.05743f, 0.16793f, 0.08284f), new Vector3(282.7379f, 277.4919f, 344.0832f), new Vector3(0.09507f, 0.09507f, 0.09507f));
			((BaseItemLike)this).AddDisplayRule("MercBody", "LowerArmR", new Vector3(-0.02357f, 0.29074f, -0.12531f), new Vector3(283.4819f, 272.5186f, 168.9351f), new Vector3(0.09507f, 0.09507f, 0.09507f));
			((BaseItemLike)this).AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0f, 1.92666f, -0.97747f), new Vector3(-8E-05f, 89.99995f, 338.9332f), new Vector3(0.23693f, 0.23693f, 0.23693f));
			((BaseItemLike)this).AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(-0.95428f, 1.92644f, -0.15987f), new Vector3(-7E-05f, 170.9057f, 338.9332f), new Vector3(0.23693f, 0.23693f, 0.23693f));
			((BaseItemLike)this).AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0.12647f, 1.82997f, 0.83765f), new Vector3(-0.00015f, 277.5678f, 338.9332f), new Vector3(0.23693f, 0.23693f, 0.23693f));
			((BaseItemLike)this).AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0.91269f, 1.92139f, -0.00786f), new Vector3(-0.00022f, 1.56418f, 338.9333f), new Vector3(0.23693f, 0.23693f, 0.23693f));
			((BaseItemLike)this).AddDisplayRule("LoaderBody", "MechBase", new Vector3(-0.0029f, 0.09235f, 0.4434f), new Vector3(0f, 270f, 0f), new Vector3(0.11442f, 0.11442f, 0.11442f));
			((BaseItemLike)this).AddDisplayRule("CrocoBody", "UpperArmL", new Vector3(-1.53705f, -0.49132f, 0.35113f), new Vector3(19.10361f, 27.51384f, 188.1016f), new Vector3(0.66791f, 0.66791f, 0.66791f));
			((BaseItemLike)this).AddDisplayRule("CrocoBody", "UpperArmR", new Vector3(1.30091f, 0.7689f, 0.42121f), new Vector3(341.9067f, 153.8942f, 172.765f), new Vector3(0.66791f, 0.66791f, 0.66791f));
			((BaseItemLike)this).AddDisplayRule("CaptainBody", "ClavicleL", new Vector3(0.07978f, 0.17095f, -0.1458f), new Vector3(344.9297f, 240.0269f, 175.6339f), new Vector3(0.08304f, 0.08304f, 0.08304f));
			((BaseItemLike)this).AddDisplayRule("CaptainBody", "ClavicleR", new Vector3(-0.03803f, 0.18421f, -0.14963f), new Vector3(6.35989f, 291.6088f, 176.4272f), new Vector3(0.08304f, 0.08304f, 0.08304f));
			((BaseItemLike)this).AddDisplayRule("WispBody", "Head", new Vector3(0f, 0.21494f, 0.57267f), new Vector3(0f, 90.00002f, 112.6696f), new Vector3(0.28338f, 0.28338f, 0.28338f));
			((BaseItemLike)this).AddDisplayRule("JellyfishBody", "Hull2", new Vector3(-0.66769f, 0.5537f, -0.38737f), new Vector3(352.1283f, 330.5081f, 147.708f), new Vector3(0.27148f, 0.27148f, 0.27148f));
			((BaseItemLike)this).AddDisplayRule("JellyfishBody", "Hull2", new Vector3(0.74791f, 0.19783f, -0.53016f), new Vector3(354.2131f, 220.0504f, 167.8046f), new Vector3(0.27148f, 0.27148f, 0.27148f));
			((BaseItemLike)this).AddDisplayRule("JellyfishBody", "Hull2", new Vector3(-0.08774f, -0.09304f, 1.00823f), new Vector3(355.0139f, 86.40502f, 178.2649f), new Vector3(0.27148f, 0.27148f, 0.27148f));
			((BaseItemLike)this).AddDisplayRule("BeetleBody", "Head", new Vector3(0f, 0.34403f, 0.37159f), new Vector3(0f, 90f, 112.8398f), new Vector3(0.25041f, 0.25041f, 0.25041f));
			((BaseItemLike)this).AddDisplayRule("BeetleBody", "Chest", new Vector3(0f, 0.19652f, -0.61841f), new Vector3(0f, 90f, 2.51592f), new Vector3(0.25041f, 0.25041f, 0.25041f));
			((BaseItemLike)this).AddDisplayRule("LemurianBody", "LowerArmL", new Vector3(-0.28231f, -1.43541f, -0.90572f), new Vector3(5.52779f, 286.69f, 183.0832f), new Vector3(1.5576f, 1.5576f, 1.5576f));
			((BaseItemLike)this).AddDisplayRule("HermitCrabBody", "Base", new Vector3(-0.35249f, 0.58269f, 0.35845f), new Vector3(358.8646f, 225.772f, 31.04556f), new Vector3(0.17834f, 0.17834f, 0.17834f));
			((BaseItemLike)this).AddDisplayRule("HermitCrabBody", "Base", new Vector3(0.00685f, 0.55719f, -0.51574f), new Vector3(344.942f, 83.88705f, 25.03429f), new Vector3(0.17834f, 0.17834f, 0.17834f));
			((BaseItemLike)this).AddDisplayRule("ImpBody", "LowerArmL", new Vector3(0.06972f, 0.11975f, -0.03627f), new Vector3(23.06966f, 280.1783f, 174.6163f), new Vector3(0.12856f, 0.12856f, 0.12856f));
			((BaseItemLike)this).AddDisplayRule("VultureBody", "LowerArmL", new Vector3(0.99875f, 1.2198f, -0.07211f), new Vector3(346.7694f, 18.99189f, 353.5514f), new Vector3(1.6618f, 1.6618f, 1.6618f));
			((BaseItemLike)this).AddDisplayRule("RoboBallMiniBody", "ROOT", new Vector3(0.81676f, 0.00111f, -0.02604f), new Vector3(0f, 0f, 0f), new Vector3(0.26827f, 0.26827f, 0.26827f));
			((BaseItemLike)this).AddDisplayRule("RoboBallMiniBody", "ROOT", new Vector3(-0.84845f, -0.00118f, -0.01821f), new Vector3(0f, 180f, 0f), new Vector3(0.26827f, 0.26827f, 0.26827f));
			((BaseItemLike)this).AddDisplayRule("MiniMushroomBody", "Head", new Vector3(-0.11137f, -0.70176f, -0.00532f), new Vector3(2.31515f, 179.125f, 332.3143f), new Vector3(0.27659f, 0.27216f, 0.27659f));
			((BaseItemLike)this).AddDisplayRule("BellBody", "Chain", new Vector3(-1.54561f, 2.68301f, -0.88636f), new Vector3(359.0105f, 327.1622f, 186.3831f), new Vector3(0.51058f, 0.51058f, 0.51058f));
			((BaseItemLike)this).AddDisplayRule("BeetleGuardBody", "UpperArmL", new Vector3(0.70768f, 0.08173f, -0.65703f), new Vector3(296.3565f, 38.50573f, 359.3476f), new Vector3(0.53579f, 0.53579f, 0.53579f));
			((BaseItemLike)this).AddDisplayRule("BeetleGuardBody", "UpperArmR", new Vector3(-0.46761f, -0.5243f, -0.96396f), new Vector3(66.01485f, 102.1822f, 316.7295f), new Vector3(0.53579f, 0.53579f, 0.53579f));
			((BaseItemLike)this).AddDisplayRule("BeetleGuardBody", "Chest", new Vector3(-0.04737f, 0.46906f, -2.27501f), new Vector3(0f, 90f, 338.564f), new Vector3(0.69074f, 0.69074f, 0.69074f));
			((BaseItemLike)this).AddDisplayRule("BisonBody", "Head", new Vector3(-0.01011f, 0.11968f, 0.81293f), new Vector3(359.9691f, 90.42268f, 96.09669f), new Vector3(0.17392f, 0.17392f, 0.17392f));
			((BaseItemLike)this).AddDisplayRule("BisonBody", "Chest", new Vector3(-0.72052f, 0.35763f, 0.20302f), new Vector3(289.3813f, 2.77055f, 216.4511f), new Vector3(0.17392f, 0.17392f, 0.17392f));
			((BaseItemLike)this).AddDisplayRule("BisonBody", "Chest", new Vector3(0.67709f, 0.41002f, 0.23429f), new Vector3(58.92508f, 166.8929f, 200.6828f), new Vector3(0.17392f, 0.17392f, 0.17392f));
			((BaseItemLike)this).AddDisplayRule("GolemBody", "Chest", new Vector3(0f, 0.23156f, 0.5453f), new Vector3(0f, 270f, 2.80391f), new Vector3(0.42934f, 0.42934f, 0.42934f));
			((BaseItemLike)this).AddDisplayRule("ParentBody", "Chest", new Vector3(110.5298f, -126.4879f, -0.57111f), new Vector3(0.00012f, 4E-05f, 0.62143f), new Vector3(35.59922f, 35.59922f, 35.59922f));
			((BaseItemLike)this).AddDisplayRule("ClayBruiserBody", "Muzzle", new Vector3(0.01772f, -0.96556f, -0.22935f), new Vector3(0.2352f, 273.3438f, 358.8563f), new Vector3(0.17276f, 0.17276f, 0.17276f));
			((BaseItemLike)this).AddDisplayRule("ClayBruiserBody", "UpperArmL", new Vector3(-0.13775f, 0.27827f, -0.23297f), new Vector3(77.3063f, 174.8385f, 82.23293f), new Vector3(0.18463f, 0.18463f, 0.18463f));
			((BaseItemLike)this).AddDisplayRule("GreaterWispBody", "MaskBase", new Vector3(0.00844f, 0.86168f, 0.53122f), new Vector3(0.85633f, 266.8001f, 29.95748f), new Vector3(0.21107f, 0.21107f, 0.21107f));
			((BaseItemLike)this).AddDisplayRule("LemurianBruiserBody", "LowerArmL", new Vector3(-0.63009f, 3.8933f, -0.49942f), new Vector3(15.7766f, 355.9879f, 166.7924f), new Vector3(1.62279f, 1.62279f, 1.62279f));
			((BaseItemLike)this).AddDisplayRule("NullifierBody", "Muzzle", new Vector3(0f, -2.22915f, 0.72636f), new Vector3(0f, 270f, 336.7043f), new Vector3(0.60054f, 0.60054f, 0.60054f));
			((BaseItemLike)this).AddDisplayRule("BeetleQueen2Body", "Head", new Vector3(0f, 3.39523f, 0.11428f), new Vector3(0f, 90f, 60.1568f), new Vector3(0.84575f, 0.84575f, 0.84575f));
			((BaseItemLike)this).AddDisplayRule("BeetleQueen2Body", "Butt", new Vector3(1E-05f, -2.28304f, -4.30972f), new Vector3(0f, 90f, 277.3143f), new Vector3(0.99843f, 0.99843f, 0.99843f));
			((BaseItemLike)this).AddDisplayRule("ClayBossBody", "PotBase", new Vector3(1.47037f, 0.14472f, 1.15155f), new Vector3(356.2741f, 328.1895f, 8.26984f), new Vector3(0.24784f, 0.24784f, 0.24784f));
			((BaseItemLike)this).AddDisplayRule("ClayBossBody", "PotBase", new Vector3(-1.42564f, 0.14467f, 1.15156f), new Vector3(3.53829f, 214.9017f, 8.46569f), new Vector3(0.24784f, 0.24784f, 0.24784f));
			((BaseItemLike)this).AddDisplayRule("ClayBossBody", "PotBase", new Vector3(0f, 0.15405f, -1.82541f), new Vector3(0f, 90f, 14.66357f), new Vector3(0.24784f, 0.24784f, 0.24784f));
			((BaseItemLike)this).AddDisplayRule("TitanBody", "UpperArmL", new Vector3(1.86778f, 0.0468f, -0.43658f), new Vector3(354.5138f, 195.9479f, 182.7553f), new Vector3(2.25074f, 2.25074f, 2.25074f));
			((BaseItemLike)this).AddDisplayRule("TitanGoldBody", "UpperArmL", new Vector3(1.86778f, 0.0468f, -0.43658f), new Vector3(354.5138f, 195.9479f, 182.7553f), new Vector3(2.25074f, 2.25074f, 2.25074f));
			((BaseItemLike)this).AddDisplayRule("VagrantBody", "Hull", new Vector3(0f, 0.95729f, 1.16298f), new Vector3(0f, 270f, 29.51815f), new Vector3(0.24784f, 0.24784f, 0.24784f));
			((BaseItemLike)this).AddDisplayRule("VagrantBody", "Hull", new Vector3(-0.98958f, 0.95724f, -0.58474f), new Vector3(9E-05f, 152.7161f, 29.51817f), new Vector3(0.24784f, 0.24784f, 0.24784f));
			((BaseItemLike)this).AddDisplayRule("VagrantBody", "Hull", new Vector3(0.98958f, 0.95708f, -0.58468f), new Vector3(0.00012f, 36.37237f, 29.51816f), new Vector3(0.24784f, 0.24784f, 0.24784f));
			string[] array = new string[2] { "MagmaWormBody", "ElectricWormBody" };
			foreach (string text in array)
			{
				((BaseItemLike)this).AddDisplayRule(text, "Head", new Vector3(1E-05f, 0.20321f, 0.35653f), new Vector3(0f, 270f, 338.4637f), new Vector3(0.50219f, 0.50219f, 0.50219f));
				((BaseItemLike)this).AddDisplayRule(text, "Head", new Vector3(-4E-05f, 0.21744f, -1.24892f), new Vector3(0f, 90f, 350.3356f), new Vector3(0.50219f, 0.50219f, 0.50219f));
				for (int k = 1; k <= 16; k++)
				{
					Vector3 vector = Vector3.one * 0.38363f * Mathf.Pow(43f / 46f, k - 1);
					((BaseItemLike)this).AddDisplayRule(text, "Neck" + k, new Vector3(0f, 0.67666f + 0.03293f * (float)(k - 1), 0.75189f - 0.02657f * (float)(k - 1)), new Vector3(0f, 270f, 350.3946f), vector);
					((BaseItemLike)this).AddDisplayRule(text, "Neck" + k, new Vector3(0f, 0.67666f + 0.03293f * (float)(k - 1), -1.298979f + 0.02657f * (float)(k - 1)), new Vector3(0f, 90f, 350.3946f), vector);
				}
			}
			((BaseItemLike)this).AddDisplayRule("RoboBallBossBody", "MainEyeMuzzle", new Vector3(-0.54958f, 0.32175f, -0.28987f), new Vector3(297.4961f, 98.16615f, 145.5536f), new Vector3(0.1247f, 0.1247f, 0.1247f));
			((BaseItemLike)this).AddDisplayRule("RoboBallBossBody", "MainEyeMuzzle", new Vector3(0.56366f, 0.31158f, -0.30237f), new Vector3(63.15763f, 91.27869f, 163.8493f), new Vector3(0.1247f, 0.1247f, 0.1247f));
			((BaseItemLike)this).AddDisplayRule("RoboBallBossBody", "MainEyeMuzzle", new Vector3(0.0067f, -0.67212f, -0.41237f), new Vector3(0f, 270f, 321.6538f), new Vector3(0.1247f, 0.1247f, 0.1247f));
			((BaseItemLike)this).AddDisplayRule("SuperRoboBallBossBody", "MainEyeMuzzle", new Vector3(-0.54958f, 0.32175f, -0.28987f), new Vector3(297.4961f, 98.16615f, 145.5536f), new Vector3(0.1247f, 0.1247f, 0.1247f));
			((BaseItemLike)this).AddDisplayRule("SuperRoboBallBossBody", "MainEyeMuzzle", new Vector3(0.56366f, 0.31158f, -0.30237f), new Vector3(63.15763f, 91.27869f, 163.8493f), new Vector3(0.1247f, 0.1247f, 0.1247f));
			((BaseItemLike)this).AddDisplayRule("SuperRoboBallBossBody", "MainEyeMuzzle", new Vector3(0.0067f, -0.67212f, -0.41237f), new Vector3(0f, 270f, 321.6538f), new Vector3(0.1247f, 0.1247f, 0.1247f));
			((BaseItemLike)this).AddDisplayRule("GravekeeperBody", "DanglingRope4L", new Vector3(0.0001f, 1.80608f, 3E-05f), new Vector3(0f, 0f, 180f), new Vector3(0.75697f, 0.75697f, 0.75697f));
			((BaseItemLike)this).AddDisplayRule("GravekeeperBody", "DanglingRope4R", new Vector3(0.0001f, 1.80608f, 3E-05f), new Vector3(-1E-05f, 180f, 180f), new Vector3(0.75697f, 0.75697f, 0.75697f));
			((BaseItemLike)this).AddDisplayRule("ImpBossBody", "LowerArmL", new Vector3(0.24444f, 0.45194f, -0.22689f), new Vector3(0f, 270f, 174.7788f), new Vector3(0.66607f, 0.66607f, 0.66607f));
			((BaseItemLike)this).AddDisplayRule("GrandParentBody", "Head", new Vector3(0f, 8.3197f, 1E-05f), new Vector3(0f, 270f, 0f), new Vector3(1.8948f, 1.8948f, 1.8948f));
			((BaseItemLike)this).AddDisplayRule("ScavBody", "Stomach", new Vector3(0.23239f, 4.35636f, -8.43252f), new Vector3(9.37492f, 90.28594f, 11.99715f), new Vector3(1.72618f, 1.72618f, 1.72618f));
			foreach (CharacterBody allBodyPrefabBodyBodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
			{
				CharacterModel componentInChildren = allBodyPrefabBodyBodyComponent.GetComponentInChildren<CharacterModel>();
				if ((bool)componentInChildren && componentInChildren.itemDisplayRuleSet != null)
				{
					DisplayRuleGroup equipmentDisplayRuleGroup = componentInChildren.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(RoR2Content.Equipment.AffixWhite.equipmentIndex);
					if (!equipmentDisplayRuleGroup.Equals(DisplayRuleGroup.empty))
					{
						string bodyName = BodyCatalog.GetBodyName(allBodyPrefabBodyBodyComponent.bodyIndex);
						ItemDisplayRule[] rules = equipmentDisplayRuleGroup.rules;
						for (int l = 0; l < rules.Length; l++)
						{
							ItemDisplayRule itemDisplayRule = rules[l];
							((BaseItemLike)this).AddDisplayRule(bodyName, ((BaseItemLike)this).itemDisplayPrefabs["halo"], itemDisplayRule.childName, itemDisplayRule.localPos, itemDisplayRule.localAngles, itemDisplayRule.localScale);
						}
					}
				}
			}
		};
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlLemurian",
			transformLocation = "LemurianArm/ROOT/base/stomach/chest/shoulder.l/upper_arm.l/lower_arm.l",
			childName = "LowerArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlImp",
			transformLocation = "ImpArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
			childName = "LowerArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlVulture",
			transformLocation = "VultureArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
			childName = "LowerArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlBeetleGuard",
			transformLocation = "BeetleGuardArmature/ROOT/base/chest",
			childName = "Chest"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlBeetleGuard",
			transformLocation = "BeetleGuardArmature/ROOT/base/chest/upper_arm.l",
			childName = "UpperArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlBeetleGuard",
			transformLocation = "BeetleGuardArmature/ROOT/base/chest/upper_arm.r",
			childName = "UpperArmR"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlBison",
			transformLocation = "BisonArmature/ROOT/Base/stomach/chest",
			childName = "Chest"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlClayBruiser",
			transformLocation = "ClayBruiserArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l",
			childName = "UpperArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlLemurianBruiser",
			transformLocation = "LemurianBruiserArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
			childName = "LowerArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlClayBoss",
			transformLocation = "ClayBossArmature/ROOT/PotBase",
			childName = "PotBase"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlTitan",
			transformLocation = "TitanArmature/ROOT/base/stomach/chest/upper_arm.l",
			childName = "UpperArmL"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlGravekeeper",
			transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1/neck.2/head/danglingrope.1.1.l/danglingrope.1.2.l/danglingrope.1.3.l/danglingrope.1.4.l",
			childName = "DanglingRope4L"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlGravekeeper",
			transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1/neck.2/head/danglingrope.1.1.r/danglingrope.1.2.r/danglingrope.1.3.r/danglingrope.1.4.r",
			childName = "DanglingRope4R"
		});
		ChildLocatorAdditions.list.Add(new Addition
		{
			modelName = "mdlImpBoss",
			transformLocation = "ImpBossArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
			childName = "LowerArmL"
		});
		AffixBarrierEquipment.selfBuffUseEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/SelfBuffInflictVFX.prefab");
		EffectComponent effectComponent = AffixBarrierEquipment.selfBuffUseEffect.AddComponent<EffectComponent>();
		effectComponent.applyScale = true;
		effectComponent.parentToReferencedTransform = true;
		effectComponent.soundName = "Play_merc_sword_impact";
		VFXAttributes vFXAttributes = AffixBarrierEquipment.selfBuffUseEffect.AddComponent<VFXAttributes>();
		vFXAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
		vFXAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
		RisingTidesContent.Resources.effectPrefabs.Add(AffixBarrierEquipment.selfBuffUseEffect);
	}

	public override bool OnUse(EquipmentSlot equipmentSlot)
	{
		if ((bool)equipmentSlot.characterBody)
		{
			EffectData effectData = new EffectData
			{
				origin = equipmentSlot.characterBody.corePosition,
				scale = equipmentSlot.characterBody.radius
			};
			effectData.SetHurtBoxReference(equipmentSlot.characterBody.gameObject);
			EffectManager.SpawnEffect(AffixBarrierEquipment.selfBuffUseEffect, effectData, transmit: true);
			if ((bool)equipmentSlot.characterBody.healthComponent)
			{
				equipmentSlot.characterBody.healthComponent.AddBarrier(equipmentSlot.characterBody.maxBarrier * ConfigurableValue<float>.op_Implicit(AffixBarrierEquipment.barrierRecharge) / 100f);
			}
			return true;
		}
		return false;
	}

	public override void AfterContentPackLoaded()
	{
		((BaseLoadableAsset)this).AfterContentPackLoaded();
		((BaseEquipment)this).equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixBarrier;
	}
}
