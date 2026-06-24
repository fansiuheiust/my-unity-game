using Progression;
using UnityEngine;

namespace UI {
    public static class GlobalColor {
        public static readonly string[] RarityTiers = { "#FFFFFF", "#00BB00", "#AA00AA", "#EECC00", "#FF00FF" };
        public static class Perk {
            public static string PerkType(CoinType type) => type switch {
                CoinType.Floor => "#00FF00",
                CoinType.RNG => "#AA00FF",
                CoinType.Class => "#DDEE00",
                _ => throw new System.NotImplementedException("Please implement color in GlobalColor for perk type " + type.ToString())
            };
            public static readonly string TopLeftOutline = "#FF0000", BotRightOutline = "#00FFFF", LockedTopLeftOutline = "#555555", LockedBotRightOutline = "#AAAAAA";
            public static readonly float OutlineOpacity = 0.5f;
        }
    }
}