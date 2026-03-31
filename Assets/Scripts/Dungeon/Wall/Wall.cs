using Dungeon;
using UnityEngine;

namespace Dungeon {
    public class Wall : MonoBehaviour {
        protected Room a, b;
        public virtual void AssignRooms(Room a, Room b) {
            this.a = a; this.b = b;
        }
    }
}