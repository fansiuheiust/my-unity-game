
using System.Collections;
using UnityEngine;

namespace Progression {
    public static class UnitTest {
        public static void TestPerkTreeSave() {
            StageController.PlayerPerk.FloorPerks.LevelUp("RoomSkipper");
            StageController.PlayerPerk.FloorPerks.LevelUp("RoomSkipper");
            StageController.PlayerPerk.FloorPerks.LevelUp("Scavenge");
            StageController.PlayerPerk.FloorPerks.LevelUp("Scavenge");
            StageController.PlayerPerk.FloorPerks.LevelUp("Scavenge");
            StageController.PlayerPerk.FloorPerks.LevelUp("Rebuff"); // does not proc due to scavenge max dependency
            StageController.PlayerPerk.FloorPerks.LevelUp("Scavenge");
            StageController.PlayerPerk.FloorPerks.LevelUp("RoomSkipper");
            StageController.PlayerPerk.FloorPerks.LevelUp("RoomSkipper");
            StageController.PlayerPerk.FloorPerks.LevelUp("Rebuff");
            var levelData = StageController.PlayerPerk.FloorPerks.PerkData;
            foreach (var (key, level) in levelData) {
                switch (key) {
                    case "RoomSkipper":
                        Debug.Assert(level == 4);
                        break;
                    case "Scavenge":
                        Debug.Assert(level == 4);
                        break;
                    case "Rebuff":
                        Debug.Assert(level == 1);
                        break;
                }
            }
        }
        public static void TestPerkTreeLoad() {
            Debug.Assert(StageController.PlayerPerk.FloorPerks["RoomSkipper"].Level == 4);
            Debug.Assert(StageController.PlayerPerk.FloorPerks["Scavenge"].Level == 4);
            Debug.Assert(StageController.PlayerPerk.FloorPerks["Rebuff"].Level == 1);
        }
        public static void TestDependencyExclusion() {
            Perk[] perks = new Perk[] {
                new("a", 10),
                new("b", 4, new Dependency("a", DependencyType.Existential)),
                new("c", 3, new Dependency("a", DependencyType.Max)),
                new("d", 8, new Dependency("b", DependencyType.Levelled), new Dependency("c", DependencyType.Levelled)),
            };
            PerkTree pt = new(perks);
            Debug.Assert(pt.Unlockable("b") == false && pt.Unlockable("c") == false);
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            Debug.Assert(pt.Unlockable("b") == true && pt.Unlockable("c") == false);
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            Debug.Assert(pt.Unlockable("b") == true && pt.Unlockable("c") == true);

            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("b");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("c");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("c");
            pt.LevelUp("c");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("b");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("b");
            pt.LevelUp("b");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == false);

            Perk p = new("tester", "Tester", "Lorem {Targets} Ipsum {Raw damage} {Bonus damage}", new PerkStats(
                new IntAttribute("Targets", 3, 5, 10),
                new DecimalAttribute("Raw damage", 12f, 15.5f, 22.77f),
                new PercentageAttribute("Bonus damage", 0.1f, 0.3f, 0.6f)
                ), CoinType.Floor,
                new (uint, uint)[3] {
                    (0, 3),
                    (0, 9),
                    (1, 4)
                }, 3,
                new Dependency[] { new Dependency("prereq", DependencyType.Existential) }, new string[0]);
        }
    }
}