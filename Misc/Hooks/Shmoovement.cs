using System;
using HarmonyLib;
using KinematicCharacterController;
using MonoMod.RuntimeDetour.HookGen;
using RoR2;
using UnityEngine;
using K = KinematicCharacterController.KinematicCharacterSystem;

namespace MiscMods.Hooks
{
    public class Shmoovement
    {
        public static Shmoovement instance;

        public static void Init() => instance ??= new Shmoovement();

        private Shmoovement()
        {
            On.KinematicCharacterController.KinematicCharacterSystem.FixedUpdate += KinematicCharacterSystem_FixedUpdate;
            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;
            On.RoR2.CharacterMotor.Awake += CharacterMotor_Awake;
            //CharacterMotor.cvCMotorSafeCollisionStepThreshold.value = 1.2f;
        }

        private void KinematicCharacterSystem_FixedUpdate(On.KinematicCharacterController.KinematicCharacterSystem.orig_FixedUpdate orig, K self)
        {
            if (K.AutoSimulation)
            {
                float deltaTime = Time.deltaTime;
                if (deltaTime != 0f)
                {
                    foreach (var motor in K.CharacterMotors)
                    {
                        // pre simulation
                        if (K.InterpolationMethod == CharacterSystemInterpolationMethod.Custom)
                        {
                            motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
                            motor.Rigidbody.position = motor.TransientPosition;
                            motor.Rigidbody.rotation = motor.TransientRotation;
                        }

                        motor.InitialTickPosition = motor.Transform.position;
                        motor.InitialTickRotation = motor.Transform.rotation;

                        // Simulate
                        motor.UpdatePhase1(deltaTime);
                        motor.UpdatePhase2(deltaTime);
                        motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
                        motor.Rigidbody.position = motor.TransientPosition;
                        motor.Rigidbody.rotation = motor.TransientRotation;
                    }

                    // post simulation
                    Physics.SyncTransforms();
                    if (K.InterpolationMethod == CharacterSystemInterpolationMethod.Custom)
                    {
                        K._lastCustomInterpolationStartTime = Time.time;
                        K._lastCustomInterpolationDeltaTime = Time.fixedDeltaTime;
                    }

                    foreach (var motor in K.CharacterMotors)
                    {
                        motor.Rigidbody.position = motor.InitialTickPosition;
                        motor.Rigidbody.rotation = motor.InitialTickRotation;
                        motor.Rigidbody.MovePosition(motor.TransientPosition);
                        motor.Rigidbody.MoveRotation(motor.TransientRotation);
                    }
                }
            }
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            Time.fixedDeltaTime = 0.0333f;
            Time.maximumDeltaTime = 0.1f;
            K.InterpolationMethod = CharacterSystemInterpolationMethod.Unity;
        }

        private void CharacterMotor_Awake(On.RoR2.CharacterMotor.orig_Awake orig, CharacterMotor self)
        {
            orig(self);

            self.ledgeHandling = false;
            self.Motor.StepHandling = StepHandlingMethod.None;
            self.interactiveRigidbodyHandling = false;
        }
    }
}
