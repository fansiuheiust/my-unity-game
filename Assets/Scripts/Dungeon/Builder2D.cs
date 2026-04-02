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
            foreach (var (type, blocks, center, rotation) in rooms) {
                Vector3Int spawnPos = center * (int)(StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness * 2);
                
            }
        }


    }
}