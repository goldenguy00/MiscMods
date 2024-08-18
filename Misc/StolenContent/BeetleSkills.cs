using System.Linq;
using EntityStates;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MiscMods.StolenContent
{
    public class BeetleSkills
    {
        public static GameObject burrowFX;

        public static void Init()
        {
            ContentAddition.AddEntityState<EnterBurrow>(out _);
            ContentAddition.AddEntityState<BeetleBurrow>(out _);
            ContentAddition.AddEntityState<ExitBurrow>(out _);

            var burrowSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            burrowSkillDef.skillName = "GS_BeetleBodyBurrow";
            burrowSkillDef.activationStateMachineName = "Body";
            burrowSkillDef.activationState = new SerializableEntityStateType(typeof(EnterBurrow));
            burrowSkillDef.baseRechargeInterval = 8f;
            burrowSkillDef.cancelSprintingOnActivation = false;
            burrowSkillDef.isCombatSkill = false;
            ContentAddition.AddSkillDef(burrowSkillDef);

            Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Beetle/BeetleBodySecondaryFamily.asset").WaitForCompletion().variants[0].skillDef = burrowSkillDef;
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Beetle/EntityStates.BeetleMonster.SpawnState.asset").WaitForCompletion().TryModifyFieldValue(nameof(EntityStates.BeetleMonster.SpawnState.duration), 3.5f);
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleBody.prefab").WaitForCompletion().GetComponent<CharacterBody>().baseMoveSpeed = 7f;

            var master = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleMaster.prefab").WaitForCompletion();
            var followNodeGraphToTarget = master.GetComponents<AISkillDriver>().FirstOrDefault(x => x.customName == "FollowNodeGraphToTarget");
            if (followNodeGraphToTarget)
            {
                MonoBehaviour.DestroyImmediate(followNodeGraphToTarget);
            }

            var jumpAtTarget = master.AddComponent<AISkillDriver>();
            jumpAtTarget.customName = "BurrowTowardsTarget";
            jumpAtTarget.skillSlot = SkillSlot.Secondary;
            jumpAtTarget.requireSkillReady = true;
            jumpAtTarget.minDistance = 20f;
            jumpAtTarget.maxDistance = 60f;
            jumpAtTarget.selectionRequiresTargetLoS = true;
            jumpAtTarget.selectionRequiresOnGround = true;
            jumpAtTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            jumpAtTarget.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            jumpAtTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;

            var newFollowNodeGraphToTarget = master.AddComponent<AISkillDriver>();
            newFollowNodeGraphToTarget.customName = "FollowNodeGraphToTarget";
            newFollowNodeGraphToTarget.skillSlot = SkillSlot.None;
            newFollowNodeGraphToTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            newFollowNodeGraphToTarget.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            newFollowNodeGraphToTarget.aimType = AISkillDriver.AimType.MoveDirection;

            burrowFX = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardSunderPop.prefab").WaitForCompletion(), "BeetleBurrowEffect", false);
            burrowFX.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;

            var dust = burrowFX.transform.Find("Particles/ParticleInitial/Dust");
            if (dust && dust.TryGetComponent(out ParticleSystemRenderer dustRenderer))
            {
                dustRenderer.sharedMaterial = new Material(dustRenderer.sharedMaterial);
                dustRenderer.sharedMaterial.SetColor("_TintColor", new Color32(201, 126, 44, 255));
            }

            var decal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion().transform.Find("ParticleInitial/Decal");
            if (decal)
            {
                var burrowDecal = GameObject.Instantiate(decal.gameObject, burrowFX.transform);
                burrowDecal.transform.localScale = Vector3.one * 5f;
            }
            ContentAddition.AddEffect(burrowFX);
        }

        public class EnterBurrow : BaseState
        {
            public static float animSpeed = 1.3f;
            public static float crossfadeDelay = 1.1f;
            public static float crossfadeDuration = 0.2f;
            public static string burrowSoundString = "Play_treeBot_sprint_end";
            public static string startSoundString = "Play_beetle_worker_idle";

            private Animator modelAnimator;
            private float duration;
            private bool didCrossfade;

            public override void OnEnter()
            {
                base.OnEnter();
                duration = crossfadeDelay + crossfadeDuration; //Instance.baseBurrowEntryDuration;
                PlayCrossfade("Body", "EmoteSurprise", 0.1f);
                modelAnimator = GetModelAnimator();
                if (modelAnimator)
                {
                    modelAnimator.speed = animSpeed;

                }
                Util.PlaySound(startSoundString, gameObject);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (fixedAge >= crossfadeDelay)
                {
                    TryCrossfade();
                }
                if (isAuthority && fixedAge >= duration)
                {
                    outer.SetNextState(new BeetleBurrow());
                    return;
                }
            }

            public override void OnExit()
            {
                TryCrossfade();
                if (NetworkServer.active)
                {
                    Util.CleanseBody(characterBody, true, false, false, true, false, false);
                }
                base.OnExit();
            }

            public void TryCrossfade()
            {
                if (!didCrossfade)
                {
                    if (modelAnimator)
                    {
                        modelAnimator.speed = 1f;
                        modelAnimator.Update(0f);
                        modelAnimator.SetFloat("Spawn1.playbackRate", 0f);
                        modelAnimator.CrossFadeInFixedTime("Spawn1", crossfadeDuration, modelAnimator.GetLayerIndex("Body"));

                        EffectManager.SimpleEffect(burrowFX, characterBody.footPosition, Quaternion.identity, false);
                        Util.PlaySound(burrowSoundString, gameObject);
                        didCrossfade = true;
                    }
                }
            }
        }

        public class BeetleBurrow : BaseState
        {
            public static float burrowAccuracyCoefficient = 0.3f;
            public static float baseBurrowDuration = 1f;
            public static float radius = 10f;

            private HurtBox target;
            private Vector3 predictedDestination;
            public float duration;
            private CharacterModel characterModel;
            private HurtBoxGroup hurtboxGroup;

            public override void OnEnter()
            {
                base.OnEnter();
                duration = baseBurrowDuration;
                var modelTransform = GetModelTransform();
                if (modelTransform)
                {
                    characterModel = modelTransform.GetComponent<CharacterModel>();
                    hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
                }
                if (characterModel)
                {
                    characterModel.invisibilityCount++;
                }
                if (hurtboxGroup)
                {
                    hurtboxGroup.hurtBoxesDeactivatorCounter++;
                }
                if (characterMotor)
                {
                    characterMotor.enabled = false;
                }
                gameObject.layer = LayerIndex.fakeActor.intVal;
                characterMotor.Motor.RebuildCollidableLayers();
                CalculatePredictedDestination();
                Util.PlaySound("Play_magmaWorm_burrowed_loop", gameObject);
            }

            private void CalculatePredictedDestination()
            {
                var difference = Vector3.zero;
                var aimRay = GetAimRay();
                var bullseyeSearch = new BullseyeSearch
                {
                    searchOrigin = aimRay.origin,
                    searchDirection = aimRay.direction,
                    maxDistanceFilter = 100f,
                    teamMaskFilter = TeamMask.allButNeutral,
                    filterByLoS = false,
                    sortMode = BullseyeSearch.SortMode.Angle,
                };
                bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(gameObject));
                bullseyeSearch.RefreshCandidates();
                target = bullseyeSearch.GetResults().FirstOrDefault();
                if (target)
                {
                    difference = target.transform.position - transform.position;
                    var characterMotor = target.healthComponent?.body?.characterMotor;
                    if (characterMotor)
                    {
                        var moveDirection = characterMotor.moveDirection.normalized;
                        var estimatedTravelDistance = target.healthComponent.body.moveSpeed * duration;
                        var differenceFromMotor = difference + ((estimatedTravelDistance + radius) * moveDirection);
                        if (differenceFromMotor.sqrMagnitude <= radius * radius)
                        {
                            differenceFromMotor = difference - ((estimatedTravelDistance - radius) * moveDirection);
                        }
                        difference = differenceFromMotor;
                    }
                }
                predictedDestination = transform.position + difference;
                characterDirection.forward = difference;
            }

            private Vector3 GetFinalPosition()
            {
                var finalDestination = target ? Vector3.Lerp(predictedDestination, target.transform.position, burrowAccuracyCoefficient) : predictedDestination;
                var groundNodes = SceneInfo.instance.groundNodes;
                var nodeIndex = groundNodes.FindClosestNode(finalDestination, characterBody.hullClassification);
                groundNodes.GetNodePosition(nodeIndex, out finalDestination);
                finalDestination += transform.position - characterBody.footPosition;
                return finalDestination;
            }

            private void SetPosition(Vector3 newPosition)
            {
                characterMotor?.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (characterMotor)
                {
                    characterMotor.velocity = Vector3.zero;
                }
                if (isAuthority && fixedAge >= duration)
                {
                    outer.SetNextState(new ExitBurrow());
                }
            }

            public override void OnExit()
            {
                var finalPosition = GetFinalPosition();
                characterDirection.forward = finalPosition - transform.position;
                SetPosition(finalPosition);
                gameObject.layer = LayerIndex.defaultLayer.intVal;
                characterMotor?.Motor.RebuildCollidableLayers();
                if (characterModel)
                {
                    characterModel.invisibilityCount--;
                }
                if (hurtboxGroup)
                {
                    hurtboxGroup.hurtBoxesDeactivatorCounter--;
                }
                if (characterMotor)
                {
                    characterMotor.enabled = true;
                }
                Util.PlaySound("Stop_magmaWorm_burrowed_loop", gameObject);
                base.OnExit();
            }
        }

        public class ExitBurrow : BaseState
        {
            public static float ANIM_DURATION_COEF = 0.7f;
            public static float baseBurrowExitDuration = 1.3f;
            public static float exitJumpMarker = 0.4f;
            public static string endSoundString = "Play_hermitCrab_unburrow";
            public static string burrowSoundString = "Play_treeBot_sprint_end";

            private float duration;
            private bool didJump;
            private bool didCancelAnimation;
            private bool movementHitAuthority;

            public override void OnEnter()
            {
                base.OnEnter();
                duration = baseBurrowExitDuration / attackSpeedStat;
                PlayAnimation("Body", "Spawn1", "Spawn1.playbackRate", duration / ANIM_DURATION_COEF);
                if (isAuthority)
                {
                    characterMotor.onMovementHit += OnMovementHit;
                }
                /*if (characterMotor)
                {
                    characterMotor.Motor.ForceUnground();
                    //characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, 5f), characterMotor.velocity.z);
                    characterMotor.velocity += Vector3.up * Instance.exitVelocityBonus;
                }*/
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (fixedAge >= duration * exitJumpMarker / ANIM_DURATION_COEF)
                {
                    TryJump();
                }
                if (fixedAge < duration)
                {
                    return;
                }
                TryCancelAnimation();
                if (isAuthority && (movementHitAuthority || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
                {
                    outer.SetNextStateToMain();
                }
            }

            public override void OnExit()
            {
                TryJump();
                TryCancelAnimation();
                if (isAuthority)
                {
                    characterMotor.onMovementHit -= OnMovementHit;
                }
                base.OnExit();
            }

            public void TryJump()
            {
                if (!didJump && characterMotor)
                {
                    EffectManager.SimpleEffect(burrowFX, characterBody.footPosition, Quaternion.identity, false);
                    Util.PlaySound(burrowSoundString, gameObject);
                    Util.PlaySound(endSoundString, gameObject);
                    characterMotor.Motor.ForceUnground();
                    didJump = true;
                }
            }

            public void TryCancelAnimation()
            {
                if (!didCancelAnimation)
                {
                    PlayCrossfade("Body", isGrounded ? "Idle" : "Jump", 1f);
                    didCancelAnimation = true;
                }
            }

            private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
            {
                movementHitAuthority = true;
            }
        }
    }
}
