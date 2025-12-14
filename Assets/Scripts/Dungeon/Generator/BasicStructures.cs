using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Dungeon.Generator {

    /// <summary>
    /// A square block
    /// </summary>
    struct Block {
        public Vector2Int coordinate;
        public Block(Vector2Int coordinate) {
            this.coordinate = coordinate;
        }

        /// <summary>
        /// An array of the coordinates horizontally xor vertically next to the block
        /// </summary>
        public List<Vector2Int> Edges => new() {
            new Vector2Int(coordinate.x+1, coordinate.y),
            new Vector2Int(coordinate.x-1, coordinate.y),
            new Vector2Int(coordinate.x, coordinate.y+1),
            new Vector2Int(coordinate.x, coordinate.y-1)
        };

        public int HammingDistance(Block other) => System.Math.Abs(coordinate.x-other.coordinate.x) + System.Math.Abs(coordinate.y-other.coordinate.y);
    }

    class Room {
        public Vector2Int Center { get; private set; }
        public List<Block> Blocks { get; private set; }

        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="center">Where (0,0) of <c>blocks</c> should be</param>
        /// <param name="blocks">blocks with origin at (0,0), a version that is centered around <c>center</c> will be created for Room.Blocks</param>
        /// <exception cref="System.Exception"></exception>
        public Room(Vector2Int center, params Block[] blocks) {
            Center = center;
            Blocks = blocks.Select(b=>new Block(b.coordinate+center)).ToList();

            if (Blocks.Count != 1 && Blocks.Any(x => !Blocks.Any(y => x.HammingDistance(y) == 1)))
                throw new System.Exception("Blocks must be next to each other");
        }
        
        public List<Vector2Int> Edges {
            get {
                // a list of distinct edges of all blocks that are not in the block itself
                List<Vector2Int> ri = Blocks.Select(b=>b.Edges)
                                            .Aggregate(new List<Vector2Int>(), (c, acc) => acc.Union(c).ToList())
                                            .Where(x=>!Blocks.Any(b=>b.coordinate == x))
                                            .ToList();
                return ri;
            }
        }
        
    }

    public static class Driver {
        public static void Main() {
            new Room(new(5,4),new Block[] { new Block(new(0, 0)) }).Edges.ForEach(x=>Debug.Log(x));
        }
    }
}