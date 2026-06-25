using Progression;
using Progression.Balance;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UI {
    public class PerkButton : MonoBehaviour {
        [field: SerializeField, Tooltip("Reads the ID from the perk button's name if set to empty")] public string perkID { get; private set; }
        [field: SerializeField] public CoinType perkType { get; private set; }
        [SerializeField] Image[] topLefts, bottomRights;
        public Button button;
        public Perk Perk => StageController.PlayerPerk.TreeOf(perkType)[perkID];

        [SerializeField] TextMeshProUGUI text;
        private void Awake() {
            if (perkID == "")
                perkID = gameObject.name;
            if (perkType == CoinType.Floor)
                perkID += "_1";
        }
        private void Start() {
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

        public void UpdatePerk(Perk p) {
            text.text = p.name;
            if (ColorUtility.TryParseHtmlString(p.Level > 0 ? GlobalColor.Perk.TopLeftOutline : StageController.PlayerPerk.CanUnlock(p) ? GlobalColor.Perk.UnlockableTopLeftOutline : GlobalColor.Perk.LockedTopLeftOutline, out Color c)) {
                c.a = GlobalColor.Perk.OutlineOpacity;
                foreach (var item in topLefts)
                    item.color = c;
            } else {
                throw new System.Exception("Invalid color code");
            }
            if (ColorUtility.TryParseHtmlString(p.Level > 0 ? GlobalColor.Perk.BotRightOutline : StageController.PlayerPerk.CanUnlock(p)? GlobalColor.Perk.UnlockableBotRightOutline: GlobalColor.Perk.LockedBotRightOutline, out c)) {
                c.a = GlobalColor.Perk.OutlineOpacity;
                foreach (var item in bottomRights)
                    item.color = c;
            } else {
                throw new System.Exception("Invalid color code");
            }
        }

        public void AttachFloorNumber(string floorInString) {
            int lastUnderscore = perkID.LastIndexOf("_");
            perkID = perkID[0..(lastUnderscore+1)]+ floorInString;
            Start();
        }
    }
}