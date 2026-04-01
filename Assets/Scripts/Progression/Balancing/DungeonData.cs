using UnityEngine;

namespace Progression.Balance {

    [CreateAssetMenu(fileName = "Dungeon", menuName = "Scriptable Objects/Dungeon")]
    public class DungeonData : ScriptableObject {
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
        uint[] mobRoomCounts;
        public uint MobRoomCounts(uint floor) => mobRoomCounts[floor];
        [SerializeField, Tooltip("Number of puzzle rooms per floor")]
        uint[] puzzleRoomCounts;
        /// <param name="floor">Floor</param>
        public uint PuzzleRoomCounts(uint floor) => puzzleRoomCounts[floor-1];
        [SerializeField, Tooltip("Number of miniboss rooms per floor")]
        uint[] minibossRoomCounts;
        /// <param name="floor">Floor</param>
        public uint MinibossRoomCounts(uint floor) => minibossRoomCounts[floor-1];
        [SerializeField, Tooltip("Number of rooms in the main path")]
        uint[] mainPathCounts;
        public uint MainPathCounts(uint floor) => mainPathCounts[floor - 1];

        [field: SerializeField, Tooltip("Length of each room")]
        public uint RoomLength { get; private set; }
        [field: SerializeField, Tooltip("How many blocks there are between rooms")]
        public uint WallThickness { get; private set; }
        [SerializeField, Tooltip("Options of normal room")]
        string[] normalRoomShapes;
        public string NormalRoomShapes(uint floor) => normalRoomShapes[floor];
        public uint NormalRoomShapeLength => (uint)normalRoomShapes.Length;

    }
}