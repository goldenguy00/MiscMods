using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EntityStates;
using HarmonyLib;
using RoR2;
using R2API.Utils;

using SYS = System.Reflection.Emit;

using RIFT = RiftTitansMod.Modules;
using RIFT_SKILLS = RiftTitansMod.SkillStates;
using MiscMods.Config;

namespace MiscMods.Hooks
{
    public static class HarmonyHooks
    {
        public static Harmony Patcher;

        public static void Init()
        {
            Patcher = new Harmony(MiscPlugin.PluginGUID);
            if (PluginConfig.enableEnemiesPlus.Value && MiscPlugin.LeagueOfLiteralGays && !MiscPlugin.WRBInstalled)
            {
                League();
            }
            if (PluginConfig.enableUnholy.Value && MiscPlugin.MSUInstalled)
            {
                MSU();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void League()
        {
            if (PluginConfig.enablePrediction.Value)
                Patcher.CreateClassProcessor(typeof(RaptorPrediction)).Patch();
            Patcher.CreateClassProcessor(typeof(RiftFlinch)).Patch();

            RiftTitansMod.RiftTitansPlugin.ReksaiCard.MonsterCategory = R2API.DirectorAPI.MonsterCategory.Champions;
            var reksaiCard = RiftTitansMod.RiftTitansPlugin.ReksaiCard.Card;
            reksaiCard.spawnCard.hullSize = HullClassification.BeetleQueen;
            reksaiCard.spawnCard.directorCreditCost = 600;
            reksaiCard.minimumStageCompletions = 2;
            var master = reksaiCard.spawnCard.prefab.GetComponent<CharacterMaster>();
            var aiList = master.GetComponents<RoR2.CharacterAI.AISkillDriver>();
            foreach (var ai in aiList)
            {
                switch (ai.customName)
                {
                    case "Special":
                        break;
                    case "Seeker":
                        break;
                    case "ChaseHard":
                        ai.shouldSprint = false;
                        break;
                    case "Attack":
                        ai.driverUpdateTimerOverride = 0.5f;
                        ai.nextHighPriorityOverride = aiList.Last();
                        break;
                    case "Chase":
                        break;
                }
            }
            master.bodyPrefab.GetComponent<CharacterBody>().baseMoveSpeed = 10;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void MSU()
        {
            Patcher.CreateClassProcessor(typeof(MSUFix)).Patch();
        }
    }

    // on principle i will not put a pr out to fix this. nah.
    [HarmonyPatch(typeof(Moonstorm.BuffModuleBase), nameof(Moonstorm.BuffModuleBase.OnBuffsChanged))]
    public class MSUFix
    {
        [HarmonyFinalizer]
        private static Exception Finalizer() => null;
    }


    [HarmonyPatch(typeof(RIFT_SKILLS.Blue.Slam), nameof(RIFT_SKILLS.Blue.Slam.GetMinimumInterruptPriority))]
    public class RiftFlinch
    {
        [HarmonyPostfix]
        private static void Postfix(ref InterruptPriority __result)
        {
            __result = InterruptPriority.Pain;
        }
    }

    [HarmonyPatch(typeof(RIFT_SKILLS.Chicken.Shoot), nameof(RIFT_SKILLS.Chicken.Shoot.Fire))]
    public class RaptorPrediction
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                yield return code;

                if (code.Calls(AccessTools.Method(typeof(BaseState), nameof(BaseState.GetAimRay))))
                {
                    yield return new CodeInstruction(SYS.OpCodes.Ldarg_0);
                    yield return new CodeInstruction(SYS.OpCodes.Call, AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody)));
                    yield return new CodeInstruction(SYS.OpCodes.Ldsfld, AccessTools.DeclaredField(typeof(RIFT.Projectiles), nameof(RIFT.Projectiles.chickenProjectilePrefab)));
                    yield return new CodeInstruction(SYS.OpCodes.Call, typeof(Utils).GetMethodCached(nameof(Utils.PredictAimray)));
                }
            }
        }
    }
}
