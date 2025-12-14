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
    }

    class Room {
        public List<Block> Blocks { get; private set; }


        public Room(Block[] blocks) {
            Blocks = blocks.ToList();

            // TODO: check if all blocks are vertically xor horizontally next to e.o.
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
            new Room(new Block[] { new Block(new(0, 0)), new(new(0, 1)), new(new(1, 0)) }).Edges.ForEach(x=>Debug.Log(x));
        }
    }
}