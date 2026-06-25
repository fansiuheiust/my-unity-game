
using Progression.Balance;
using System.Collections;
using UnityEngine;

namespace Progression {
    public static class UnitTest {
        static bool Close(float a, float b, float epsilon = 1E-4f) => Mathf.Abs(a - b) < epsilon;
        /// <summary>
        /// Pass
        /// </summary>
        public static void TestLevelGearScaling() {
            Combat.Player player = StageController.Player;
            player.Equip(Loot.GearDatabase.Get("dagger"));
            float atk = player.EquippedWeapon.@base.Atk;
            uint pointRequired = (uint)StageController.LevelingData.LevelCurve.Evaluate(1);
            player.Level.AddPoint(pointRequired+1);
            player.Equip(Loot.GearDatabase.GetScaled("dagger"));
            Debug.Assert(player.Level.Level == 1, "Player level not levelled correctly");
            Debug.Assert(Close(atk * StageController.LevelingData.ItemBaseStatsMultiplier.Evaluate(1), player.EquippedWeapon.@base.Atk), "Dagger's attack not changed correctly");
            for (int i = 1; i < StageController.LevelingData.MaxLevel; i++) {
                player.Level.AddPoint((uint)StageController.LevelingData.LevelCurve.Evaluate(i+1));
                player.Equip(Loot.GearDatabase.GetScaled("dagger"));
                Debug.Assert(player.Level.Level == i+1, $"Player level not levelled correctly to level {i+1}");
                Debug.Assert(Close(atk * StageController.LevelingData.ItemBaseStatsMultiplier.Evaluate(i+1), player.EquippedWeapon.@base.Atk), $"Dagger's attack not changed correctly for level {i+1}");
            }
        }

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
                Debug.Assert(floorPerks.Contains($"RoomSkipper_{i}"));
                Debug.Assert(floorPerks[$"RoomSkipper_{i}"].name == $"Room Skipper (floor {i})");
                Debug.Assert(floorPerks.Contains($"Rebuff_{i}"));
                Debug.Assert(floorPerks.Contains($"UltimateSkipper_{i}"));

                // test if dependencies and exclusions are correctly updated
                Debug.Assert(floorPerks[$"Scavenge_{i}"].dependencies[0].id == $"RoomSkipper_{i}");
                Debug.Assert(floorPerks[$"Scavenge_{i}"].dependencies[0].type == DependencyType.Existential);
                Debug.Assert(floorPerks[$"Scaler_{i}"].exclusions[0] == $"RoomSkipper_{i}");

                // test cloning of stats
                floorPerks.LevelUp($"RoomSkipper_{i}");
                Debug.Assert(Mathf.Abs(floorPerks[$"RoomSkipper_{i}"]["Room Reduction"]-0.1f) < 0.001f);
                floorPerks.LevelUp($"RoomSkipper_{i}");
                Debug.Assert(floorPerks[$"RoomSkipper_{i}"]["Room Reduction"] == 0.25f);
                floorPerks.LevelUp($"RoomSkipper_{i}");
                Debug.Assert(floorPerks[$"RoomSkipper_{i}"]["Room Reduction"] == 0.5f);
                floorPerks.LevelUp($"RoomSkipper_{i}");
                Debug.Assert(floorPerks[$"RoomSkipper_{i}"]["Room Reduction"] == 1f);

                // test cost
                var (_, tier, value) = floorPerks[$"RoomSkipper_{i}"].CostAt(2);
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
                new("a", "A", "A", new Stats(new Attribute[0]), CoinType.Floor, new (uint, uint)[]{ (0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0), }, 10, new Dependency[0], new string[0]),
                new("b", "B", "B", new Stats(new Attribute[0]), CoinType.Floor, new (uint, uint)[]{(0, 0),(0, 0),(0, 0),(0, 0), }, 4, new Dependency[]{new("a", DependencyType.Existential) }, new string[0]),
                new("c", "C", "C", new Stats(new Attribute[0]), CoinType.Floor, new (uint, uint)[]{(0, 0),(0, 0),(0, 0), }, 3, new Dependency[]{new("a", DependencyType.Max) }, new string[0]),
                new("d", "D", "D", new Stats(new Attribute[0]), CoinType.Floor, new (uint, uint)[]{(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0),(0, 0), }, 8, new Dependency[]{new("b", DependencyType.Levelled), new("c", DependencyType.Levelled) }, new string[0]),
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