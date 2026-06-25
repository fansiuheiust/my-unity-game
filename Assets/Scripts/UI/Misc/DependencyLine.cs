using Progression;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class DependencyLine : MonoBehaviour {
        [SerializeField] Vector3 from, to;
        [SerializeField] float width = 10;
        Image image;
        RectTransform rectTransform = null;
        public DependencyType dependencyType;
        public Vector3 From {
            get => from; set {
                from = value;
                UpdateObject();
            }
        }
        public Vector3 To {
            get => to; set {
                to = value;
                UpdateObject();
            }
        }
        public float Width {
            get => width; set {
                width = value;
                UpdateObject();
            }
        }
        private void Awake() {
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            UpdateObject();
        }

        public void UpdateObject() {
            rectTransform.localPosition = (from + to) / 2;
            Vector3 delta = to - from;
            rectTransform.sizeDelta = new Vector2(width, delta.magnitude);
            rectTransform.eulerAngles = new Vector3(0, 0, -Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg);
        }

        public Color Color {
            get => image.color; set {
                image.color = value;
            }
        }
    }
}