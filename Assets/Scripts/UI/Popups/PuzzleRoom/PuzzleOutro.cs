using TMPro;
using UnityEngine;

namespace UI {
    public class PuzzleOutro : MonoBehaviour {
        [SerializeField]
        TextMeshProUGUI optimalityDisplay, eligableRewards;

        int score, optimalScore;

        public float Optimality => (float)score / optimalScore;

        public void Init(int score, int optimalScore, bool coinEligable, bool buffEligable, bool gearEligable) {
            this.score = score;
            this.optimalScore = optimalScore;
            optimalityDisplay.text = $"{score}/{optimalScore} ({Optimality*100:F0}%)";
            optimalityDisplay.color = score * 2 >= optimalScore ? Color.green: Color.red;
            string toPrint;
            if (coinEligable || buffEligable || gearEligable) {
                toPrint = "You are eligable for:\n<b>";
                if (coinEligable)
                    toPrint += $"<color={GlobalColor.Coin}>Coin</color>\n";
                if (buffEligable)
                    toPrint += $"<color={GlobalColor.Buff}>Buff</color>\n";
                if (gearEligable)
                    toPrint += $"<color={GlobalColor.Gear}>Gear</color>\n";
                toPrint += "</b>";
            } else {
                toPrint = "You are not eligable for any rewards.";
            }
            eligableRewards.text = toPrint;
        }
    }
}