using TMPro;
using UnityEngine;

namespace UI {
    public class PuzzleOutro : MonoBehaviour {
        [SerializeField]
        TextMeshProUGUI optimalityDisplay;

        int score, optimalScore;

        public float Optimality => (float)score / optimalScore;

        public void Init(int score, int optimalScore) {
            this.score = score;
            this.optimalScore = optimalScore;
            optimalityDisplay.text = $"{score}/{optimalScore} ({Optimality*100:F0}%)";
            optimalityDisplay.color = score * 2 >= optimalScore ? Color.green: Color.red;
        }
    }
}