using UnityEngine;
using Combat;
 
namespace BuildingBlocks {
    public interface IInteractable {
        /// <summary>
        /// Whether the user can interact with the object or not
        /// </summary>
        public bool IsInteractable { get; }
        /// <summary>
        /// Behaviour of interaction with the interactable
        /// </summary>
        public void Interact(Mob interacter);
    }
}
