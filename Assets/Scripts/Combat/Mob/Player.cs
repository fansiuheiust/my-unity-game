using Progression;
using Progression.Balance;
using UnityEngine;

namespace Combat {
    public class Player : Mob {

        Transform _camera;

        public PlayerLevel Level { get; private set; }
        public PlayerPerk PerkManager { get; private set; } 
        [SerializeField, Tooltip("This leveling data will be used for the player's leveling")]
        Leveling levelingData;
        [SerializeField, Tooltip("This perk data will be used for the player's perks")]
        Perks perkData;
        protected override void Awake() {
            base.Awake();
            Level = new(levelingData);
            PerkManager = new(perkData);
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


        public void GainCoin(CoinType type, Rarity rarity, uint amount) {
            PerkManager.GainCoin(type, rarity, amount);
            uint tier = rarity switch {
                Rarity.Common => 0,
                Rarity.Rare => 1,
                Rarity.Epic => 2,
                Rarity.Legendary => 3,
                Rarity.Mythical => 4,
                _ => 0
            };
            Level.AddPoint((uint)(amount * Mathf.Pow(perkData.CoinDecompositionRatio, tier) * perkData.CoinPerLevelPoint));
        }
    }
}