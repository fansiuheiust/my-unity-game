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
        public string Name { get; private set; }
        Vector2Int _center;

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
        /// <param name="center">Where (0,0) of <c>blocks</c> should be</param>
        /// <param name="blocks">blocks with origin at (0,0), a version that is centered around <c>center</c> will be created for Room.Blocks.<br />An implicit (0,0) will be generated if not given</param>
        /// <exception cref="System.Exception">Thrown when blocks are invalid</exception>
        public Room(Vector2Int center, string name, RoomType type, params Block[] blocks) {

            if (blocks is null || blocks.Length == 0)
                blocks = new Block[1] { new Block(0,0) };

            if (!blocks.Any(x=>x.coordinate==Vector2Int.zero))
                blocks = blocks.Append(new Block(0,0)).ToArray();

            if (blocks.Length != 1 && blocks.Any(x => !blocks.Any(y => x.HammingDistance(y) == 1)))
                throw new System.Exception("Blocks must be next to each other horizontally XOR vertically");
            Name = name;
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
        public Room(int centerX, int centerY, string name, RoomType type, params Block[] blocks) : this(new Vector2Int(centerX, centerY), name, type, blocks) { }

        /// <summary>
        /// Checks whether the room occupies a coordinate
        /// </summary>
        /// <param name="coordinate">the coordinate to check</param>
        /// <returns>self-documenting</returns>
        public bool Contains(Vector2Int coordinate) => Blocks.Any(b=>b.coordinate==coordinate);

        public bool Collides(Room other) => Blocks.Any(b => other.Contains(b.coordinate));

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
        
        
        public Vector2Int Center {
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
                // [sinx, cosx ] [y] = [xsinx+ycosx]
                // 0: (+x, +y)
                // 1: (-y, +x)
                // 2: (-x, -y)
                // 3: (+y, -x)
                uint toRotate = (4 + value - _normalizedRotation) % 4;
                for (int i = 0; i < Blocks.Length; i++) {
                    ref Block b = ref Blocks[i];
                    b.coordinate -= Center;
                    b.coordinate = new Vector2Int((toRotate % 3 == 0 ? 1 : -1) * (toRotate % 2 == 0 ? b.coordinate.x : b.coordinate.y),
                                                  (toRotate < 2 ? 1 : -1) * (toRotate % 2 == 0 ? b.coordinate.y : b.coordinate.x))
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
        public Vector2Int MinCorner => new(Blocks.Min(b => b.coordinate.x), Blocks.Min(b => b.coordinate.y));

        /// <summary>
        /// Vector that contains the largest X and Y of the entire room
        /// </summary>
        public Vector2Int MaxCorner => new(Blocks.Max(b => b.coordinate.x), Blocks.Max(b => b.coordinate.y));
    }

    struct Connection {
        /// <summary>
        /// The rooms that are connected to each other
        /// </summary>
        public Room a, b;
        /// <summary>
        /// The coordinate of the connection point of the connectinf rooms
        /// </summary>
        public Vector2 posA, posB;

        public Connection(Room a, Room b, Vector2 posA, Vector2 posB) {
            this.a = a;
            this.b = b;
            this.posA = posA;
            this.posB = posB;
        }
    }
}