using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
namespace Combat {
    /// <summary>
    /// Attached to the Melee weapon gameObject, used for handling the melee weapon's movement
    /// </summary>
    public class MeleeObject : WeaponObject {
        /// <summary>
        /// The rigid body ceneted around the weapon
        /// </summary>
        Transform _model;
        /// <summary>
        /// blade of the melee weapon
        /// </summary>
        Blade _blade;

        /// <summary>
        /// Change to the localPosition of model when started blocking
        /// </summary>
        Vector3 _blockChange = new(0.5f, -0.4f, 0);

        /// <summary>
        /// The active attack animation
        /// </summary>
        Coroutine _attackAnimation;

        protected override void Awake() {
            base.Awake();
            _blade = transform.Find("Model").Find("Blade").GetComponent<Blade>();
            _model = transform.Find("Model");
            _blade.OnAttackInterrupted += AttackInterruptedByBlock;
        }

        /// <summary>
        /// Performs an attack
        /// </summary>
        /// <param name="attackTime">self-documenting</param>
        public override void AttackClicked(float attackTime) {
            if (_blade.Stance != BladeStance.None) {
                base.AttackClicked(0);
                return;
            }

            // in this part, attack will take place

            owner.TakeStun(attackTime, null, true);

            if (owner is Player p)
                p.RotateToCamera();

            owner.OnStunStart.AddListener(AttackInterruptedByStun);

            _blade.attackTime = attackTime;
            Swing(attackTime);
        }
        /// <summary>
        /// Nothing
        /// </summary>
        /// <param name="attackTime">self-documenting</param>
        public override void AttackLifted(float attackTime) {
            // blank
        }

        const float max_block_dur = 0.5f;
        /// <summary>
        /// indicates if action block is undergoing cooldown
        /// </summary>
        bool _blockUnderCd = false;
        /// <summary>
        /// Starts blocking
        /// </summary>
        public override void BlockClicked() {
            if (_blockUnderCd || _blade.Stance != BladeStance.None) {
                base.BlockClicked();
                return;
            }
            _blade.Stance = BladeStance.Block;
            _model.transform.localPosition += _blockChange;
            BlockRotated(0);

            StartBlock();
            owner.TakeStun(max_block_dur, null, true);

            if (owner is Player p)
                p.RotateToCamera();

            StartCoroutine(BlockTire(max_block_dur));
        }

        /// <summary>
        /// Note that this will be called if the mob is "tired" of blocking
        /// </summary>
        public override void BlockLifted() {
            if (_blade.Stance != BladeStance.Block) return;
            _model.transform.localPosition -= _blockChange;

            transform.localEulerAngles = Vector3.zero;
            _model.transform.localEulerAngles = Vector3.zero;

            _blade.Stance = BladeStance.None;
            owner.InterruptStun();
            StartCoroutine(BlockCooldown(max_block_dur));
            EndBlock();
            ResetBlockControl();
        }
        /// <summary>
        /// Stops blocking after a set amount of time
        /// </summary>
        /// <param name="time">Time until blocking stops</param>
        IEnumerator BlockTire(float time) {
            yield return new WaitForSeconds(time);
            BlockLifted();
            yield break;
        }
        IEnumerator BlockCooldown(float time) {
            _blockUnderCd = true;
            yield return new WaitForSeconds(time);
            _blockUnderCd = false;
        }
        public override void BlockRotated(float angle) {
            // localEuler x of model:
            // [-30, 90):     -90
            // [90, 180) \cup [-180, -150):   90
            // [-150, -90): -90
            // [-90, -30):    90
            transform.localEulerAngles = new Vector3(0, 0, angle);
            _model.transform.localEulerAngles = new Vector3(-30 <= angle && angle < 90 || -150 <= angle && angle < -90 ? -90 : 90, 0, 0);
        }




        /// <summary>
        /// Swing time distribution: 33.33333...% 180 degree movement (from right to left), 66.66666...% stay at left 
        /// </summary>
        /// <param name="time">Duration of the swing</param>
        public void Swing(float time) {
            _attackAnimation = StartCoroutine(SwingAnimation(time));
        }
        IEnumerator SwingAnimation(float time) {
            StartAttack();

            _blade.Stance = BladeStance.Attack;

            _model.localEulerAngles = new Vector3(0, 90, 90);
            //  swing
            float vel = 0;
            float swingTime = time / 3;
            for (float timePassed = 0; timePassed < swingTime; timePassed += Time.deltaTime) {
                // 50% time: time*.5
                // theta target: \pi
                // find \alpha
                // for simplicity, i will use linear symbols instead
                // s = ut+1/2*at^2
                // 180 = 1/2*a*(swingTime)^2 
                // a = 360/(swingTime*swingTime)

                // v = at
                vel -= 360 / (swingTime * swingTime) * Time.deltaTime;

                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + vel * Time.deltaTime, 0);
                // _rb.AddTorque(new Vector3(0, 8 * Mathf.PI / (time * time), 0), ForceMode.Acceleration);
                yield return new WaitForSeconds(Time.deltaTime);
            }


            // disable blade collision and stay
            _blade.Stance = BladeStance.Idle;
            yield return new WaitForSeconds(2 * time / 3);

            CancelAttack();

            yield break;
        }


        /// <summary>
        /// called when an attack should be cancelled (due to animation over or blocked)
        /// </summary>
        void CancelAttack() {
            transform.localEulerAngles = Vector3.zero;
            _model.localEulerAngles = Vector3.zero;
            _blade.Stance = BladeStance.None;

            owner.OnStunStart.RemoveListener(AttackInterruptedByStun);

            EndAttack();
            ResetAttackControl();
        }

        /// <summary>
        /// What happens when a blade gets blocked
        /// </summary>
        /// <param name="src">blocker</param>
        void AttackInterruptedByBlock(Mob src) {
            StopCoroutine(_attackAnimation);
            CancelAttack();
            _attackAnimation = null;
            owner.OnAttackInterrupt.Invoke(owner, src);
        }
        /// <summary>
        /// What happens when stunned during attack
        /// </summary>
        /// <param name="self">Useless</param>
        void AttackInterruptedByStun(Mob self) {
            if (_attackAnimation != null) StopCoroutine(_attackAnimation);
            CancelAttack();
            _attackAnimation = null;
        }
    }
}