using System.Collections;
using UnityEngine;

namespace Combat {
    public class Dodge : Block {
        [SerializeField, Min(0.1f), Tooltip("How long dodge should last")] float dodgeDur = 0.5f;
        [SerializeField, Min(0.1f), Tooltip("How long dodge should cooldown, note that this starts counting down AFTER dodging")] float dodgeCD = 1.7f;

        bool _cd = false;
        public override void BlockClicked() {
            if (_cd || WeaponObject.isActing) {
                base.BlockClicked();
                return;
            }

            WeaponObject.isActing = true;
            _cd = true;
            StartBlock();
            ResetBlockControl();

            StartCoroutine(DodgeAnimation());
        }

        IEnumerator DodgeAnimation() {
            Owner.TakeStun(dodgeDur, null, true);
            Owner.TakeKnockback(Owner.transform.position - Owner.transform.Find("Rotatable").forward, dodgeDur, true, 2.236f);
            Owner.AddEffect<Immunity>().Apply(dodgeDur);
            // Owner.GetComponent<Rigidbody>().AddForce(Owner.transform.Find("Rotatable").forward*50f, ForceMode.VelocityChange);
            
            yield return new WaitForSeconds(dodgeDur);

            WeaponObject.isActing = false;
            EndBlock();
            yield return new WaitForSeconds(dodgeCD);
            _cd = false;
        }
    }
}
