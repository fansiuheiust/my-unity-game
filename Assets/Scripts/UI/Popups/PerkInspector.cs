using Progression;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PerkInspector : MonoBehaviour {

        static readonly string CANNOTUPGRADE = "#FF0000", DEFAULTDEPENDENCYLINE = "#AAAAAA", EXCLUSION = "#FF0000", DEPENDENT = "#00FFFF";


        [SerializeField] PerkDisplay display;
        [SerializeField] GameObject treeView;
        [SerializeField] Button levelUpButton;
        [SerializeField] TextMeshProUGUI levelUpText;
        [SerializeField] GameObject dependencyLine;
        readonly Dictionary<string, PerkButton> perkButtons = new();
        readonly Dictionary<string, Dictionary<string, DependencyLine>> dependencyLines = new();
        readonly Dictionary<string, Dictionary<string, DependencyLine>> exclusionLines = new();
        Perk selectedPerk = null;
        private void Start() {
            if (treeView.transform.childCount != 1) throw new System.Exception("TreeView does not have perk tree as the only child");
            Transform perkTree = treeView.transform.GetChild(0);
            PerkButton[] buttons = perkTree.GetComponentsInChildren<PerkButton>();
            foreach (var b in buttons) {
                b.button.onClick.AddListener(()=>Select(b.perkID, b.perkType));
                perkButtons.Add(b.perkID, b);
            }
            if (ColorUtility.TryParseHtmlString(DEFAULTDEPENDENCYLINE, out Color c)) {
                c.a = GlobalColor.Perk.OutlineOpacity;
            } else {
                throw new System.Exception("Bad color");
            }

            foreach (var b in perkButtons.Values) {
                foreach (Dependency d in b.Perk.dependencies) {
                    if (!perkButtons.ContainsKey(d.id)) continue;
                    DependencyLine line = Instantiate(dependencyLine, perkTree.Find("Lines")).GetComponent<DependencyLine>();
                    line.From = perkButtons[d.id].GetComponent<RectTransform>().localPosition;
                    line.To = b.GetComponent<RectTransform>().localPosition;
                    line.Width = 10;
                    line.dependencyType = d.type;
                    line.Color = c;
                    if (!dependencyLines.ContainsKey(d.id))
                        dependencyLines.Add(d.id, new() { { b.perkID, line } });
                    else
                        dependencyLines[d.id].Add(b.perkID, line);
                }
                foreach (string x in b.Perk.exclusions) {
                    if (!perkButtons.ContainsKey(x)) continue;
                    DependencyLine line;
                    if (exclusionLines.ContainsKey(b.perkID) && exclusionLines[b.perkID].ContainsKey(x)) {
                        line = exclusionLines[b.perkID][x];
                    } else {
                        line = Instantiate(dependencyLine, perkTree.Find("Lines")).GetComponent<DependencyLine>();
                        line.From = perkButtons[x].GetComponent<RectTransform>().localPosition;
                        line.To = b.GetComponent<RectTransform>().localPosition;
                        line.Width = 10;
                        line.dependencyType = DependencyType.Existential;
                        line.Color = c;
                    }
                    if (!exclusionLines.ContainsKey(x)) {
                        exclusionLines.Add(x, new() { { b.perkID, line } });
                    } else
                        exclusionLines[x].Add(b.perkID, line);
                }
            }
            foreach (var b in perkButtons.Values) {
                UpdateDependencyLines(b.Perk);
                if (b.Perk.Level != 0)
                    UpdateExclusionLines(b.Perk);
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
            foreach (var b in perkButtons)
                b.Value.UpdatePerk(b.Value.Perk);
            perkButtons[p.id].UpdatePerk(p);
            Select(p.id, p.type);
            UpdateDependencyLines(p);
            UpdateExclusionLines(p);
        }
        void UpdateDependencyLines(Perk p) {
            string id = p.id;
            if (!dependencyLines.ContainsKey(id)) return;
            if (ColorUtility.TryParseHtmlString(DEPENDENT, out Color c)) {
                c.a = GlobalColor.Perk.OutlineOpacity;
            } else {
                throw new System.Exception("Bad color");
            }
            PerkTree tree = StageController.PlayerPerk.TreeOf(p.type);
            foreach (var l in dependencyLines[id]) {
                Perk dependentPerk = tree[l.Key];
                Dependency d = dependentPerk.dependencies.Where(x=>x.id == id).FirstOrDefault();
                if (tree.FulfilledDependency(dependentPerk, d))
                    l.Value.Color = c;
            }
        }
        void UpdateExclusionLines(Perk p) {
            string id = p.id;
            if (!exclusionLines.ContainsKey(id)) return;
            if (ColorUtility.TryParseHtmlString(EXCLUSION, out Color c)) {
                c.a = GlobalColor.Perk.OutlineOpacity;
            } else {
                throw new System.Exception("Bad color");
            }
            foreach (var l in exclusionLines[id].Values) {
                l.Color = c;
            }

        }
    }
}