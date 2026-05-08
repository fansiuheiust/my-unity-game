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
        Start, Mob, Final, Ladder
    }

    /// <summary>
    /// A square block
    /// </summary>
    struct Block {
        public Vector3Int coordinate;
        public Block(Vector3Int coordinate) {
            this.coordinate = coordinate;
        }
        public Block(int x, int z) : this(new Vector3Int(x, 0, z)) { }

        /// <summary>
        /// An array of the coordinates horizontally xor vertically next to the block
        /// </summary>
        public Vector3Int[] Edges => new Vector3Int[] {
            new Vector3Int(coordinate.x+1, coordinate.y, coordinate.z),
            new Vector3Int(coordinate.x-1, coordinate.y, coordinate.z),
            new Vector3Int(coordinate.x, coordinate.y, coordinate.z+1),
            new Vector3Int(coordinate.x, coordinate.y, coordinate.z-1)
        };
        /// <summary>
        /// The hamming distance between the block and a coordinate *only considering xz-plane* 
        /// </summary>
        public int HammingDistance(in Vector3Int other) => System.Math.Abs(coordinate.x - other.x) + System.Math.Abs(coordinate.z - other.z);
        /// <summary>
        /// The hamming distance between the block and another block *only considering xz-plane*
        /// </summary>
        public int HammingDistance(in Block other) => HammingDistance(other.coordinate);
    }

    /// <summary>
    /// A collection of blocks 
    /// </summary>
    class Room {
        public RoomType Type { get; private set; }
        public string ShapeName { get; private set; }
        Vector3Int _center;

        /// <summary>
        /// Self-documenting, you can assume that this must contain at least 1 element
        /// </summary>
        public Block[] Blocks { get; private set; }

        uint _normalizedRotation = 0;

        /// <summary>
        /// Blocks with origin=(0,0)
        /// </summary>
        public IEnumerable<Block> LocalBlocks => Blocks.Select(b => new Block(b.coordinate - Center));

        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="shapeName">How a shape should be identified as</param>
        /// <param name="center">Where (0,0) of <c>blocks</c> should be</param>
        /// <param name="blocks">blocks with origin at (0,0), a version that is centered around <c>center</c> will be created for Room.Blocks.<br />An implicit (0,0) will be generated if not given</param>
        /// <exception cref="System.Exception">Thrown when blocks are invalid</exception>
        public Room(Vector3Int center, string shapeName, RoomType type, params Block[] blocks) {

            if (blocks.Any(b => b.coordinate.y != 0))
                throw new System.Exception("Only ladders can have non-zero y coordinated blocks");

            if (blocks is null || blocks.Length == 0)
                blocks = new Block[1] { new Block(0,0) };

            if (!blocks.Any(x=>x.coordinate==Vector3Int.zero))
                blocks = blocks.Append(new Block(0,0)).ToArray();

            if (blocks.Length != 1 && blocks.Any(x => blocks.All(y => x.HammingDistance(y) != 1)))
                throw new System.Exception("Blocks must be next to each other in x XOR z");
            ShapeName = shapeName;
            Blocks = blocks;
            Center = center;
            Type = type;
        }
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="centerX">Where (0,0)'s x of <c>blocks</c> should be</param>
        /// <param name="centerY">Where (0,0)'s y of <c>blocks</c> should be</param>
        /// <param name="blocks">blocks with origin at (0,0), a version that is centered around <c>center</c> will be created for Room.Blocks</param>
        /// <exception cref="System.Exception">Thrown when blocks are invalid</exception>
        public Room(int centerX, int centerZ, string name, RoomType type, params Block[] blocks) : this(new Vector3Int(centerX, 0, centerZ), name, type, blocks) { }

        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="aggregated">A block that contains the name, room type, and the x-z coordinates of all blocks</param>
        public Room((string, RoomType, Vector2Int[]) aggregated) : this(0, 0, aggregated.Item1, aggregated.Item2, aggregated.Item3.Select(x=>new Block(x.x, x.y)).ToArray()) { }

        /// <summary>
        /// Checks whether the room occupies a coordinate
        /// </summary>
        /// <param name="coordinate">the coordinate to check</param>
        /// <returns>self-documenting</returns>
        public bool Contains(Vector3Int coordinate) => Blocks.Any(b=>b.coordinate==coordinate);

        public bool Collides(Room other) => Blocks.Any(b => other.Contains(b.coordinate));

        /// <summary>
        /// The list of edges 
        /// </summary>
        public IEnumerable<Vector3Int> Edges => UnfilteredEdges
                                                    .Distinct()
                                                    .Where(x => !Blocks.Any(b => b.coordinate == x));
        /// <summary>
        /// The list of edges in the room, WITHOUT filtering out overlaps and those inside of blocks, obviously faster (O(n))
        /// </summary>
        public IEnumerable<Vector3Int> UnfilteredEdges => Blocks.Select(b => b.Edges)
                                                        .SelectMany(e=>e);
        
        
        public Vector3Int Center {
            get => _center;
            set {
                for (int i = 0; i < Blocks.Length; i++)
                    Blocks[i].coordinate += value - _center;
                _center = value;
            }
        }

        /// <summary>
        /// floor(Rotation/90), only support 4 rotations
        /// </summary>
        public uint NormalizedRotation { 
            get => _normalizedRotation;
            set {
                value %= 4;
                // rotate by 90*n degrees:
                // [cosx, -sinx] [x]   [xcosx-ysinx]
                // [sinx, cosx ] [z] = [xsinx+ycosx]
                // 0: (+x, +z)
                // 1: (-z, +x)
                // 2: (-x, -z)
                // 3: (+z, -x)
                uint toRotate = (4 + value - _normalizedRotation) % 4;
                for (int i = 0; i < Blocks.Length; i++) {
                    ref Block b = ref Blocks[i];
                    b.coordinate -= Center;
                    b.coordinate = new Vector3Int((toRotate % 3 == 0 ? 1 : -1) * (toRotate % 2 == 0 ? b.coordinate.x : b.coordinate.z), b.coordinate.y,
                                                  (toRotate < 2 ? 1 : -1) * (toRotate % 2 == 0 ? b.coordinate.z : b.coordinate.x))
                                 + Center;
                }
                _normalizedRotation = value; 
            }
        }

        /// <summary>
        /// Rotation in the game
        /// </summary>
        public int RotationInGame => -90 * (int)_normalizedRotation;

        /// <summary>
        /// Vector that contains the smallest X and Y of the entire room
        /// </summary>
        public Vector3Int MinCorner => new(Blocks.Min(b => b.coordinate.x), Blocks.Min(b => b.coordinate.y), Blocks.Min(b => b.coordinate.z));

        /// <summary>
        /// Vector that contains the largest X and Y of the entire room
        /// </summary>
        public Vector3Int MaxCorner => new(Blocks.Max(b => b.coordinate.x), Blocks.Max(b => b.coordinate.y), Blocks.Max(b => b.coordinate.z));
        /// <summary>
        /// A new instance of ladder room with the purpose of connecting 2 layers, its origin is at (0,0)
        /// </summary>
        public static Room Ladder {
            get {
                Room ri = new(0, 0, "1x1", RoomType.Ladder);
                ri.Blocks = new Block[] {
                    new(Vector3Int.zero),
                    new(Vector3Int.down),
                };
                return ri;
            }
        }
        /// <summary>
        /// A new instance of start room with its origin at (0,0)
        /// </summary>
        public static Room Start => new(0, 0, "1x1", RoomType.Start);
        /// <summary>
        /// A new instance of final room with its origin at (0,0)
        /// </summary>
        public static Room Final => new(0, 0, "1x1", RoomType.Final);
    }

    struct Connection {
        /// <summary>
        /// The rooms that are connected to each other
        /// </summary>
        public Room a, b;
        /// <summary>
        /// The coordinate of the connection point of the connectinf rooms
        /// </summary>
        public Vector3Int posA, posB;

        public Connection(Room a, Room b, Vector3Int posA, Vector3Int posB) {
            this.a = a;
            this.b = b;
            this.posA = posA;
            this.posB = posB;
        }
    }
}