using Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class CoinReward : MonoBehaviour {
        [SerializeField]
        TextMeshProUGUI rewardList;
        [SerializeField]
        GameObject rewardChooser;
        [SerializeField]
        Button floorChooser, rngChooser, classChooser;

        [SerializeField, Min(1)]
        int minCoin = 4, maxCoin = 9;

        float optimality;
        public void Init(float optimality) {
            this.optimality = optimality;
            if (StageController.PlayerPerk.RNGPerks.Unlocked("OptimalFinancer") && optimality >= StageController.PlayerPerk.RNGPerks["OptimalFinancer"]["Optimality Threshold"]) {
                InitChooser();
                rewardList.text = "";
                return;
            }
            Destroy(rewardChooser);
            AwardCoin((CoinType)Random.Range(0, 2));
        }

        void InitChooser() {
            floorChooser.GetComponentInChildren<TextMeshProUGUI>().color = GlobalColor.Parse(GlobalColor.Perk.PerkType(CoinType.Floor));
            floorChooser.onClick.AddListener(() => OnButtonClick(CoinType.Floor));
            rngChooser.GetComponentInChildren<TextMeshProUGUI>().color = GlobalColor.Parse(GlobalColor.Perk.PerkType(CoinType.RNG));
            rngChooser.onClick.AddListener(() => OnButtonClick(CoinType.RNG));
            classChooser.GetComponentInChildren<TextMeshProUGUI>().color = GlobalColor.Parse(GlobalColor.Perk.PerkType(CoinType.Class));
            classChooser.onClick.AddListener(() => OnButtonClick(CoinType.Class));
        }


        public void OnButtonClick(CoinType type) {
            floorChooser.onClick.RemoveAllListeners();
            rngChooser.onClick.RemoveAllListeners();
            classChooser.onClick.RemoveAllListeners();
            Destroy(rewardChooser);
            AwardCoin(type);
        }
        public void AwardCoin(CoinType type) {
            uint totalAwardedCoin = (uint)Mathf.RoundToInt(optimality * Random.Range(minCoin, maxCoin));
            float coinTierDistribution = StageController.DungeonData.CoinTier.Evaluate(StageController.Floor);
            uint tier = (uint)coinTierDistribution;
            float lowTierProportion = coinTierDistribution - tier;
            uint lowTierCoins = (uint)(lowTierProportion * totalAwardedCoin);
            uint highTierCoins = totalAwardedCoin - lowTierCoins;
            if (lowTierCoins > 0)
                StageController.PlayerPerk.GainCoin(type, tier, lowTierCoins);
            if (highTierCoins > 0)
                StageController.PlayerPerk.GainCoin(type, tier + 1, highTierCoins);
            string toPrint = "";
            if (lowTierCoins > 0)
                toPrint += RewardInfo(type, tier, lowTierCoins) + "\n";
            if (highTierCoins > 0)
                toPrint += RewardInfo(type, tier+1, highTierCoins) + "\n";
            rewardList.text = toPrint;
        }
        string RewardInfo(CoinType type, uint tier, uint amount) => $"<color={GlobalColor.Coin}>{amount}</color><b> <color={GlobalColor.RarityTiers[tier]}>{Global.Rarities[tier]}</color> <color={GlobalColor.Perk.PerkType(type)}>{type}</b>";
    }
}