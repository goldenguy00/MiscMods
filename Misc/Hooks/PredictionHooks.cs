using System;
using EntityStates;
using EntityStates.BeetleGuardMonster;
using EntityStates.Bell.BellWeapon;
using EntityStates.ClayBoss;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.GravekeeperBoss;
using EntityStates.GreaterWispMonster;
using EntityStates.ImpBossMonster;
using EntityStates.ImpMonster;
using EntityStates.LemurianBruiserMonster;
using EntityStates.LemurianMonster;
using EntityStates.RoboBallBoss.Weapon;
using EntityStates.ScavMonster;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.Vulture.Weapon;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2.Projectile;
using UnityEngine;

namespace MiscMods.Hooks
{


    /// <summary>
    /// literally just rewrote this to demonstrate how concise it could have been
    /// </summary>
    public class PredictionHooks
    {
        public static void Init()
        {
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += (orig, self, maxDist, _, filterByLoS) => orig(self, maxDist, true/*360*/, filterByLoS);

            // special cases
            IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate += (il) => FireSunder_FixedUpdate(new(il));
            // IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate += (il) => ChargeTrioBomb_FixedUpdate(new(il));

            // generic type T
            IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) => FireProjectile<GenericProjectileBaseState>(new(il));
            IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter += (il) => FireProjectile<FireCannons>(new(il));
            IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate += (il) => FireProjectile<FireEyeBlast>(new(il));
            IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += (il) => FireProjectile<FireWindblade>(new(il));
            IL.EntityStates.GravekeeperBoss.FireHook.OnEnter += (il) => FireProjectile<FireHook>(new(il));
            IL.EntityStates.LemurianMonster.FireFireball.OnEnter += (il) => FireProjectile<FireFireball>(new(il));
            IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += (il) => FireProjectile<FireMegaFireball>(new(il));
            IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter += (il) => FireProjectile<FireEnergyCannon>(new(il));
            IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade += (il) => FireProjectile<FireBombardment>(new(il));
            IL.EntityStates.ClayBoss.FireTarball.FireSingleTarball += (il) => FireProjectile<FireTarball>(new(il));
            IL.EntityStates.ImpMonster.FireSpines.FixedUpdate += (il) => FireProjectile<FireSpines>(new(il));

            //fire many
            IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate += (il) => FireProjectileGroup<JellyBarrage>(new(il));
            IL.EntityStates.ImpBossMonster.FireVoidspikes.FixedUpdate += (il) => FireProjectileGroup<FireVoidspikes>(new(il));
        }
        /// <summary>
        /// aimRay must be on the stack before calling this!
        /// </summary>
        /// <param name="c"></param>
        /// <param name="type"></param>
        private static void EmitPredictAimray(ILCursor c, Type type, string prefabName = "projectilePrefab")
        {
            //this.characterbody
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody)));

            // this.projectilePrefab
            // - or -
            // {TYPE}.projectilePrefab
            var fieldInfo = AccessTools.Field(type, prefabName);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldarg_0);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldfld, fieldInfo);
            else c.Emit(OpCodes.Ldsfld, fieldInfo);

            // Utils.PredictAimRay(aimRay, characterBody, projectilePrefab);
            c.Emit(OpCodes.Call, typeof(Utils).GetMethodCached(nameof(Utils.PredictAimray)));
        }

        #region Generics
        private static void FireProjectile<T>(ILCursor c, string prefabName)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T), prefabName);
            else Log.Error("AccurateEnemies: Generic OnEnter IL Hook failed ");
        }

        private static void FireProjectile<T>(ILCursor c)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T));
            else Log.Error("AccurateEnemies: Generic OnEnter IL Hook failed ");
        }

        private static void FireProjectileGroup<T>(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T));
        }
        #endregion

        private static void FireSunder_FixedUpdate(ILCursor c)
        {
            int loc = 0;

            if (c.TryGotoNext(x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))) &&
                c.TryGotoNext(x => x.MatchLdarg(out loc)) &&
                c.TryGotoNext(x => x.MatchCall(AccessTools.PropertyGetter(typeof(ProjectileManager), nameof(ProjectileManager.instance)))))
            {
                c.Emit(OpCodes.Ldloc, loc);
                EmitPredictAimray(c, typeof(FireSunder));
                c.Emit(OpCodes.Stloc, loc);
            }
            else Log.Error("AccurateEnemies: EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate IL Hook failed");
        }

        private static void ChargeTrioBomb_FixedUpdate(ILCursor c)
        {
            int rayLoc = 0, transformLoc = 0;

            if (c.TryGotoNext(x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))) &&
                c.TryGotoNext(x => x.MatchLdarg(out rayLoc)) &&
                c.TryGotoNext(x => x.MatchCall<ChargeTrioBomb>(nameof(ChargeTrioBomb.FindTargetChildTransformFromBombIndex))) &&
                c.TryGotoNext(x => x.MatchLdarg(out transformLoc)) &&
                c.TryGotoNext(x => x.MatchCall(AccessTools.PropertyGetter(typeof(ProjectileManager), nameof(ProjectileManager.instance)))))
            {
                // set origin
                c.Emit(OpCodes.Ldloc, rayLoc);
                c.Emit(OpCodes.Ldloc, transformLoc);
                c.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.position)));
                c.Emit(OpCodes.Call, AccessTools.PropertySetter(typeof(Ray), nameof(Ray.origin)));

                // call prediction utils
                c.Emit(OpCodes.Ldloc, rayLoc);
                EmitPredictAimray(c, typeof(ChargeTrioBomb), "bombProjectilePrefab");
                c.Emit(OpCodes.Stloc, rayLoc);
            }
            else Log.Error("AccurateEnemies: EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate IL Hook failed");
        }
    }
}
