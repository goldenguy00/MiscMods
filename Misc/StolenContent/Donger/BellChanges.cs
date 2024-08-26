﻿using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using EntityStates;
using R2API;
using RoR2.Skills;
using RoR2.CharacterAI;

namespace MiscMods.StolenContent.Donger
{
    internal class BellChanges
    {
        internal GameObject BellMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellMaster.prefab").WaitForCompletion();
        internal GameObject BellBody => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBody.prefab").WaitForCompletion();

        public static BellChanges Instance { get; private set; }

        public static void Init() => Instance ??= new BellChanges();

        private BellChanges()
        {
            ContentAddition.AddEntityState<BuffBeamPlus>(out _);

            var buffBeam = ScriptableObject.CreateInstance<SkillDef>();
            buffBeam.skillName = "BuffBeamPlus";
            (buffBeam as ScriptableObject).name = "BuffBeamPlus";
            buffBeam.skillNameToken = "BuffBeamPlus";
            buffBeam.skillDescriptionToken = "Creates a beam to a nearby ally and makes them invincible";
            buffBeam.activationState = new SerializableEntityStateType(typeof(BuffBeamPlus));
            buffBeam.activationStateMachineName = "Weapon";
            buffBeam.interruptPriority = InterruptPriority.Death;
            buffBeam.baseMaxStock = 1;
            buffBeam.baseRechargeInterval = 45;
            buffBeam.rechargeStock = 1;
            buffBeam.requiredStock = 1;
            buffBeam.stockToConsume = 1;
            buffBeam.cancelSprintingOnActivation = false;
            buffBeam.canceledFromSprinting = false;
            buffBeam.forceSprintDuringState = false;
            buffBeam.resetCooldownTimerOnUse = false;
            buffBeam.mustKeyPress = false;
            buffBeam.dontAllowPastMaxStocks = true;
            buffBeam.beginSkillCooldownOnSkillEnd = true;
            buffBeam.fullRestockOnAssign = true;
            buffBeam.isCombatSkill = true;
            ContentAddition.AddSkillDef(buffBeam);

            var skill = BellBody.AddComponent<GenericSkill>();
            skill.skillName = "BuffBeamPlus";

            var newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = "BellBuffBeamPlusFamily";
            newFamily.variants = [new SkillFamily.Variant() { skillDef = buffBeam }];

            skill._skillFamily = newFamily;
            ContentAddition.AddSkillFamily(newFamily);

            BellBody.GetComponent<SkillLocator>().secondary = skill;

            var buffBeamDriver = BellMaster.AddComponent<AISkillDriver>();
            buffBeamDriver.customName = "BuffBeam";
            buffBeamDriver.skillSlot = SkillSlot.Secondary;
            buffBeamDriver.requireSkillReady = true;
            buffBeamDriver.requireEquipmentReady = false;
            buffBeamDriver.maxUserHealthFraction = 0.8f;
            buffBeamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;

            BellMaster.ReorderSkillDrivers(2);
        }
    }
}
