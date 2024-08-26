using System;
using System.Linq;
using System.Reflection;
using EntityStates;
using HarmonyLib;
using HG.GeneralSerializer;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace MiscMods
{
    public static class Utils
    {

        public static void DumpEntityStateConfig(EntityStateConfiguration esc)
        {

            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue)
                {
                    Debug.Log(esc.serializedFieldsCollection.serializedFields[i].fieldName + " - " + esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue);
                }
                else
                {
                    Debug.Log(esc.serializedFieldsCollection.serializedFields[i].fieldName + " - " + esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue);
                }
            }
        }
        public static void DumpEntityStateConfig(string entityStateName)
        {
            var esc = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/" + entityStateName);
            DumpEntityStateConfig(esc);
        }


        public static void DumpAddressableEntityStateConfig(string addressablePath)
        {
            var esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(addressablePath).WaitForCompletion();
            DumpEntityStateConfig(esc);
        }

        public static UnityEngine.Object GetEntityStateFieldObject(string entityStateName, string fieldName)
        {
            var esc = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/" + entityStateName);
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    return esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue;
                }
            }
            return null;
        }

        public static string GetEntityStateFieldString(string entityStateName, string fieldName)
        {
            var esc = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/" + entityStateName);
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    return esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue;
                }
            }
            return string.Empty;
        }

        public static bool SetEntityStateField(string entityStateName, string fieldName, UnityEngine.Object newObject)
        {
            var esc = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/" + entityStateName);
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue = newObject;
                    return true;
                }
            }
            return false;
        }

        public static bool SetEntityStateField(string entityStateName, string fieldName, string value)
        {
            var esc = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/" + entityStateName);
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue = value;
                    return true;
                }
            }
            return false;
        }

        public static bool SetAddressableEntityStateField(string fullEntityStatePath, string fieldName, string value)
        {
            var esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).WaitForCompletion();
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue = value;
                    return true;
                }
            }
            return false;
        }

        public static bool SetAddressableEntityStateField(string fullEntityStatePath, string fieldName, UnityEngine.Object newObject)
        {
            var esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).WaitForCompletion();
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue = newObject;
                    return true;
                }
            }
            return false;
        }

        public static UnityEngine.Object GetAddressableEntityStateFieldObject(string fullEntityStatePath, string fieldName)
        {
            var esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).WaitForCompletion();
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    return esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue;
                }
            }
            return null;
        }

        public static string GetAddressableEntityStateFieldString(string fullEntityStatePath, string fieldName)
        {
            var esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).WaitForCompletion();
            for (var i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    return esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue;
                }
            }
            return string.Empty;
        }

        public static void ReorderSkillDrivers(this GameObject master, int targetIdx)
        {
            var c = master.GetComponents<AISkillDriver>();
            master.ReorderSkillDrivers(c, c.Length - 1, targetIdx);
        }
        public static void ReorderSkillDrivers(this GameObject master, AISkillDriver targetSkill, int targetIdx)
        {
            var c = master.GetComponents<AISkillDriver>();
            master.ReorderSkillDrivers(c, Array.IndexOf(c, targetSkill), targetIdx);
        }
        public static void ReorderSkillDrivers(this GameObject master, AISkillDriver[] skills, int currentIdx, int targetIdx)
        {
            if (currentIdx < 0 || currentIdx >= skills.Length)
            {
                Log.Error($"{currentIdx} index not found or out of range. Must be less than {skills.Length}");
                return;
            }
            var targetName = skills[currentIdx].customName;

            if (targetIdx < 0 || targetIdx >= skills.Length)
            {
                Log.Error($"Unable to reorder skilldriver {targetName} into position {targetIdx}. target must be less than {skills.Length}");
                return;
            }

            if (targetIdx == currentIdx)
            {
                Log.Warning($"Skilldriver {targetName} already has the target index of {targetIdx}");
                return;
            }

            // reference to original might get nulled so they need to be re-added later
            var overrides = skills.Where(s => s.nextHighPriorityOverride != null)
                .ToDictionary(
                s => s.customName,
                s => s.nextHighPriorityOverride.customName);

            // move down. this modifies the order.
            if (targetIdx > currentIdx)
            {
                master.AddComponentCopy(skills[currentIdx]);
                Component.DestroyImmediate(skills[currentIdx]);
            }
            
            // anything before the target idx can be ignored.
            // move all elements after the target target skilldriver without modifying order
            for (var i = targetIdx; i < skills.Length; i++)
            {
                if (i != currentIdx)
                {
                    // start with skill that currently occupies target idx
                    master.AddComponentCopy(skills[i]);
                    Component.DestroyImmediate(skills[i]);
                }
            }

            // sanity check
            skills = master.GetComponents<AISkillDriver>(); 
            var newTarget = skills.FirstOrDefault(s => s.customName == targetName);
            if (newTarget != null && Array.IndexOf(skills, newTarget) == targetIdx)
                Log.Debug($"Successfully set {targetName} to {targetIdx}");
            else
                Log.Error($"Done fucked it up on {targetName} with {targetIdx}");

            // restore overrides
            if (overrides.Any())
            {
                for (var i = 0; i < skills.Length; i++)
                {
                    var skill = skills[i];
                    if (skill && overrides.TryGetValue(skill.customName, out var target))
                    {
                        var skillComponent = skills.FirstOrDefault(s => s.customName == target);
                        if (skillComponent == null)
                        {
                            Log.Error($"Unable to reset skill override for {skill.customName} targeting {target}");
                        }
                        else
                        {
                            skill.nextHighPriorityOverride = skillComponent;
                            Log.Debug($"successfully reset override for {skill.customName} targeting {target}");
                        }
                    }
                }
            }
        }

        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var component))
            {
                Component.Destroy(component);
            }
        }

        public static void RemoveComponents<T>(this GameObject go) where T : Component
        {
            var coms = go.GetComponents<T>();
            for (var i = 0; i < coms.Length; i++)
            {
                Component.Destroy(coms[i]);
            }
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            var type = comp.GetType();
            if (type != other.GetType())
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T GetCopyOf<T>(this ScriptableObject comp, T other) where T : ScriptableObject
        {
            var type = comp.GetType();
            if (type != other.GetType())
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }


        public static T AddComponentCopy<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd);
        }

        /// <summary>
        /// aimRay must be on the stack before calling this!
        /// </summary>
        /// <param name="c"></param>
        /// <param name="type"></param>
        public static void EmitPredictAimray<T>(this ILCursor c, string prefabName = "projectilePrefab")
        {
            //this.characterbody
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody)));

            // this.projectilePrefab
            // - or -
            // {TYPE}.projectilePrefab
            var fieldInfo = AccessTools.Field(typeof(T), prefabName);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldarg_0);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldfld, fieldInfo);
            else c.Emit(OpCodes.Ldsfld, fieldInfo);

            // Utils.PredictAimRay(aimRay, characterBody, projectilePrefab);
            c.Emit(OpCodes.Call, typeof(Utils).GetMethodCached(nameof(Utils.PredictAimray)));
        }

        private static float dt => Time.fixedDeltaTime;
        private const float zero = 0.00001f;


        public static Ray PredictAimrayOLD(Ray aimRay, CharacterBody body, GameObject projectilePrefab)
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
                    aT = GetAccel(targetBody.characterMotor, ref vT);
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
                    aT = GetAccel(targetBody.characterMotor, ref vT);
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
        private static Vector3 GetAccel(CharacterMotor motor, ref Vector3 velocity)
        {
            float num = motor.acceleration;
            if (motor.isAirControlForced || !motor.isGrounded)
            {
                num *= (motor.disableAirControlUntilCollision ? 0f : motor.airControl);
            }

            Vector3 vector = motor.moveDirection;
            if (!motor.isFlying)
            {
                vector.y = 0f;
            }

            if (motor.body.isSprinting)
            {
                float magnitude = vector.magnitude;
                if (magnitude < 1f && magnitude > 0f)
                {
                    float num2 = 1f / vector.magnitude;
                    vector *= num2;
                }
            }

            Vector3 target = vector * motor.walkSpeed;
            if (!motor.isFlying)
            {
                target.y = velocity.y;
            }

            velocity = Vector3.MoveTowards(velocity, target, num * Time.fixedDeltaTime);
            if (motor.useGravity)
            {
                ref float y = ref velocity.y;
                y += Physics.gravity.y * dt;
                if (motor.isGrounded)
                {
                    y = Mathf.Max(y, 0f);
                }
            }
            return velocity;
        }
        //All in world space! Gets point you have to aim to
        //NOTE: this will break with infinite speed projectiles!
        //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
        public static Ray GetRay(Ray aimRay, float sP, Vector3 pT, Vector3 vT, Vector3 aT)
        {
            //time to target guess
            var t = Vector3.Distance(aimRay.origin, pT) / sP;

            // target position relative to ray position
            pT -= aimRay.origin;

            var useAccel = aT.sqrMagnitude > zero;

            //quartic coefficients
            // a = t^4 * (aT·aT / 4.0)
            // b = t^3 * (aT·vT)
            // c = t^2 * (aT·pT + vT·vT - s^2)
            // d = t   * (2.0 * vT·pT)
            // e =       pT·pT
            var c = vT.sqrMagnitude - Pow2(sP);
            var d = 2f * Vector3.Dot(vT, pT);
            var e = pT.sqrMagnitude;
            
            if (useAccel)
            {
                var a = aT.sqrMagnitude * 0.25f;
                var b = Vector3.Dot(aT, vT);
                c += Vector3.Dot(aT, pT);

                //solve with newton
                t = SolveQuarticNewton(t, 6, a, b, c, d, e);
            }
            else
            {
                t = SolveQuadraticNewton(t, 6, c, d, e);
            }

            t = SolveQuadraticNewton(t, 8, c, d, e);
            if (t > 0f)
            {
                //p(t) = pT + (vT * t) + ((aT/2.0) * t^2)
                var relativeDest = pT + (vT * t);
                //if (useAccel)
                //    relativeDest += 0.5f * aT * Pow2(t);
                return new Ray(aimRay.origin, relativeDest);
            }
            return aimRay;

        }

        private static float SolveQuarticNewton(float guess, int iterations, float a, float b, float c, float d, float e)
        {
            for (var i = 0; i < iterations; i++)
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
            for (var i = 0; i < iterations; i++)
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
            for (var i = 0; i < aiComponents.Length; i++)
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
    }
}
