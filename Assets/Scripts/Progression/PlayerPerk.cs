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
        public PerkTree PerkTree { get; private set; }

        
        public PlayerPerk(Dictionary<CoinType, uint[]> coins) {
            this.coins = coins;
        }

        /// <summary>
        /// An empty player perk
        /// </summary>
        public PlayerPerk() { }


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
