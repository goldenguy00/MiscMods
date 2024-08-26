using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MiscMods.StolenContent.Beetle
{
    public class BeetleSpit : BaseState
    {
        public static float baseDuration = 1f;
        public static float damageCoefficient;
        public static string attackSoundString = "Play_beetle_worker_attack";

        private bool hasFired;
        private float stopwatch;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.stopwatch = 0f;
            this.duration = baseDuration / this.attackSpeedStat;
            this.GetModelTransform();
            this.StartAimMode();
            Util.PlayAttackSpeedSound(attackSoundString, this.gameObject, 2f);
            this.PlayCrossfade("Body", "EmoteSurprise", "Headbutt.playbackRate", this.duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.deltaTime;
            if (!this.hasFired && this.stopwatch >= this.duration)
            {
                this.hasFired = true;
                var aimRay = Utils.PredictAimray(this.GetAimRay(), this.characterBody, BeetleChanges.beetleSpit);
                ProjectileManager.instance.FireProjectile(BeetleChanges.beetleSpit, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat * 1, 0.0f, Util.CheckRoll(this.critStat, this.characterBody.master));
            }
            if (this.fixedAge < this.duration || !this.isAuthority)
                return;
            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
