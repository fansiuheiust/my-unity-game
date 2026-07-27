using Combat;
using Loot;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GearDisplay: MonoBehaviour {

        public static readonly string STATTEXT = "#FFFFFF", GOODSTAT = "#00FF00", BADSTAT = "#FF0000",
            ABILITYHEADER = "#AA00AA", ABILITYKEY = "#FF00FF", ABILITYDESC = "#FFFFFF", ABILITYATT = "#00FFFF", COOLDOWNTEXT = "#888888", COOLDOWNVALUE = "#FFFF00", MANACOSTTEXT = "#888888", MANACOSTVALUE = "#00BBBB";
        //                                  Common  Rare        Epic        Legendary   Mythical
        [SerializeField] TextMeshProUGUI gearName;
        [SerializeField] TextMeshProUGUI info;
        [SerializeField] Image outline, line;

        static string Stat(string name, float b, float s) {
            if (b == 0 && s == 0) return "";
            return $"<color={STATTEXT}>{name}</color><line-height=0em>\n<align=right>{(b > 0? $"<color={GOODSTAT}>+{b:F0}</color>": b < 0? $"<color={BADSTAT}>{b:F0}</color>" : "")}{(s > 0? $" <color={GOODSTAT}>+{s*100:F0}%</color>": s < 0? $" <color={BADSTAT}>{s * 100:F0}%</color>" : "")}</align></line-height>\n";
        }
        public static string Stats(BaseStats b, ScalingStats s) {
            string ri = $"{Stat("Attack", b.Atk, s.Atk)}{Stat("Defence", b.Def, s.Def)}{Stat("Max HP", b.MaxHp, s.MaxHp)}{Stat("Max Mana", b.MaxMana, s.MaxMana)}{Stat("Mana Regen", b.ManaRegen, s.ManaRegen)}";
            // hashed base stat goes here
            ri += $"\n{Stat("Walk Speed", 0, s.WalkSpeed)}{Stat("Attack Speed", 0, s.AtkSpeed)}{Stat("Crit Rate", 0, s.CritRate)}{Stat("Crit Damage", 0, s.CritDmg)}{Stat("Damage Reduction", 0, s.DmgReduction)}{Stat("Knockback", 0, s.Knockback)}{Stat("Knockback Resistance", 0, s.KnockbackResistance)}";
            foreach (ScalingAttribute h in Enum.GetValues(typeof(ScalingAttribute)))
                ri += $"{Stat(Global.HashedScalingStat(h), 0, s[h])}";

            return ri;
        }
        public static string Ability(Ability a) {
            string ri = $"<color={ABILITYHEADER}>Ability: {a.name}</color> <b><color={ABILITYKEY}>{Global.AbilityKey(a.triggerKey)}</color></b>\n<i><color={ABILITYDESC}>";
            for (int i =0; i < a.rawDescription.Length; i++) {
                if (a.rawDescription[i] == '{') {
                    int j = i + 1;
                    for (; j < a.rawDescription.Length; j++) {
                        if (a.rawDescription[j] == '}') break;
                    }
                    string key = a.rawDescription.Substring(i+1, j - i-1);
                    ri += $"<color={ABILITYATT}>{a.AttributeString(key)}</color>";
                    i = j;
                    continue;
                }
                ri += a.rawDescription[i];
            }
            ri += "</color></i>\n";
            if (a.Cooldown != 0) {
                ri += $"<color={COOLDOWNTEXT}>Cooldown: </color><color={COOLDOWNVALUE}>{a.Cooldown}s</color>\n";
            }
            if (a.ManaCost != 0) {
                ri += $"<color={MANACOSTTEXT}>Mana Cost: </color><color={MANACOSTVALUE}>{a.ManaCost}</color>\n";
            }
            return ri;
        }

        public static string GearName(Gear gear) => $"<color={GlobalColor.RarityTiers[gear.tier]}>{gear.name}</color>";

        /// <summary>
        /// Returns the TextMeshPro string for a gear
        /// </summary>
        /// <param name="gear">gear</param>
        /// <param name="gearType">type of gear, e.g. helmet, chestplate</param>
        public static string Gear(Gear gear, string gearType) {
            return $"{Stats(gear.@base, gear.scaling)}{(gear.ability is not null? Ability(gear.ability) : "")}\n\n<color={GlobalColor.RarityTiers[gear.tier]}><b>{Global.Rarities[gear.tier]} {gearType}</b></color>";
        }

        void Display(Gear gear, string gearType) {
            if (gear is null) {
                gearName.text = $"No {gearType}";
                info.text = $"You have not equipped {gearType}.";
                RecolorUI("#FFFFFF");
                return;
            }
            gearName.text = GearName(gear);
            info.text = Gear(gear, gearType);
            RecolorUI(GlobalColor.RarityTiers[gear.tier]);
        }
        void RecolorUI(string colorCode) {
            if (ColorUtility.TryParseHtmlString(colorCode, out Color c)) {
                outline.color = c;
                line.color = c;
            }
        }
        public void Display(string gearType) {
            foreach (ArmorType type in Enum.GetValues(typeof(ArmorType))) {
                if (type.ToString() == gearType) {
                    Display(StageController.Player.EquippedArmors[type], gearType);
                    return;
                }
            }
            if (gearType == "Weapon") {
                Display(StageController.Player.EquippedWeapon, gearType);
                return;
            }
            throw new System.Exception($"Invalid Gear type: {gearType}");
        }
    }
}
