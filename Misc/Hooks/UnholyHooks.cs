using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.Stats.StatManager;

namespace MiscMods.Hooks
{
    public class UnholyHooks
    {
        internal const BindingFlags
            All = (BindingFlags)(-1),
            Default = BindingFlags.Static | BindingFlags.Instance |
                      BindingFlags.Public | BindingFlags.NonPublic,
            InvokeMethod = BindingFlags.InvokeMethod | Default,
            GetField = BindingFlags.GetField | Default,
            SetField = BindingFlags.SetField | Default;

        internal const string
            HookMap = "HookMap",
            HookList = "HookList",
            GetEndpoint = "GetEndpoint";

        public static UnholyHooks instance;

        public static void Init()
        {
            instance ??= new UnholyHooks();
        }

        public UnholyHooks()
        {
            RoR2Application.onLoad += OnLoad;
            On.RoR2.Stats.PlayerStatsComponent.Init += PlayerStatsComponent_Init;
            On.RoR2.Stats.StatManager.OnCharacterDeath += StatManager_OnCharacterDeath;
        }

        private void OnLoad()
        {
            var hookType = typeof(On.RoR2.Projectile.ProjectileController.hook_IgnoreCollisionsWithOwner);
            var orig = AccessTools.DeclaredMethod(typeof(ProjectileController), nameof(ProjectileController.IgnoreCollisionsWithOwner));
            UnsetAllHooks(hookType, orig);

            hookType = typeof(On.RoR2.BulletAttack.hook_DefaultFilterCallbackImplementation);
            orig = AccessTools.DeclaredMethod(typeof(BulletAttack), nameof(BulletAttack.DefaultFilterCallbackImplementation));
            UnsetAllHooks(hookType, orig);

            On.RoR2.BulletAttack.DefaultFilterCallbackImplementation += BulletAttack_DefaultFilterCallbackImplementation;
            On.RoR2.Projectile.ProjectileController.IgnoreCollisionsWithOwner += ProjectileController_IgnoreCollisionsWithOwner;
        }

        private void UnsetAllHooks(Type hookType, MethodInfo orig)
        {
            var hookEndpoint = typeof(HookEndpointManager).InvokeMember(GetEndpoint, InvokeMethod, null/*binder*/, null/*instance*/, [orig]);

            if (hookEndpoint.GetType().InvokeMember(HookMap, GetField, null, hookEndpoint, null)
                is Dictionary<Delegate, Stack<IDetour>> hookMap && hookMap.Any())
            {
                foreach (var hook in hookMap.Keys.Where(del => del != null && del.GetType() == hookType).ToList())
                {
                    // just in case
                    int loops = 0;
                    while (hookMap.ContainsKey(hook) && loops < 100)
                    {
                        Log.Warning($"Removing {nameof(hook)} from {HookEndpointManager.GetOwner(hook)}");

                        HookEndpointManager.Remove(orig, hook);
                        loops++;
                    }

                    if (loops >= 100)
                        Log.Error($"holy fuck youre stupid {nameof(hook)} was NOT removed {loops} times");
                }
            }
        }

        private static void StatManager_OnCharacterDeath(On.RoR2.Stats.StatManager.orig_OnCharacterDeath orig, DamageReport damageReport)
        {
            if (damageReport.victim)
                orig(damageReport);
        }


        private static void PlayerStatsComponent_Init(On.RoR2.Stats.PlayerStatsComponent.orig_Init orig)
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (NetworkServer.active && damageReport.victimMaster)
                {
                    var playerStatsComponent = damageReport.victimMaster.playerStatsComponent;
                    if (playerStatsComponent)
                    {
                        playerStatsComponent.serverTransmitTimer = 0f;
                    }
                }
            };
        }

        private static bool BulletAttack_DefaultFilterCallbackImplementation(On.RoR2.BulletAttack.orig_DefaultFilterCallbackImplementation orig, BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            return orig(bulletAttack, ref hitInfo)
                && !(hitInfo.hitHurtBox
                && hitInfo.hitHurtBox.healthComponent
                && bulletAttack.owner
                && bulletAttack.owner.TryGetComponent<TeamComponent>(out var attackerTeamComponent)
                && attackerTeamComponent.teamIndex == TeamIndex.Player
                && !FriendlyFireManager.ShouldDirectHitProceed(hitInfo.hitHurtBox.healthComponent, attackerTeamComponent.teamIndex));
        }

        private static void ProjectileController_IgnoreCollisionsWithOwner(On.RoR2.Projectile.ProjectileController.orig_IgnoreCollisionsWithOwner orig, ProjectileController self, bool shouldIgnore)
        {
            if (self.teamFilter.teamIndex == TeamIndex.Player && self.myColliders.Length != 0 && self.owner &&
                self.owner.TryGetComponent<CharacterBody>(out var ownerBody) && ownerBody.isPlayerControlled)
            {
                foreach (var tc in TeamComponent.GetTeamMembers(TeamIndex.Player))
                {
                    var body = tc.body;
                    if (body && body.healthComponent && body.hurtBoxGroup && (body == ownerBody || !FriendlyFireManager.ShouldDirectHitProceed(body.healthComponent, TeamIndex.Player)))
                    {
                        var hurtBoxes = body.hurtBoxGroup.hurtBoxes;
                        for (int i = 0; i < hurtBoxes.Length; i++)
                        {
                            for (int j = 0; j < self.myColliders.Length; j++)
                            {
                                Physics.IgnoreCollision(hurtBoxes[i].collider, self.myColliders[j], shouldIgnore);
                            } // end for
                        } // end for
                    }
                } // endforeach
            }
            else
            {
                orig(self, shouldIgnore);
                return;
            }
        }
    }
}
