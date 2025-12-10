using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dungeon {
    public abstract class Room : MonoBehaviour {
        bool _cleared = false;
        public bool Cleared {
            get => _cleared;
            protected set {
                _cleared = value;
                if (value) {
                    OnRoomClear.Invoke(this);
                }
            }
        }

        /// <summary>
        /// <para>Invoked when the room is cleared</para>
        /// Room0: The room that is cleared
        /// </summary>
        public UnityEvent<Room> OnRoomClear;

    }
}