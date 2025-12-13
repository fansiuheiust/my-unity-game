using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        public List<Block> Blocks { get; private set; } = new();


        public Room(Block[] blocks) {
            foreach (Block b in blocks) {
                Blocks.Add(b);
            }
            // TODO: check if all blocks are vertically xor horizontally next to e.o.
        }
        
        public List<Vector2Int> Edges {
            get {
                List<Vector2Int> ri = new();
                // this chunk of code is O(n^2), optimize it
                foreach (Block b in Blocks) {
                    List<Vector2Int> result = b.Edges;
                    foreach (Vector2Int edge in result) {
                        if (!Blocks.Any(x=>x.coordinate == edge))
                            ri.Add(edge);
                    }
                }
                return ri;
            }
        }
        
    }
}