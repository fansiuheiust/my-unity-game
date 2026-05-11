using UnityEngine;
using System.Collections;

namespace Combat {
    /// <summary>
    /// Block attack physically with your weapon
    /// </summary>
    public class WeaponBlock : Block {
        /// <summary>
        /// blade of the melee weapon
        /// </summary>
        Blade _blade;

        /// <summary>
        /// Change to the localPosition of model when started blocking
        /// </summary>
        Vector3 _blockChange = new(0.4f, -0.4f, 0);

        [SerializeField, Min(0.1f), Tooltip("How long blocking should last")] float max_block_dur = 0.7f;


        protected override void Awake() {
            base.Awake();
            _blade = transform.Find("Model").Find("Blade").GetComponent<Blade>();
        }

        /// <summary>
        /// indicates if action block is undergoing cooldown
        /// </summary>
        bool _blockUnderCd = false;
        /// <summary>
        /// Starts blocking
        /// </summary>
        public override void BlockClicked() {
            if (_blockUnderCd || WeaponObject.isActing) {
                base.BlockClicked();
                return;
            }

            WeaponObject.isActing = true;

            _blade.Stance = BladeStance.Block;
            WeaponObject.Model.transform.localPosition += _blockChange;
            BlockRotated(0);

            StartBlock();
            Owner.TakeStun(max_block_dur, null, true);

            if (Owner is Player p)
                p.RotateToCamera();

            StartCoroutine(BlockTire(max_block_dur));
        }

        /// <summary>
        /// Note that this will be called if the mob is "tired" of blocking
        /// </summary>
        public override void BlockLifted() {
            if (_blade.Stance != BladeStance.Block) return;
            WeaponObject.Model.transform.localPosition -= _blockChange;

            transform.localEulerAngles = Vector3.zero;
            WeaponObject.Model.transform.localEulerAngles = Vector3.zero;

            _blade.Stance = BladeStance.None;
            WeaponObject.isActing = false;

            Owner.InterruptStun();
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
            transform.localEulerAngles = new Vector3(0, -90 <= angle && angle < 90 ? -30 : 30, angle);
            WeaponObject.Model.transform.localEulerAngles = new Vector3(-30 <= angle && angle < 90 || -150 <= angle && angle < -90 ? -90 : 90, 0, 0);
        }
    }
}