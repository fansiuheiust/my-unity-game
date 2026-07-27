using Progression;
using Progression.Balance;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Combat {
    public class Player : Mob {

        public Transform Camera { get; private set; }
        public PlayerLevel Level { get; private set; }
        public PlayerPerk PerkManager { get; private set; }
        protected override void Awake() {
            base.Awake();
            

            Camera = transform.Find("Camera");
            Faction = Faction.Ally;

            // while (StageController.instance == null) ;
            Level = StageController.PlayerLevel;
            PerkManager = StageController.PlayerPerk;
            Level.PlayerLevelChanged += OnLevelChanged;

        }

        void OnLevelChanged(uint newLevel) {
            
        }



        protected override void Die(Mob killer) {
            Debug.Log("You died, but let me restore your HP.");
            stats.Heal(Stats[BaseAttribute.MaxHp], stats);
        }

        /// <summary>
        /// Rotates player's movement
        /// </summary>
        /// <param name="rotation">rotation</param>
        public void RotateMovement(Quaternion rotation) {
            CastMovement<PlayerMovement>().Rotate(rotation);
        }

        public void RotateToCamera() {
            Rotatable.localEulerAngles = new Vector3(0, Camera.localEulerAngles.y, 0);
        }


        public void GainCoin(CoinType type, uint tier, uint amount) {
            PerkManager.GainCoin(type, tier, amount);
            Level.AddPoint((uint)(amount * Mathf.Pow(StageController.PerkData.CoinDecompositionRatio, tier) * StageController.PerkData.CoinPerLevelPoint));
        }


        
    }
}