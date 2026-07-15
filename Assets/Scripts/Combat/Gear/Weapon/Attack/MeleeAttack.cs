using UnityEngine;
using System.Collections;

namespace Combat {
    public class MeleeAttack : Attack {
        
        /// <summary>
        /// blade of the melee weapon
        /// </summary>
        WeaponBody _blade;

        /// <summary>
        /// The active attack animation
        /// </summary>
        Coroutine _attackAnimation;

        protected override void Awake() {
            base.Awake();
            _blade = transform.Find("Model").Find("Blade").GetComponent<WeaponBody>();
            _blade.OnAttackInterrupted += AttackInterruptedByBlock;
        }

        /// <summary>
        /// Performs an attack
        /// </summary>
        /// <param name="attackTime">self-documenting</param>
        public override void AttackClicked(float attackTime) {
            if (WeaponObject.isActing) { // mutual exclusion
                base.AttackClicked(0);
                return;
            }

            WeaponObject.isActing = true;

            // in this part, attack will take place

            Owner.TakeStun(attackTime, null, true);

            if (Owner is Player p)
                p.RotateToCamera();

            Owner.OnStunStart.AddListener(AttackInterruptedByStun);

            _blade.attackTime = attackTime;
            if (Owner is Player) { // hard-code
                Swing(attackTime);
            } else {
                CourteousSwing(attackTime);
            }
        }
        /// <summary>
        /// Nothing
        /// </summary>
        /// <param name="attackTime">self-documenting</param>
        public override void AttackLifted(float attackTime) {
            // blank
        }

        // swing blade
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

            WeaponObject.Model.localEulerAngles = new Vector3(0, 90, 90);
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
                yield return null;
            }


            // disable blade collision and stay
            _blade.Stance = BladeStance.Idle;
            yield return new WaitForSeconds(2 * time / 3);

            CancelAttack();

            yield break;
        }


        /// <summary>
        /// called when an attack should end (due to end of attack or interruption)
        /// </summary>
        void CancelAttack() {
            transform.localEulerAngles = Vector3.zero;
            WeaponObject.Model.localEulerAngles = Vector3.zero;
            _blade.Stance = BladeStance.None;

            Owner.OnStunStart.RemoveListener(AttackInterruptedByStun);

            EndAttack();
            ResetAttackControl();
            WeaponObject.isActing = false;
        }

        /// <summary>
        /// What happens when a blade gets blocked
        /// </summary>
        /// <param name="src">blocker</param>
        void AttackInterruptedByBlock(Mob src) {
            StopCoroutine(_attackAnimation);
            CancelAttack();
            _attackAnimation = null;
            Owner.OnAttackInterrupt.Invoke(Owner, src);
        }
        /// <summary>
        /// What happens when stunned during attack
        /// </summary>
        /// <param name="self">Useless</param>
        void AttackInterruptedByStun(Mob self) {
            // script that interrupts attack when stunned
            // if (_attackAnimation != null) StopCoroutine(_attackAnimation);
            // CancelAttack();
            // _attackAnimation = null;
        }

        void CourteousSwing(float time) {
            _attackAnimation = StartCoroutine(CourteousSwingAnimation(time));
        }

        /// <summary>
        /// 33.3333%: positioning sword to 90 degrees
        /// 66.6667%: swing animation
        /// </summary>
        /// <param name="time">self-documenting</param>
        IEnumerator CourteousSwingAnimation(float time) {
            _blade.Stance = BladeStance.Idle;
            WeaponObject.Model.localEulerAngles = new(0, 0, 90);
            float courtesyDur = time / 2f;
            float angularVelocity = 90f / courtesyDur;
            for (float raiseTime = 0; raiseTime < courtesyDur; raiseTime += Time.deltaTime) {
                WeaponObject.Model.localEulerAngles += new Vector3(0, angularVelocity * Time.deltaTime, 0);
                yield return null;
            }

            _attackAnimation = StartCoroutine(SwingAnimation(time / 2f));

            yield return null;
        }
    }
}