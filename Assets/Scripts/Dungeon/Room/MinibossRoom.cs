using BuildingBlocks;
using Combat;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon {
    public class MinibossRoom : Room {
        [SerializeField, Tooltip("Outer nest: floor since it is availabe; Inner nest: the list of bosses for that floor")]
        BossArray[] bosses;
        [SerializeField]
        MinibossChooser minibossChooser;

        [SerializeField]
        GameObject wallBlockerPrefab;

        List<GameObject> wallBlockers = new();

        void Awake() {
            List<string> bossCandidate = new();
            for (int i = 1; i < bosses.Length && i <= StageController.Floor; i++) {
                bossCandidate.AddRange(bosses[i].bosses);
            }
            minibossChooser.bossID = bossCandidate[Random.Range(0, bossCandidate.Count)];

            minibossChooser.onMobSpawn += OnMinibossSpawned;
            minibossChooser.onStart += OnMinibossStarted;
        }

        void OnMinibossSpawned(Mob m) {
            m.OnDeath.AddListener(OnMinibossDied);
        }

        void OnMinibossDied(Mob _, Mob __) {
            if (Cleared) return;
            foreach (var w in wallBlockers)
                Destroy(w);
            wallBlockers = null;
            Cleared = true;
        }


        void OnMinibossStarted() {
            // block the walls

            // front
            GameObject wall = Instantiate(wallBlockerPrefab);
            wall.transform.position = transform.position + (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness) / 2 * Vector3.forward;
            wall.transform.localEulerAngles = new Vector3(0, 90, 0);
            wallBlockers.Add(wall);
            // back
            wall = Instantiate(wallBlockerPrefab);
            wall.transform.position = transform.position - (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness) / 2 * Vector3.forward;
            wall.transform.localEulerAngles = new Vector3(0, 90, 0);
            wallBlockers.Add(wall);
            // left
            wall = Instantiate(wallBlockerPrefab);
            wall.transform.position = transform.position - (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness) / 2 * Vector3.right;
            wallBlockers.Add(wall);
            // right
            wall = Instantiate(wallBlockerPrefab);
            wall.transform.position = transform.position + (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness) / 2 * Vector3.right;
            wallBlockers.Add(wall);
        }
    }

    [System.Serializable]
    class BossArray {
        public string[] bosses;
    }
}