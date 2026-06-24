using Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PerkDisplay : MonoBehaviour {
        [SerializeField] TextMeshProUGUI perkName, info;
        [SerializeField] Image[] topLefts, botRights;
        [SerializeField] Image line, outline;

        static readonly string INACTIVESTAT = "#AAAAAA", ACTIVESTAT = "#00FFFF";
        public static string Description(Perk p) {
            string ri = "";
            for (int i = 0; i < p.rawDescription.Length; i++) {
                if (p.rawDescription[i] == '{') {
                    ri += $"<color={INACTIVESTAT}>";
                    int j = i + 1;
                    for (; j < p.rawDescription.Length; j++)
                        if (p.rawDescription[j] == '}') break;
                    string key = p.rawDescription.Substring(i + 1, j - i - 1);
                    for (uint k = 1; k <= p.maxLevel; k++) {
                        ri += k == p.Level ? $"<color={ACTIVESTAT}>{p.AttributeString(key, k)}</color>": p.AttributeString(key, k);
                        if (k != p.maxLevel) ri += "/";
                    }
                    ri += "</color>";
                    i = j;
                    continue;
                }
                ri += p.rawDescription[i];
            }
            return ri+"\n";
        }

        public static string Info(Perk p) {
            string ri = Description(p);
            ri += "\n\nLevel ";
            if (p.Level == 0)
                ri += $"<color={INACTIVESTAT}>0</color>/{p.maxLevel}\n\n";
            else
                ri += $"<color={ACTIVESTAT}>{p.Level}</color>/{p.maxLevel}\n\n";
            ri += $"<color={GlobalColor.Perk.PerkType(p.type)}><b>{p.type} Perk</b></color>";
            return ri;
        } 

        private void Start() {
            Display(StageController.PlayerPerk.FloorPerks["RoomSkipper1"]);
        }
        public void Display(Perk p) {
            perkName.text = $"<color={GlobalColor.Perk.PerkType(p.type)}>{p.name}</color>";
            info.text = Info(p);
            
            // outline color
            if (ColorUtility.TryParseHtmlString(GlobalColor.Perk.PerkType(p.type), out Color c)) {
                line.color = c;
                outline.color = c;
            } else {
                throw new System.Exception("Bad color");
            }

            
            if (p.Level != 0) {
                // unlocked color
                SetOutline(GlobalColor.Perk.TopLeftOutline, topLefts);
                SetOutline(GlobalColor.Perk.BotRightOutline, botRights);
            } else {
                // locked color
                SetOutline(GlobalColor.Perk.LockedTopLeftOutline, topLefts);
                SetOutline(GlobalColor.Perk.LockedBotRightOutline, botRights);
            }

            void SetOutline(string color, Image[] targets) {
                if (ColorUtility.TryParseHtmlString(color, out c)) {
                    c.a = GlobalColor.Perk.OutlineOpacity;
                    foreach (var item in targets)
                        item.color = c;
                } else throw new System.Exception("Bad color");
            }
        }
    }
}