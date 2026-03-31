
using BuildingBlocks;
using UnityEngine;
using UnityEngine.Events;

namespace Dungeon {
    public class GatedWall : Wall {
        public UnityEvent<Wall> OnUnlock;
        public bool Locked { get; private set; } = false;
        public override void AssignRooms(Room a, Room b) {
            base.AssignRooms(a, b);
            a.OnRoomClear.AddListener(OpenGate);
            b.OnRoomClear.AddListener(OpenGate);
        }

        void OpenGate(Room _) {
            if (Locked) return;
            Locked = true;
            OnUnlock.Invoke(this);
        }
    }
}