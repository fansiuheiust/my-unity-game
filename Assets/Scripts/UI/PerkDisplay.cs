using Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PerkDisplay : MonoBehaviour {
        [SerializeField] TextMeshProUGUI perkName, info;
        [SerializeField] Image[] topLefts, botRights;
        [SerializeField] Image line, outline;

        static readonly string INACTIVESTAT = "#AAAAAA", ACTIVESTAT = "#00FFFF", NOPERK = "#FFFFFF", ACTIVECOST = "#EECC00", INACTIVECOST = "#AAAAAA";
        /// <summary>
        /// Parsed description of a perk
        /// </summary>
        /// <param name="p">perk to parse description</param>
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

        /// <summary>
        /// Comprehensive information about the perk
        /// </summary>
        /// <param name="p">perk to extract info from</param>
        public static string Info(Perk p) {
            string ri = Description(p);
            ri += "\n\nLevel ";
            if (p.Level == 0)
                ri += $"<color={INACTIVESTAT}>0</color>/{p.maxLevel}\n";
            else
                ri += $"<color={ACTIVESTAT}>{p.Level}</color>/{p.maxLevel}\n";
            ri += $"Upgrade cost: <color={INACTIVECOST}>";
            for (uint l = 1; l <= p.maxLevel; l++) {
                // Note: I assume that Perk type dictates cost
                var (_, tier, amount) = p.CostAt(l);
                ri += (l-1 == p.Level)? $"<b><color={ACTIVECOST}>{amount} <color={GlobalColor.RarityTiers[tier]}>{Global.Rarities[tier]}</color></color></b>": $"{amount} {Global.Rarities[tier]}";
                if (l != p.maxLevel) ri += "/";
            }
            ri += $"</color>\n\n<color={GlobalColor.Perk.PerkType(p.type)}><b>{p.type} Perk</b></color>";
            return ri;
        }

        /// <summary>
        /// sets color of an array of images
        /// </summary>
        /// <param name="color">color in #ABCDEF</param>
        /// <param name="targets">images to change color</param>
        /// <exception cref="System.Exception"></exception>
        static void SetColor(string color, Image[] targets) {
            if (ColorUtility.TryParseHtmlString(color, out Color c)) {
                c.a = GlobalColor.Perk.OutlineOpacity;
                foreach (var item in targets)
                    item.color = c;
            } else throw new System.Exception("Bad color");
        }

        private void Start() {
            Display(StageController.PlayerPerk.FloorPerks["RoomSkipper1"]);
            // Display(null);
        }

        /// <summary>
        /// Set the PerkDisplay to display a perk
        /// </summary>
        /// <param name="p">perk to be displayed</param>
        public void Display(Perk p) {
            if (p is null) {
                NoPerk();
                return;
            }
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
                SetColor(GlobalColor.Perk.TopLeftOutline, topLefts);
                SetColor(GlobalColor.Perk.BotRightOutline, botRights);
            } else {
                // locked color
                SetColor(GlobalColor.Perk.LockedTopLeftOutline, topLefts);
                SetColor(GlobalColor.Perk.LockedBotRightOutline, botRights);
            }
        }
        
        /// <summary>
        /// change the panel to no perk
        /// </summary>
        public void NoPerk() {
            perkName.text = $"No Perk";
            info.text = "No perk has been selected yet.";
            ColorUtility.TryParseHtmlString(NOPERK, out Color c);
            line.color = c;
            outline.color = c;
            SetColor(GlobalColor.Perk.LockedTopLeftOutline, topLefts);
            SetColor(GlobalColor.Perk.LockedBotRightOutline, botRights);
        }
    }
}