using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HG.GeneralSerializer;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MiscMods
{
    public static class Utils
    {
        private const float dt = 0.0333f;
        private const float zero = 0.00001f;

        public static bool TryModifyFieldValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName, T value)
        {
            ref var serializedField = ref entityStateConfiguration.serializedFieldsCollection.GetOrCreateField(fieldName);
            var type = typeof(T);
            if (serializedField.fieldValue.objectValue && typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                serializedField.fieldValue.objectValue = value as UnityEngine.Object;
                return true;
            }
            else if (serializedField.fieldValue.stringValue != null && StringSerializer.CanSerializeType(type))
            {
                serializedField.fieldValue.stringValue = StringSerializer.Serialize(type, value);
                return true;
            }
            return false;
        }

        public static Ray PredictAimray(Ray aimRay, CharacterBody body, GameObject projectilePrefab)
        {
            if (!body || !body.master || !projectilePrefab)
                return aimRay;

            var projectileSpeed = 0f;
            if (projectilePrefab.TryGetComponent<ProjectileSimple>(out var ps))
                projectileSpeed = ps.desiredForwardSpeed;

            if (projectilePrefab.TryGetComponent<ProjectileCharacterController>(out var pcc))
                projectileSpeed = Mathf.Max(projectileSpeed, pcc.velocity);

            var targetBody = GetAimTargetBody(body);
            if (projectileSpeed > 0f && targetBody)
            {
                //Velocity shows up as 0 for clients due to not having authority over the CharacterMotor
                //Less accurate, but it works online.
                Vector3 vT, aT, pT = targetBody.transform.position;
                if (targetBody.characterMotor && targetBody.characterMotor.hasEffectiveAuthority)
                {
                    vT = targetBody.characterMotor.velocity;
                    aT = (vT - targetBody.characterMotor.lastVelocity) / dt;
                }
                else
                {
                    vT = (pT - targetBody.previousPosition) / dt;
                    aT = Vector3.zero;
                }

                if (vT.sqrMagnitude > zero) //Dont bother predicting stationary targets
                {
                    return GetRay(aimRay, projectileSpeed, pT, vT, aT);
                }
            }

            return aimRay;
        }
        //All in world space! Gets point you have to aim to
        //NOTE: this will break with infinite speed projectiles!
        //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
        public static Ray GetRay(Ray aimRay, float sP, Vector3 pT, Vector3 vT, Vector3 aT)
        {
            //time to target guess
            float t = Vector3.Distance(aimRay.origin, pT) / sP;

            // target position relative to ray position
            pT -= aimRay.origin;

            bool useAccel = aT.sqrMagnitude > zero;

            //quartic coefficients
            // a = t^4 * (aT·aT / 4.0)
            // b = t^3 * (aT·vT)
            // c = t^2 * (aT·pT + vT·vT - s^2)
            // d = t   * (2.0 * vT·pT)
            // e =       pT·pT
            float c = Vector3.Dot(vT, vT) - Pow2(sP);
            float d = 2f * Vector3.Dot(vT, pT);
            float e = Vector3.Dot(pT, pT);

            if (useAccel)
            {
                float a = Vector3.Dot(aT, aT) * 0.25f;
                float b = Vector3.Dot(aT, vT);
                c += Vector3.Dot(aT, pT);

                //solve with newton
                t = SolveQuarticNewton(t, 6, a, b, c, d, e);
            }
            else
            {
                t = SolveQuadraticNewton(t, 6, c, d, e);
            }

            if (t > 0f)
            {
                //p(t) = pT + (vT * t) + ((aT/2.0) * t^2)
                var relativeDest = pT + (vT * t);
                if (useAccel)
                    relativeDest += 0.5f * aT * Pow2(t);
                relativeDest.Normalize();
                var diff = Mathf.Min(Vector3.Angle(aimRay.direction, relativeDest), Vector3.Angle(relativeDest, aimRay.direction));
                Log.Debug($"Accel {aT.magnitude} angle diff {diff} ");
                aimRay = new Ray(aimRay.origin, relativeDest);
            }
            return aimRay;

        }

        private static float SolveQuarticNewton(float guess, int iterations, float a, float b, float c, float d, float e)
        {
            for (int i = 0; i < iterations; i++)
            {
                guess -= EvalQuartic(guess, a, b, c, d, e) / EvalQuarticDerivative(guess, a, b, c, d);
            }
            return guess;
        }

        private static float EvalQuartic(float t, float a, float b, float c, float d, float e)
        {
            return (a * Pow4(t)) + (b * Pow3(t)) + (c * Pow2(t)) + (d * t) + e;
        }

        private static float EvalQuarticDerivative(float t, float a, float b, float c, float d)
        {
            return (4f * a * Pow3(t)) + (3f * b * Pow2(t)) + (2f * c * t) + d;
        }

        private static float SolveQuadraticNewton(float guess, int iterations, float a, float b, float c)
        {
            for (int i = 0; i < iterations; i++)
            {
                guess -= EvalQuadratic(guess, a, b, c) / EvalQuadraticDerivative(guess, a, b);
            }
            return guess;
        }

        private static float EvalQuadratic(float t, float a, float b, float c)
        {
            return (a * Pow2(t)) + (b * t) + c;
        }

        private static float EvalQuadraticDerivative(float t, float a, float b)
        {
            return (2f * a * t) + b;
        }

        private static float Pow2(float n) => n * n;
        private static float Pow3(float n) => n * n * n;
        private static float Pow4(float n) => n * n * n * n;

        private static CharacterBody GetAimTargetBody(CharacterBody body)
        {
            var aiComponents = body.master.aiComponents;
            for (int i = 0; i < aiComponents.Length; i++)
            {
                var ai = aiComponents[i];
                if (ai && ai.hasAimTarget)
                {
                    var aimTarget = ai.skillDriverEvaluation.aimTarget;
                    if (aimTarget.characterBody && aimTarget.healthComponent && aimTarget.healthComponent.alive)
                    {
                        return aimTarget.characterBody;
                    }
                }
            }
            return null;
        }

        public static bool IsValid(EliteDef ed)
        {
            return ed && ed.IsAvailable()
                && ed.eliteEquipmentDef
                && ed.eliteEquipmentDef.passiveBuffDef
                && ed.eliteEquipmentDef.passiveBuffDef.isElite
                && !StolenContent.Cruelty.BlacklistedElites.Contains(ed.eliteEquipmentDef);
        }

        public static bool GetRandom(float availableCredits, DirectorCard card, Xoroshiro128Plus rng, List<BuffIndex> exclude, out EliteDef def, out float cost)
        {
            def = null;
            cost = 0;

            var tiers = EliteAPI.GetCombatDirectorEliteTiers();
            if (tiers == null || tiers.Length == 0)
                return false;

            List<(EliteDef, float)> availableDefs = [];
            for (int j = 0; j < tiers.Length; j++)
            {
                var etd = tiers[j];
                if (etd != null && !etd.canSelectWithoutAvailableEliteDef &&
                   (card == null || etd.CanSelect(card.spawnCard.eliteRules)))
                {
                    float eliteCost = card?.cost ?? 0f;
                    bool canAfford = eliteCost > 0f && availableCredits >= eliteCost * etd.costMultiplier;

                    if (canAfford)
                    {
                        for (int i = 0; i < etd.eliteTypes.Length; i++)
                        {
                            var ed = etd.eliteTypes[i];
                            if (IsValid(ed) && !exclude.Contains(ed.eliteEquipmentDef.passiveBuffDef.buffIndex))
                                availableDefs.Add((ed, eliteCost));
                        }
                    }
                }
            }

            if (availableDefs.Any())
            {
                var d = rng.NextElementUniform(availableDefs);
                def = d.Item1;
                cost = d.Item2;
                return true;
            }
            return false;
        }

        public static void MultiplyHealth(Inventory inventory, float multAdd)
        {
            int itemCount = (int)((multAdd - 1f) * 10);
            if (itemCount > 0)
                inventory.GiveItem(RoR2Content.Items.BoostHp, itemCount);
        }

        public static void MultiplyDamage(Inventory inventory, float multAdd)
        {
            int itemCount = (int)((multAdd - 1f) * 10);
            if (itemCount > 0 && inventory)
                inventory.GiveItem(RoR2Content.Items.BoostDamage, itemCount);
        }
    }
}
