using UnityEngine;
using Dungeon.Generator;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Unity.VisualScripting;

namespace Dungeon {
    public class Builder2D : MonoBehaviour {
        Generator.Dungeon dungeon;
        [SerializeField]
        uint floor = 1;
        static readonly Dictionary<string, Vector2Int[]> shapeToKey = new Dictionary<string, Vector2Int[]> {
            { "1x1", new Vector2Int[]{} },
            { "1x2", new Vector2Int[]{new(1, 0)} },
            {"1x3", new Vector2Int[]{new(1,0), new(2,0) } },
            {"2x2", new Vector2Int[]{new(1,0), new(0, 1), new(1, 1) } },
            {"Plus", new Vector2Int[]{Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down } }
        };
        void Awake() {
            CompileDungeon();
            dungeon.Visualize();
            Build();
        }


        public void CompileDungeon() {
            List<(string, RoomType, Vector2Int[])> options = new();
            for (uint i = 0; i < StageController.DungeonData.NormalRoomShapeLength; i++) {
                options.Add((StageController.DungeonData.NormalRoomShapes(i), RoomType.Mob, shapeToKey[StageController.DungeonData.NormalRoomShapes(i)]));
            }
            // TODO: puzzle room
            // TODO: miniboss room
            dungeon = new(options.ToArray(), StageController.DungeonData.MainPathCounts(floor), new() { { RoomType.Mob, StageController.DungeonData.MobRoomCounts(floor) } }, false);
        }

        public void Build() {
            var rooms = dungeon.GeneratedRooms;
            var connections = dungeon.GeneratedConnections;
            uint id = 0;
            uint absLengthX = (uint)(dungeon.MaxCorner.x - dungeon.MinCorner.x+1), absLengthY = (uint)(dungeon.MaxCorner.y - dungeon.MinCorner.y+1);
            uint[,] grid = new uint[absLengthX, absLengthY]; // logging ids for wall spawning
            for (uint i = 0; i < absLengthX; i++) {
                for (uint j = 0; j < absLengthY; j++) {
                    grid[i, j] = (uint)rooms.Count; // count signals empty room
                }
            }
            foreach (var (type, shape, blocks, center, rotation) in rooms) {
                Vector3 spawnPos = (Vector3) center * (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness);
                
                foreach (var block in blocks) {
                    grid[block.x - dungeon.MinCorner.x, block.y - dungeon.MinCorner.y] = id; 
                }

                string path = $"Dungeon/Rooms/{type}/{shape}/";
                var candidates = Resources.LoadAll(path);
                GameObject spawnedRoom = (GameObject)Instantiate(candidates[Random.Range(0, candidates.Length-1)]);
                spawnedRoom.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0, rotation, 0));

                id++;

            }
        }


    }
}