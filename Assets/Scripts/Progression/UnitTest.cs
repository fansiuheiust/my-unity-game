
using Progression.Balance;
using System.Collections;
using UnityEngine;

namespace Progression {
    public static class UnitTest {
        /// <summary>
        /// Pass
        /// </summary>
        public static void TestPerkAbilityObject() {
            StageController.PlayerPerk.ClassPerks["SampleClassAbility"].LevelUp();
            Debug.Log("Test whether the ability behaves correctly");
        }
        /// <summary>
        /// Pass
        /// </summary>
        public static void TestPerkAbility() {
            Debug.Assert(AbilityDatabase.ContainsAbility("SampleClassAbility"), "whether ability database has sample perk ability");
            StageController.PlayerPerk.ClassPerks["SampleClassAbility"].LevelUp();
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility").ManaCost == 0, "whether default mana cost is used");
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility").Cooldown == 5, "whether cooldown is correct for level 1");
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility")["Power"] == 1, "whether power is correct for level 1");
            StageController.PlayerPerk.ClassPerks["SampleClassAbility"].LevelUp();
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility").ManaCost == 0, "whether default mana cost is used");
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility").Cooldown == 2, "whether cooldown is correct for level 2");
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility")["Power"] == 2, "whether power is correct for level 2");
            StageController.PlayerPerk.ClassPerks["SampleClassAbility"].LevelUp();
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility").ManaCost == 0, "whether default mana cost is used");
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility").Cooldown == 2, "whether cooldown is correct for level 3");
            Debug.Assert(AbilityDatabase.GetAbility("SampleClassAbility")["Power"] == 3, "whether power is correct for level 3");
        }

        /// <summary>
        /// Pass
        /// </summary>
        public static void TestAbilityDatabase() {
            Debug.Assert(AbilityDatabase.ContainsAbility("Speedy"));
            Debug.Assert(AbilityDatabase.GetAbility("Speedy").id == "Speedy");
            Debug.Assert(AbilityDatabase.GetAbility("Speedy").name == "Speedy");
            Debug.Assert(AbilityDatabase.GetAbility("Speedy")["Speed Boost"] == 0.5f);
            Debug.Assert(AbilityDatabase.GetAbility("Speedy")["Duration"] == 4f);
            Debug.Assert(AbilityDatabase.GetAbility("Speedy").abilityObject == typeof(Combat.Abilities.Speedy));
        }

        /// <summary>
        /// Pass, Perform this with LoadFromSave off
        /// </summary>
        public static void TestFloorPerkDuplication() {
            PerkTree floorPerks = StageController.PlayerPerk.FloorPerks;
            // test ID change and name change
            Debug.Assert(!floorPerks.Contains("RoomSkipper"));// it should not exist
            Debug.Assert(!floorPerks.Contains("Rebuff"));
            Debug.Assert(!floorPerks.Contains("UltimateSkipper"));

            for (int i = 1; i <= 9; i++) {
                Debug.Assert(floorPerks.Contains($"RoomSkipper{i}"));
                Debug.Assert(floorPerks[$"RoomSkipper{i}"].name == $"Room Skipper (floor {i})");
                Debug.Assert(floorPerks.Contains($"Rebuff{i}"));
                Debug.Assert(floorPerks.Contains($"UltimateSkipper{i}"));

                // test if dependencies and exclusions are correctly updated
                Debug.Assert(floorPerks[$"Scavenge{i}"].dependencies[0].id == $"RoomSkipper{i}");
                Debug.Assert(floorPerks[$"Scavenge{i}"].dependencies[0].type == DependencyType.Existential);
                Debug.Assert(floorPerks[$"Scaler{i}"].exclusions[0] == $"RoomSkipper{i}");

                // test cloning of stats
                floorPerks.LevelUp($"RoomSkipper{i}");
                Debug.Assert(Mathf.Abs(floorPerks[$"RoomSkipper{i}"]["Room Reduction"]-0.1f) < 0.001f);
                floorPerks.LevelUp($"RoomSkipper{i}");
                Debug.Assert(floorPerks[$"RoomSkipper{i}"]["Room Reduction"] == 0.25f);
                floorPerks.LevelUp($"RoomSkipper{i}");
                Debug.Assert(floorPerks[$"RoomSkipper{i}"]["Room Reduction"] == 0.5f);
                floorPerks.LevelUp($"RoomSkipper{i}");
                Debug.Assert(floorPerks[$"RoomSkipper{i}"]["Room Reduction"] == 1f);

                // test cost
                var (_, tier, value) = floorPerks[$"RoomSkipper{i}"].CostAt(2);
                Debug.Assert(tier == (i-1)/2);
                Debug.Assert(value == (i % 2 == 1? 6: 12));
            }
        }
        /// <summary>
        /// Pass
        /// </summary>
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
        /// <summary>
        /// Pass
        /// </summary>
        public static void TestPerkTreeLoad() {
            Debug.Assert(StageController.PlayerPerk.FloorPerks["RoomSkipper"].Level == 4);
            Debug.Assert(StageController.PlayerPerk.FloorPerks["Scavenge"].Level == 4);
            Debug.Assert(StageController.PlayerPerk.FloorPerks["Rebuff"].Level == 1);
        }
        /// <summary>
        /// Pass
        /// </summary>
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

            Perk p = new("tester", "Tester", "Lorem {Targets} Ipsum {Raw damage} {Bonus damage}", new Stats(
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