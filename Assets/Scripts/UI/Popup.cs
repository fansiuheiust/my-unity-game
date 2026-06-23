using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class Popup : MonoBehaviour {
        [SerializeField] Button exitButton;
        public virtual void OnExitPressed() {
            Destroy(gameObject);
        }

    }
}