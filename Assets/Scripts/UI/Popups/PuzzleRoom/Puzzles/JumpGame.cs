using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Puzzle {
    public class JumpGame : PuzzlePopup {
        static readonly string ValueColor = "#0ff", ActiveTower = "#0a0", OtherTower = "#fff";
        [SerializeField]
        GameObject towerButtonPrefab;
        [SerializeField]
        int pixelsPerHeight = 50, buttonWidth = 100;
        [SerializeField]
        RectTransform buttonContainer;
        [SerializeField]
        TextMeshProUGUI jumpRangeDisplay, numJumpsDisplay, noJumpDisplay;

        // variables
        [SerializeField, Min(1)]
        int minTowers = 8, maxTowers = 12;
        [SerializeField, Min(1)]
        int minJumpRange = 1, maxJumpRange = 5;
        [SerializeField, Min(1)]
        int minOptimalJumps= 3, maxOptimalJumps = 8;

        Button[] towers;
        int[] heights;
        int[] optimalJumps;

        int _currTower;

        int CurrTower {
            get => _currTower;
            set {
                towers[CurrTower].GetComponent<Image>().color = GlobalColor.Parse(OtherTower);
                towers[value].GetComponent<Image>().color = GlobalColor.Parse(ActiveTower);
                _currTower = value;
                UpdateTowerInteractability();
            }
        }

        int optimalJump;
        int jumpRange;

        int _numJumps;

        int NumJumps {
            get => _numJumps;
            set {
                _numJumps = value;
                numJumpsDisplay.text = $"Jumps: <color={ValueColor}>{value}</color>";
            }
        }

        protected override void Awake() {
            base.Awake();
            NumJumps = 0;
            noJumpDisplay.alpha = 0;
            optimalJump = Random.Range(minOptimalJumps, maxOptimalJumps);
            int pass = 0;
            Regenerate:
            CreateButtons();
            
            jumpRange = Random.Range(minJumpRange, maxJumpRange);
            jumpRangeDisplay.text = $"Jump range: <color={ValueColor}>{jumpRange}</color>";


            // find the solution
            List<int> validIndices = new();
            for (int i = 0; i < towers.Length; i++) {
                int j = OptimalJumps(i);
                if (j == optimalJump) validIndices.Add(i);
            }
            if (validIndices.Count == 0) {
                foreach (Button b in towers) {
                    b.onClick.RemoveAllListeners();
                    Destroy(b.gameObject);
                }
                pass++;
                if (pass%100 == 0) {
                    Debug.Log("Too many failures, subtracting optimal jumps by 1");
                    optimalJump--;
                }
                goto Regenerate;
            }
            // position the player on a tower that can do at least 1 jump
            CurrTower = validIndices[Random.Range(0, validIndices.Count-1)];
        }

        void CreateButtons() {
            int numTowers = Random.Range(minTowers, maxTowers);
            towers = new Button[numTowers];
            heights = new int[numTowers];
            optimalJumps = new int[numTowers];
            System.Array.Fill(optimalJumps, -1);
            for (int i = 0; i < numTowers; i++)
                heights[i] = i + 1;
            for (int i = numTowers-1; i > 0; i--) {
                int k = Random.Range(0, i);
                (heights[k], heights[i]) = (heights[i], heights[k]);
            }
            for (int i = 0; i < numTowers; i++) {
                towers[i] = Instantiate(towerButtonPrefab, buttonContainer).GetComponent<Button>();
                ((RectTransform)towers[i].transform).sizeDelta = new Vector2(buttonWidth, pixelsPerHeight * heights[i]);
                towers[i].GetComponentInChildren<TextMeshProUGUI>().text = heights[i].ToString();
                towers[i].GetComponent<Image>().color = GlobalColor.Parse(OtherTower);
                int copy = i;
                towers[i].onClick.AddListener(() => OnButtonClick(copy));
            }
            buttonContainer.sizeDelta = new Vector2(buttonWidth * numTowers, 100);
        }

        /// <summary>
        /// Makes unreachable towers non-interactable
        /// </summary>
        /// <returns>number of interactable towers</returns>
        int UpdateTowerInteractability() {
            int ri = 0;
            for (int i = 0; i < towers.Length; i++)
                towers[i].interactable = false;
            for (int j = 1; j <= jumpRange; j++) {
                if (CurrTower + j >= towers.Length || heights[CurrTower + j] >= heights[CurrTower]) break;
                towers[CurrTower + j].interactable = true;
                ri++;
            }
            for (int j = 1; j <= jumpRange; j++) {
                if (CurrTower - j < 0 || heights[CurrTower - j] >= heights[CurrTower]) break;
                towers[CurrTower - j].interactable = true;
                ri++;
            }
            return ri;
        }

        int OptimalJumps(int i) {
            if (optimalJumps[i] != -1) return optimalJumps[i];
            optimalJumps[i] = 0;
            for (int j = 1; j <= jumpRange; j++) {
                if (i + j >= towers.Length || heights[i+j] >= heights[i]) break;
                int jumps = OptimalJumps(i + j)+1;
                if (jumps > optimalJumps[i])
                    optimalJumps[i] = jumps;
            }
            for (int j = 1; j <= jumpRange; j++) {
                if (i - j < 0 || heights[i-j] >= heights[i]) break;
                int jumps = OptimalJumps(i - j) + 1;
                if (jumps > optimalJumps[i])
                    optimalJumps[i] = jumps;
            }
            return optimalJumps[i];
        }

        

        void OnButtonClick(int i) {
            NumJumps++;
            CurrTower = i;
            if (UpdateTowerInteractability() == 0) {
                GameOver();
            }
        }

        void GameOver() {
            foreach (Button b in towers) {
                b.onClick.RemoveAllListeners();
            }
            noJumpDisplay.alpha = 1;
            Clear(NumJumps, optimalJump);
        }
    }
}