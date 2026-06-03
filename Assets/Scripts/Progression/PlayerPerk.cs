using System.Collections.Generic;
using UnityEngine;
using Progression.Balance;


namespace Progression {
    public enum CoinType {
        Floor, RNG, Class
    }
    /// <summary>
    ///  for handling coins and perk unlocking of a player
    /// </summary>
    public class PlayerPerk {
        static uint[] InitialValuesPerTier => new uint[Global.Rarities.Length];
        readonly Dictionary<CoinType, uint[]> coins = new() {
            { CoinType.Floor, InitialValuesPerTier },
            { CoinType.RNG, InitialValuesPerTier },
            { CoinType.Class, InitialValuesPerTier }
        };
        public PerkTree FloorPerks { get; private set; }
        public PerkTree RNGPerks { get; private set; }
        public PerkTree ClassPerks { get; private set; }

        
        public PlayerPerk(Dictionary<CoinType, uint[]> coins, Dictionary<string, uint> floorPerks, Dictionary<string, uint> rngPerks, Dictionary<string, uint> classPerks): this() {
            this.coins = coins;
            foreach (var (id, level) in floorPerks)
                FloorPerks[id].LevelUp(level); // force level up a perk
            foreach (var (id, level) in rngPerks)
                RNGPerks[id].LevelUp(level);
            foreach (var (id, level) in classPerks)
                ClassPerks[id].LevelUp(level);
        }

        /// <summary>
        /// An empty player perk
        /// </summary>
        public PlayerPerk() {
            FloorPerks = StageController.PerkData.FloorPerkTree;
            RNGPerks = StageController.PerkData.RNGPerkTree;
            ClassPerks = StageController.PerkData.ClassPerkTree;
        }


        /// <summary>
        /// Number of coins of a coin type and tier
        /// </summary>
        public uint Coin(CoinType type, uint tier) => coins[type][tier];
        /// <summary>
        /// Obtains a number of quantity for a coin
        /// </summary>
        public void GainCoin(CoinType type, uint tier, uint quantity = 1) => coins[type][tier]+= quantity;
    }
}
