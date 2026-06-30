using NUnit.Framework.Internal.Commands;
using Progression;
using Progression.Balance;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PerkDisplay : MonoBehaviour {
        [SerializeField] TextMeshProUGUI perkName, info;
        [SerializeField] Image[] topLefts, botRights;
        [SerializeField] Image line, outline;

        static readonly string INACTIVE = "#AAAAAA", ACTIVESTAT = "#00FFFF", NOPERK = "#FFFFFF",
            UNFULFILLED = "#888888", FULFILLED = "#00FF00", ACTIVECOST = "#EECC00", ACTIVEDEP = "#00FFFF", ACTIVEEXCL = "#FF0000";

        static readonly Dictionary<string, Func<Perk, string, uint, bool, string>> Commands = new() {
            { "Rarity", (p, key, level, active)=> {
                uint tier = (uint) p.Attribute(key, level);
                return active? $"<color={GlobalColor.RarityTiers[tier]}><b>{Global.Rarities[tier]}</b></color>": Global.Rarities[tier];
            } },
            { "Cooldown", (p, key, level, active) => active? $"<color={GearDisplay.COOLDOWNVALUE}>{p.AttributeString(key, level)}</color>": p.AttributeString(key, level) },
            { "ManaCost", (p, key, level, active) => active? $"<color={GearDisplay.MANACOSTVALUE}>{p.AttributeString(key, level)}</color>": p.AttributeString(key, level) }
        };

        static string Attribute(Perk p, string key, string command, uint level, bool active) {
            string ri = "";
            if (command != "")
                ri += Commands[command](p, key, level, active);
            else
                ri += active ? $"<color={ACTIVESTAT}>{p.AttributeString(key, level)}</color>" : p.AttributeString(key, level);
            return ri;
        }
        static string Stats(Perk p, string key, string command) {
            string ri = "";
            if (p.IsConstantAttribute(key)) {
                ri += Attribute(p, key, command, 1, p.Level > 0);
            } else {
                for (uint k = 1; k <= p.maxLevel; k++) {
                    ri += Attribute(p, key, command, k, p.Level == k);
                    if (k != p.maxLevel) ri += "/";
                }
            }
            return ri;
        }

        /// <summary>
        /// Parsed description of a perk
        /// </summary>
        /// <param name="p">perk to parse description</param>
        public static string Description(Perk p) {
            string ri = "";
            for (int i = 0; i < p.rawDescription.Length; i++) {
                if (p.rawDescription[i] == '{') {
                    ri += $"<color={INACTIVE}>";
                    int j = i + 1;
                    int commandStart = -1;
                    int keyStart = i+1;
                    for (; j < p.rawDescription.Length; j++) {                // 0123456789012
                        if (p.rawDescription[j] == '[') commandStart = j + 1; // [Rarity]Tier}
                        if (p.rawDescription[j] == ']') keyStart = j+1;
                        if (p.rawDescription[j] == '}') break;
                    }
                    string key = p.rawDescription.Substring(keyStart, j - keyStart), command = (commandStart != -1)? p.rawDescription.Substring(commandStart, keyStart - commandStart +1): "";
                    ri += Stats(p, key, command);
                    ri += "</color>";
                    i = j;
                    continue;
                }
                ri += p.rawDescription[i];
            }
            ri += "\n";
            if (AbilityDatabase.ContainsAbility(p.id)) {
                ri += $"\n<color={GearDisplay.ABILITYHEADER}><b>Perk with Ability</b></color>\n";
                if (p.ContainsAttribute("Cooldown")) {
                    ri += $"<color={GearDisplay.COOLDOWNTEXT}>Cooldown: </color><color={INACTIVE}>" + Stats(p, "Cooldown", "Cooldown") + "</color>\n";
                }
                if (p.ContainsAttribute("Mana Cost")) {
                    ri += $"<color={GearDisplay.MANACOSTTEXT}>Mana Cost: </color><color={INACTIVE}>" + Stats(p, "Mana Cost", "ManaCost") + "</color>\n";
                }
                ri += "\n";
            }
            return ri;
        }

        public static string LevelInfo(Perk p) {
            string ri = "Level ";
            if (p.Level == 0)
                ri += $"<color={INACTIVE}>0</color>/{p.maxLevel}";
            else
                ri += $"<color={ACTIVESTAT}>{p.Level}</color>/{p.maxLevel}";
            ri += $"\n\n<color={(StageController.PlayerPerk.CanAfford(p)? FULFILLED: UNFULFILLED)}>Upgrade Cost:</color>\n<color={INACTIVE}>";
            for (uint l = 1; l <= p.maxLevel; l++) {
                // Note: I assume that Perk type dictates cost
                var (_, tier, amount) = p.CostAt(l);
                ri += (l - 1 == p.Level) ? $"<b><color={ACTIVECOST}>{amount} <color={GlobalColor.RarityTiers[tier]}>{Global.Rarities[tier]}</color></color></b>" : $"{amount} {Global.Rarities[tier]}";
                if (l != p.maxLevel) ri += "/";
            }
            ri += $"\n\n";
            PerkTree tree = StageController.PlayerPerk.TreeOf(p.type);
            if (p.dependencies.Length != 0) {
                ri += $"<color={(tree.FulfilledDependencies(p)? FULFILLED: UNFULFILLED)}>Required Perks:</color>\n";
                foreach (Progression.Dependency d in p.dependencies) {
                    ri += $"<color={(tree.FulfilledDependency(p, d)? ACTIVEDEP : INACTIVE)}>{tree[d.id].name}: "+(d.type switch {
                        DependencyType.Existential => "Unlocked",
                        DependencyType.Max => "Maxed",
                        DependencyType.Levelled => "Same Level",
                        _ => throw new System.NotImplementedException($"Unimplemented dependency {d.type}")
                    })+"</color>\n";
                }
                ri += "\n";
            }
            if (p.exclusions.Length != 0) {
                ri += $"<color={(tree.FulfilledExclusions(p)? FULFILLED: UNFULFILLED)}>Exclusions:</color>\n";
                foreach (string e in p.exclusions) {
                    ri += $"<color={(tree.FulfilledExclusion(e)? INACTIVE: ACTIVEEXCL)}>{tree[e].name}</color>\n";
                }
            }
            return ri;
        }

        /// <summary>
        /// Comprehensive information about the perk
        /// </summary>
        /// <param name="p">perk to extract info from</param>
        public static string Info(Perk p) {
            string ri = Description(p);
            ri += $"\n\n<color={GlobalColor.Perk.PerkType(p.type)}><b>{p.type} Perk</b></color>\n\n";
            ri += LevelInfo(p);
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