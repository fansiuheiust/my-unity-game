using UnityEngine;

namespace Combat {
    public class Player : Mob {

        Transform _camera;

        protected override void Awake() {
            base.Awake();
            _camera = transform.Find("Camera");
            Faction = Faction.Ally;
        }



        protected override void Die(Mob killer) {
            Debug.Log("You died, but let me restore your HP.");
            Stats.Heal(Stats.Final.MaxHp, Stats);
        }

        /// <summary>
        /// Rotates player's movement
        /// </summary>
        /// <param name="rotation">rotation</param>
        public void RotateMovement(Quaternion rotation) {
            CastMovement<PlayerMovement>().Rotate(rotation);
        }

        public void RotateToCamera() {
            _rotatable.localEulerAngles = new Vector3(0, _camera.localEulerAngles.y, 0);
        }
    }
}