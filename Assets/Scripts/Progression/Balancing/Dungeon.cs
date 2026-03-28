using UnityEngine;

namespace Progression.Balance {

    [CreateAssetMenu(fileName = "Dungeon", menuName = "Scriptable Objects/Dungeon")]
    public class Dungeon : ScriptableObject {
        [field: SerializeField]
        public int NumFloors { get; private set; } = 10;
        [field: SerializeField, Tooltip("Multiplier to mob's base stats for every room")]
        public AnimationCurve MobBaseStatsMultiplier { get; private set; }
        [field: SerializeField, Tooltip("How much coin in loot chests on average, normal dist")]
        public float CoinMeanLootChest { get; private set; }
        [field: SerializeField, Tooltip("S.D of coin getting in loot chests, normal dist")]
        public float CoinSDLootChest { get; private set; }
        [field: SerializeField, Tooltip("How much coin one get on average at the end of dungeon, normal dist")]
        public float CoinMeanDungeonEnd { get; private set; }
        [field: SerializeField, Tooltip("S.D of coin getting at the end of dungeon, normal dist")]
        public float CoinSDDungeonEnd { get; private set; }

        [field: SerializeField, Tooltip("Tier per room. Tier 0 is common, ..., 4 is mythical. decimal tier: that the decimal part is the probability of getting a coin 1 tier higher")]
        public AnimationCurve CoinTier { get; private set; }

        [SerializeField, Tooltip("Number of mob rooms per floor")]
        int[] mobRoomCounts;
        public int MobRoomCounts(uint i) => mobRoomCounts[i];
        [SerializeField, Tooltip("Number of puzzle rooms per floor")]
        int[] puzzleRoomCounts;
        public int PuzzleRoomCounts(uint i) => puzzleRoomCounts[i];
        [SerializeField, Tooltip("Number of miniboss rooms per floor")]
        int[] minibossRoomCounts;
        public int MinibossRoomCounts(uint i) => minibossRoomCounts[i];


    }
}