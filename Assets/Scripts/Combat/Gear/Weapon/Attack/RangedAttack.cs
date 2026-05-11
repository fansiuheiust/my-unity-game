using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Combat {
    public class RangedAttack : Attack {

        [SerializeField] string projectileName = "Default";
        Transform _rotatable;
        bool isCharging = false;
        float timeElapsed = 0f;


        protected override void Awake() {
            base.Awake();
            _rotatable = Owner.transform.Find("Rotatable");
        }
        public override void AttackClicked(float attackTime) {
            if (WeaponObject.isActing) {
                base.AttackClicked(attackTime);
                return;
            }

            // does stuff
            WeaponObject.isActing = true;
            isCharging = true;

            StartAttack();
            transform.localEulerAngles = new Vector3(-90, 0, 0);
        }

        public override void AttackLifted(float attackTime) {
            if (!isCharging) return;
            transform.localEulerAngles = Vector3.zero;

            // spawn projectile
            Vector3 spawnPos = Owner.transform.position + Vector3.up * Owner.GetComponent<CapsuleCollider>().height/2;
            string path = $"Prefabs/Weapon/Projectile/{projectileName}";
            Projectile projectile = Instantiate(Resources.Load(path)).GetComponent<Projectile>();

            projectile.transform.position = spawnPos;

            float powerRatio = Mathf.Min(timeElapsed / attackTime, 1f);
            Vector3 speedDir = Owner is Player p ? p.Camera.forward : _rotatable.forward;
            projectile.Set(Owner, powerRatio, powerRatio * Mathf.Sqrt(-Physics.gravity.y * ((RangedObject)WeaponObject).AttackRange) * speedDir);
            
            
            
            
            EndAttack();
            ResetAttackControl();
            WeaponObject.isActing = false;
            isCharging = false;
            timeElapsed = 0f;
        }

        private void Update() {
            if (!isCharging) return;
            timeElapsed += Time.deltaTime;
            if (Owner is Player p) {
                p.RotateToCamera();
            }
        }
    }
}
