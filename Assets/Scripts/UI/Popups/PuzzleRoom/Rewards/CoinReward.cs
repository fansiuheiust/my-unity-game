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

        CoinType coinType;
        float rewardRatio;
        public void Init(float optimality, float rewardRatio) {
            this.rewardRatio = rewardRatio;
            this.optimality = optimality;
            if (StageController.PlayerPerk.RNGPerks.Unlocked("OptimalFinancer") && optimality >= StageController.PlayerPerk.RNGPerks["OptimalFinancer"]["Optimality Threshold"]) {
                GetComponent<Popup>().CanExit = false;
                InitChooser();
                rewardList.text = "";
                return;
            }
            Destroy(rewardChooser);
            coinType = (CoinType)Random.Range(0, 2);
            AwardCoin();
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
            coinType = type;
            AwardCoin();
            GetComponent<Popup>().CanExit = true;
        }
        public void AwardCoin() {
            uint totalAwardedCoin = (uint)Mathf.RoundToInt(optimality * Random.Range(minCoin, maxCoin) * rewardRatio);
            float coinTierDistribution = StageController.DungeonData.CoinTier.Evaluate(StageController.Floor);
            uint tier = (uint)coinTierDistribution;
            float lowTierProportion = 1-(coinTierDistribution - tier);
            uint lowTierCoins = (uint)(lowTierProportion * totalAwardedCoin);
            uint highTierCoins = totalAwardedCoin - lowTierCoins;
            if (lowTierCoins > 0)
                StageController.PlayerPerk.GainCoin(coinType, tier, lowTierCoins);
            if (highTierCoins > 0)
                StageController.PlayerPerk.GainCoin(coinType, tier + 1, highTierCoins);
            string toPrint = "You have obtained:\n";
            if (lowTierCoins > 0)
                toPrint += RewardInfo(coinType, tier, lowTierCoins) + "\n";
            if (highTierCoins > 0)
                toPrint += RewardInfo(coinType, tier+1, highTierCoins) + "\n";
            rewardList.text = toPrint;
        }
        string RewardInfo(CoinType type, uint tier, uint amount) => $"<color={GlobalColor.Coin}>+{amount}<b> <color={GlobalColor.RarityTiers[tier]}>{Global.Rarities[tier]}</color> <color={GlobalColor.Perk.PerkType(type)}>{type}</color></b> coin(s)</color>";
    }
}