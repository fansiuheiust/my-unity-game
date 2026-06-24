using Progression;
using Progression.Balance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PerkButton : MonoBehaviour {
        static readonly string LOCKEDCOLORTOPLEFT = "#555555", LOCKEDCOLORBOTRIGHT = "#AAAAAA", UNLOCKEDCOLORTOPLEFT = "#FF0000", UNLOCKEDCOLORBOTRIGHT = "#00FFFF";
        static readonly float OPACITY = 0.5f;
        [SerializeField, Tooltip("Reads the ID from the perk button's name if set to empty")] string perkID;
        [SerializeField] CoinType perkType;
        [SerializeField] Image[] topLefts, bottomRights;

        [SerializeField] TextMeshProUGUI text;
        private void Start() {
            if (perkID == "")
                perkID = gameObject.name;
            PerkTree tree = perkType switch {
                CoinType.Floor => StageController.PlayerPerk.FloorPerks,
                CoinType.RNG => StageController.PlayerPerk.RNGPerks,
                _ => StageController.PlayerPerk.ClassPerks
            };
            if (tree.Contains(perkID))
                UpdatePerk(tree[perkID]);
            else
                throw new System.Exception($"No {perkType} perk with ID {perkID}");
        }

        void UpdatePerk(Perk p) {
            text.text = p.name;
            foreach (var item in topLefts) {
                if (ColorUtility.TryParseHtmlString(p.Level > 0 ? UNLOCKEDCOLORTOPLEFT : LOCKEDCOLORTOPLEFT, out Color c)) {
                    c.a = OPACITY;
                    item.color = c;
                } else {
                    throw new System.Exception("Invalid color code");
                }
            }
            foreach (var item in bottomRights) {
                if (ColorUtility.TryParseHtmlString(p.Level > 0 ? UNLOCKEDCOLORBOTRIGHT : LOCKEDCOLORBOTRIGHT, out Color c)) {
                    c.a = OPACITY;
                    item.color = c;
                } else {
                    throw new System.Exception("Invalid color code");
                }
            }
        }
    }
}