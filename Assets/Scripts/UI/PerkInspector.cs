using Progression;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PerkInspector : MonoBehaviour {

        static readonly string CANNOTUPGRADE = "#FF0000";


        [SerializeField] PerkDisplay display;
        [SerializeField] GameObject treeView;
        [SerializeField] Button levelUpButton;
        [SerializeField] TextMeshProUGUI levelUpText;
        Dictionary<string, PerkButton> perkButtons = new();
        Perk selectedPerk = null;
        private void Start() {
            if (treeView.transform.childCount != 1) throw new System.Exception("TreeView does not have perk tree as the only child");
            Transform perkTree = treeView.transform.GetChild(0);
            PerkButton[] buttons = perkTree.GetComponentsInChildren<PerkButton>();
            foreach (var b in buttons) {
                b.button.onClick.AddListener(()=>Select(b.perkID, b.perkType));
                perkButtons.Add(b.perkID, b);
            }
            levelUpButton.onClick.AddListener(() => LevelUp(selectedPerk));
            NoPerk();
        }
        void Select(string perkID, CoinType perkType) {
            PerkTree tree = StageController.PlayerPerk.TreeOf(perkType);
            selectedPerk = tree[perkID];
            if (selectedPerk is null) {
                NoPerk();
                return;
            }
            display.Display(selectedPerk);
            bool fulfilledCost = StageController.PlayerPerk.CanAfford(selectedPerk), fulfilledDep = tree.FulfilledDependencies(selectedPerk), fulfilledExcl = tree.FulfilledExclusions(selectedPerk);
            if (fulfilledCost && fulfilledDep && fulfilledExcl) {
                levelUpButton.interactable = true;
                levelUpText.text = "Level Up";
            } else {
                levelUpButton.interactable = false;
                levelUpText.text = $"<color={CANNOTUPGRADE}>{(!fulfilledCost? "Insufficient Coins": !fulfilledDep? "Unfulfilled Requirements": "Conflicted Exclusion")}</color>";
            }
        }
        void NoPerk() {
            levelUpButton.interactable = false;
            levelUpText.text = "No perk selected";
            display.NoPerk();
        }

        void LevelUp(Perk p) {
            StageController.PlayerPerk.TryLevelUp(p.type, p.id);
            perkButtons[p.id].UpdatePerk(p);
            Select(p.id, p.type);
        }
    }
}