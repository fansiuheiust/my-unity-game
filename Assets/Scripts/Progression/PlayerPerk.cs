using System.Collections.Generic;
using UnityEngine;


namespace Progression {
    public enum CoinType {
        Floor, RNG, MiscBuffs
    }
    /// <summary>
    ///  for handling coins and perk unlocking of a player
    /// </summary>
    public class PlayerPerk {
        static Dictionary<Rarity, uint> InitialValuesPerRarity => new() {
            { Rarity.Common, 0 },
            { Rarity.Rare, 0 },
            { Rarity.Epic, 0 },
            { Rarity.Legendary, 0 },
            { Rarity.Mythical, 0 },
        };
        readonly Dictionary<CoinType, Dictionary<Rarity, uint>> coins = new() {
            { CoinType.Floor, InitialValuesPerRarity },
            { CoinType.RNG, InitialValuesPerRarity },
            { CoinType.MiscBuffs, InitialValuesPerRarity }
        };
        public PerkTree PerkTree { get; private set; }

        /// <summary>
        /// Number of coins of a coin type and rarity
        /// </summary>
        public uint Coin(CoinType type, Rarity rarity) => coins[type][rarity];
        /// <summary>
        /// Obtains a number of quantity for a coin
        /// </summary>
        public void GainCoin(CoinType type, Rarity rarity, uint quantity = 1) => coins[type][rarity]+= quantity;
    }
}
