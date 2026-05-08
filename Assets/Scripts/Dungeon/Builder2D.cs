using UnityEngine;
using Dungeon.Generator;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Unity.VisualScripting;
using Progression.Balance;

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
            uint absLengthX = (uint)(dungeon.MaxCorner.x - dungeon.MinCorner.x+1), absLengthZ = (uint)(dungeon.MaxCorner.z - dungeon.MinCorner.z+1);
            uint[,] grid = new uint[absLengthX, absLengthZ]; // logging ids for wall spawning
            for (uint i = 0; i < absLengthX; i++) {
                for (uint j = 0; j < absLengthZ; j++) {
                    grid[i, j] = (uint)rooms.Count; // count signals empty room
                }
            }
            List<Room> generatedRooms = new();
            foreach (var (type, shape, blocks, center, rotation) in rooms) {
                
                foreach (var block in blocks) {
                    grid[block.x - dungeon.MinCorner.x, block.z - dungeon.MinCorner.z] = id; 
                }

                Vector3 spawnPos = (Vector3)center * (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness);

                string path = $"Dungeon/Rooms/{type}/{shape}/";
                var candidates = Resources.LoadAll(path);
                GameObject spawnedRoom = (GameObject)Instantiate(candidates[Random.Range(0, candidates.Length-1)]);
                spawnedRoom.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0, -rotation, 0));
                generatedRooms.Add(spawnedRoom.GetComponent<Room>());

                id++;

            }
            for (uint i = 0; i < absLengthX; i++) {
                for (uint j = 0; j < absLengthZ; j++) {
                    // vertical placement - edge of the dungeon
                    if ((j == 0 || j == absLengthZ-1) && grid[i, j] != rooms.Count) {
                        BuildWall((new Vector3(i, 0,j + (j==0 ? -0.5f: 0.5f))+dungeon.MinCorner)*(StageController.DungeonData.RoomLength+StageController.DungeonData.WallThickness), false, true);
                    }
                    // horizontal placement - edge of the dungeon
                    if ((i == 0 || i == absLengthX-1) && grid[i, j] != rooms.Count) {
                        BuildWall((new Vector3(i + (i==0 ? -0.5f: 0.5f), 0, j) + dungeon.MinCorner) * (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness), false, false);
                    }

                    // vertical placement
                    if (j < absLengthZ - 1 && grid[i,j] != grid[i,j+1]) {
                        var c = connections.Where(x => x.posA == new Vector3Int((int)i, 0, (int)j) && x.posB == new Vector3Int((int)i, 0, (int)j + 1) || x.posB == new Vector3Int((int)i, 0, (int)j) && x.posA == new Vector3Int((int)i, 0, (int)j + 1));
                        GameObject wall = BuildWall((new Vector3(i, 0, j + 0.5f) + dungeon.MinCorner) * (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness),
                            c.Count() > 0,
                            true);
                        if (c.Count() > 0) {
                            var (a, b, posA, posB) = c.FirstOrDefault();
                            wall.GetComponent<GatedWall>().AssignRooms(generatedRooms[a], generatedRooms[b]);
                        }
                    }

                    // horizontal placement
                    if (i < absLengthX - 1 && grid[i, j] != grid[i + 1, j]) {
                        var c = connections.Where(x => x.posA == new Vector3Int((int)i, 0, (int)j) && x.posB == new Vector3Int((int)i+1, 0, (int)j) || x.posB == new Vector3Int((int)i, 0, (int)j) && x.posA == new Vector3Int((int)i+1, 0, (int)j));
                        GameObject wall = BuildWall((new Vector3(i + 0.5f, 0, j) + dungeon.MinCorner) * (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness),
                            c.Count() > 0,
                            false);
                        if (c.Count() > 0) {
                            var (a, b, posA, posB) = c.FirstOrDefault();
                            wall.GetComponent<GatedWall>().AssignRooms(generatedRooms[a], generatedRooms[b]);
                        }
                    }

                }
            }
        }
        /// <summary>
        /// Builds a wall at <c>position</c>
        /// </summary>
        /// <returns>The instantiated wall</returns>
        /// <param name="isVertical">Vertical: |, Horizontal: -</param>
        GameObject BuildWall(Vector3 position, bool isGated, bool isVertical) {
            string path = $"Dungeon/Walls/{(isGated? "GatedWall": "UngatedWall")}";
            GameObject wall = (GameObject)Instantiate(Resources.Load(path));
            wall.transform.SetPositionAndRotation(position, Quaternion.Euler(0, isVertical? 90: 0, 0));
            return wall;
        }

    }
}