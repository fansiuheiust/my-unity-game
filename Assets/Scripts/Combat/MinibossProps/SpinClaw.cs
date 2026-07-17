using System.Collections;
using UnityEngine;

namespace Combat.Miniboss {
    public class SpinClaw : MonoBehaviour {
        [SerializeField]
        WeaponBody hitbox;
        [SerializeField]
        Transform model;
        public void Set(Mob owner, float attackTime, float radius) {
            hitbox.Set(owner);
            owner.OnDeath.AddListener(OnOwnerDead);
            hitbox.attackTime = attackTime;
            transform.localScale = radius * Vector3.one;
        }

        public void Spin(float time) {
            StartCoroutine(SpinAnimation(time));
        }
        IEnumerator SpinAnimation(float time) {
            hitbox.Stance = BladeStance.Attack;
            for (float t = 0; t < time; t += Time.deltaTime) {
                transform.localEulerAngles += 360 / time * Time.deltaTime * Vector3.up;
                yield return null;
            }
            Destroy(gameObject);
        }


        void OnOwnerDead(Mob _, Mob __) {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}