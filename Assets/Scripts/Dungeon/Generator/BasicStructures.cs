using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Dungeon.Generator {

    /// <summary>
    /// <para>Self-documenting</para>
    /// <c>Start</c>: Self-documenting<br />
    /// <c>Mob</c>: Rooms where you kill mobs to clear it<br />
    /// <c>Final</c>: The room that leads you to the final boss<br />
    /// </summary>
    public enum RoomType {
        Start, Mob, Final
    }

    /// <summary>
    /// A square block
    /// </summary>
    struct Block {
        public Vector2Int coordinate;
        public Block(Vector2Int coordinate) {
            this.coordinate = coordinate;
        }
        public Block(int x, int y) : this(new Vector2Int(x, y)) { }

        /// <summary>
        /// An array of the coordinates horizontally xor vertically next to the block
        /// </summary>
        public Vector2Int[] Edges => new Vector2Int[] {
            new Vector2Int(coordinate.x+1, coordinate.y),
            new Vector2Int(coordinate.x-1, coordinate.y),
            new Vector2Int(coordinate.x, coordinate.y+1),
            new Vector2Int(coordinate.x, coordinate.y-1)
        };

        public int HammingDistance(Block other) => System.Math.Abs(coordinate.x-other.coordinate.x) + System.Math.Abs(coordinate.y-other.coordinate.y);
    }

    /// <summary>
    /// A collection of blocks 
    /// </summary>
    class Room {
        public RoomType Type { get; private set; }
        public Vector2Int Center { get; private set; }
        public Block[] Blocks { get; private set; }
        /// <summary>
        /// Rotation/90
        /// </summary>
        public int NormalizedRotation { get; private set; } = 0;
        /// <summary>
        /// Blocks with origin=(0,0)
        /// </summary>
        public IEnumerable<Block> LocalBlocks => Blocks.Select(b => new Block(b.coordinate - Center));

        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="center">Where (0,0) of <c>blocks</c> should be</param>
        /// <param name="blocks">blocks with origin at (0,0), a version that is centered around <c>center</c> will be created for Room.Blocks</param>
        /// <exception cref="System.Exception">Thrown when blocks are invalid</exception>
        public Room(Vector2Int center, params Block[] blocks) {

            if (blocks is null || blocks.Length == 0)
                throw new System.Exception("No blocks given");

            if (!blocks.Any(x=>x.coordinate==Vector2Int.zero))
                throw new System.Exception("There must exist a block at the origin");

            if (blocks.Length != 1 && blocks.Any(x => !blocks.Any(y => x.HammingDistance(y) == 1)))
                throw new System.Exception("Blocks must be next to each other horizontally XOR vertically");

            Center = center;
            Blocks = blocks.Select(b=>new Block(b.coordinate+center)).ToArray();

            

        }
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="x">Where (0,0)'s x of <c>blocks</c> should be</param>
        /// <param name="y">Where (0,0)'s y of <c>blocks</c> should be</param>
        /// <param name="blocks">blocks with origin at (0,0), a version that is centered around <c>center</c> will be created for Room.Blocks</param>
        /// <exception cref="System.Exception">Thrown when blocks are invalid</exception>
        public Room(int x, int y, params Block[] blocks) : this(new Vector2Int(x, y), blocks) { }

        /// <summary>
        /// Checks whether the room occupies a coordinate
        /// </summary>
        /// <param name="coordinate">the coordinate to check</param>
        /// <returns>self-documenting</returns>
        public bool Contains(Vector2Int coordinate) => Blocks.Any(b=>b.coordinate==coordinate); 

        /// <summary>
        /// The list of edges 
        /// </summary>
        public IEnumerable<Vector2Int> Edges => UnfilteredEdges
                                                    .Distinct()
                                                    .Where(x => !Blocks.Any(b => b.coordinate == x));
        /// <summary>
        /// The list of edges in the room, WITHOUT filtering out overlaps and those inside of blocks, obviously faster (O(n))
        /// </summary>
        public IEnumerable<Vector2Int> UnfilteredEdges => Blocks.Select(b => b.Edges)
                                                        .SelectMany(e=>e);

        /// <summary>
        /// Rotates the entire room about the center
        /// </summary>
        /// <param name="degreeNormalized">actual degree/90</param>
        public void Rotate(uint degreeNormalized) {
            if (degreeNormalized >= 4) degreeNormalized %= 4;
            // [cosx, -sinx] [x]   [xcosx-ysinx]
            // [sinx, cosx ] [y] = [xsinx+ycosx]
            // 0: (+x, +y)
            // 1: (-y, +x)
            // 2: (-x, -y)
            // 3: (+y, -x)
            System.Array.ForEach(Blocks, b=>b.coordinate = new Vector2Int(degreeNormalized%3==0? 1: -1 * degreeNormalized%2==0? b.coordinate.x-Center.x: b.coordinate.y-Center.y,
                                                                            degreeNormalized < 2? 1: -1 * degreeNormalized%2 == 0? b.coordinate.y - Center.y : b.coordinate.x - Center.x)
                                                            + Center);
        }
        
        
    }

    /// <summary>
    /// The generator
    /// </summary>
    public class Floor {
        List<Room> rooms = new();

        /// <summary>
        /// The big thing that generates the dungeon
        /// </summary>
        /// <param name="options">Basically the list of rooms the program can choose from</param>
        /// <param name="minimums">The minimum number of each type of room, no entry if no minimum for the type; null if no minimum at all</param>
        /// <param name="maximums">The minimum number of each type of room, no entry if no maximum for the type; null if no maximum at all</param>
        public void Generate((RoomType, Vector2Int[])[] options, Dictionary<RoomType, uint> minimums = null, Dictionary<RoomType, uint> maximums = null) {
            // for now, treat this segment as a black box
        }

        IEnumerable<Vector2Int> Edges => rooms.Select(r => r.Edges)
                                            .SelectMany(list=>list)
                                            .Distinct()
                                            .Where(c=>!rooms.Any(r=>r.Contains(c)));
        IEnumerable<Vector2Int> RandomizedEdges => Edges.OrderBy(_ => Random.value);
    }

    public static class Driver {
        public static void Main() {
            new Room(new(5,4), new Block(0, 0), new Block(0,1)).Edges.ToList().ForEach(x=>Debug.Log(x));
        }
    }
}