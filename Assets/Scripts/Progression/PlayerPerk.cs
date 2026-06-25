using System.Collections.Generic;
using UnityEngine;
using Progression.Balance;
using System.Linq;


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

        public void LoadFromSave(Dictionary<CoinType, uint[]> coins, Dictionary<string, uint> floorPerks, Dictionary<string, uint> rngPerks, Dictionary<string, uint> classPerks) {
            foreach (var c in coins)
                this.coins[c.Key] = c.Value.ToArray();
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

        /// <summary>
        /// Whether player can afford to upgrade a perk, maxed out perks are treated as unaffordable
        /// </summary>
        /// <param name="perkType">type of the perk</param>
        /// <param name="perkID">ID of a perk</param>
        public bool CanAfford(CoinType perkType, string perkID) => CanAfford((perkType switch {
                CoinType.Class => ClassPerks,
                CoinType.Floor => FloorPerks,
                CoinType.RNG => RNGPerks,
                _ => throw new System.NotImplementedException($"Please implement for {perkType}")
            })[perkID]);
        /// <summary>
        /// Whether player can afford to upgrade a perk, maxed out perks are treated as unaffordable
        /// </summary>
        /// <param name="target">perk to check</param>
        public bool CanAfford(Perk target) => target.Level != target.maxLevel && coins[target.Cost.type][target.Cost.tier] >= target.Cost.value;

        /// <summary>
        /// Whether player can unlock a perk
        /// </summary>
        /// <param name="target">Perk to be checked</param>
        /// <returns></returns>
        public bool CanUnlock(Perk target) => CanAfford(target) && TreeOf(target.type).Unlockable(target);

        /// <summary>
        /// Whether player can unlock a perk
        /// </summary>
        /// <param name="perkType">Type of the perk</param>
        /// <param name="perkID">ID of the perk</param>
        public bool CanUnlock(CoinType perkType, string perkID) => CanUnlock(TreeOf(perkType)[perkID]);

        /// <summary>
        /// Levels up a perk if affordable and not violating dependency/exclusions
        /// </summary>
        /// <param name="perkType">type of the perk</param>
        /// <param name="perkID">ID of the perk</param>
        /// <returns>true iff leveled up</returns>
        public bool TryLevelUp(CoinType perkType, string perkID) {
            if (!TreeOf(perkType).Contains(perkID)) {
                Debug.Log($"Note that perk tree {perkType} does not contain perk with ID {perkID}");
                return false; 
            }
            if (!CanUnlock(TreeOf(perkType)[perkID])) return false;
            var (type, tier, value) = TreeOf(perkType)[perkID].Cost;
            coins[type][tier] -= value;
            TreeOf(perkType).LevelUp(perkID);
            return true;
        }

        public PerkTree TreeOf(CoinType type) => type switch {
            CoinType.Floor => FloorPerks,
            CoinType.RNG => RNGPerks,
            CoinType.Class => ClassPerks,
            _ => throw new System.NotImplementedException($"No implementation for {type}")
        };
    }
}
