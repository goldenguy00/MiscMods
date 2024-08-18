using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EntityStates;
using HarmonyLib;
using RoR2;
using R2API.Utils;
using UnityEngine;

using SYS = System.Reflection.Emit;

using NUX = EnemiesPlus;
using HUNK = HunkMod.Modules.Components;
using RIFT = RiftTitansMod.Modules;
using RIFT_SKILLS = RiftTitansMod.SkillStates;
using DM = ThinkInvisible.Dronemeld;

namespace MiscMods.Hooks
{
    public static class HarmonyHooks
    {
        public static Harmony harm;
        public static void Init()
        {
            harm = new Harmony(MiscPlugin.PluginGUID);
            if (MiscPlugin.LeagueOfLiteralGays && !MiscPlugin.WRBInstalled)
            {
                League();
            }
            if (MiscPlugin.DronemeldInstalled)
            {
                Meld();
            }
            if (MiscPlugin.MSUInstalled)
            {
                MSU();
            }
            if (MiscPlugin.HunkInstalled)
            {
                Hunk();
            }
            if (MiscPlugin.EnemiesPlusInstalled)
            {
                EnemiesPlus();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void League()
        {
            harm.CreateClassProcessor(typeof(RaptorPrediction)).Patch();
            harm.CreateClassProcessor(typeof(RiftFlinch)).Patch();

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
            harm.CreateClassProcessor(typeof(MSUFix)).Patch();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Meld()
        {
            harm.CreateClassProcessor(typeof(DroneFixAgain)).Patch();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Hunk()
        {
            harm.CreateClassProcessor(typeof(HunkSprintFix)).Patch();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void EnemiesPlus()
        {
            harm.CreateClassProcessor(typeof(BeetlePrediction)).Patch();
            harm.CreateClassProcessor(typeof(ImpPrediction)).Patch();
        }
    }

    [HarmonyPatch(typeof(HUNK.HunkController), nameof(HUNK.HunkController.HandleSprint))]
    public class HunkSprintFix
    {
        [HarmonyPrefix]
        public static bool HandleSprint(HUNK.HunkController __instance)
        {
            var body = __instance.characterBody;
            if (body)
            {
                if (body.isSprinting)
                {
                    __instance.sprintStopwatch += Time.fixedDeltaTime;
                    if (__instance.sprintStopwatch >= 1.5f)
                    {
                        int value = Mathf.RoundToInt(Mathf.Clamp((__instance.sprintStopwatch - 1.5f) * 2, 0f, 7f));
                        __instance.sprintMultiplier = 1f + value * 0.04f;
                    }
                }
                else
                {
                    __instance.sprintStopwatch = 0f;
                    __instance.sprintMultiplier = 1f;
                }

                if (__instance.lastSprintMultiplier != __instance.sprintMultiplier)
                {
                    body.sprintingSpeedMultiplier = __instance.baseSprintValue * __instance.sprintMultiplier;
                    body.MarkAllStatsDirty();
                }

                __instance.lastSprintMultiplier = __instance.sprintMultiplier;
            }
            return false;
        }
    }

    // i swear to fucking god
    [HarmonyPatch(typeof(DM.DronemeldPlugin), nameof(DM.DronemeldPlugin.DirectorCore_TrySpawnObject))]
    public class DroneFixAgain
    {
        [HarmonyPrefix]
        public static bool DirectorCore_TrySpawnObject(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            if (directorSpawnRequest is null || !directorSpawnRequest.spawnCard || !directorSpawnRequest.spawnCard.prefab)
            {
                orig(self, directorSpawnRequest);
                return false;
            }
            return true;
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

    [HarmonyPatch(typeof(NUX.BeetleSpit), nameof(NUX.BeetleSpit.FixedUpdate))]
    public class BeetlePrediction
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
                    yield return new CodeInstruction(SYS.OpCodes.Ldsfld, AccessTools.DeclaredField(typeof(NUX.EnemiesPlus), nameof(NUX.EnemiesPlus.beetleSpit)));
                    yield return new CodeInstruction(SYS.OpCodes.Call, typeof(Utils).GetMethodCached(nameof(Utils.PredictAimray)));
                }
            }
        }
    }

    [HarmonyPatch(typeof(NUX.SpikeSlash), "HandleSlash")]
    public class ImpPrediction
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
                    yield return new CodeInstruction(SYS.OpCodes.Ldsfld, AccessTools.DeclaredField(typeof(NUX.EnemiesPlus), nameof(NUX.EnemiesPlus.voidSpike)));
                    yield return new CodeInstruction(SYS.OpCodes.Call, typeof(Utils).GetMethodCached(nameof(Utils.PredictAimray)));
                }
            }
        }
    }
}
