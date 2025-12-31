using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BuildingBlocks {

    public enum DisappearStyle {
        None, Fall, Expand, Shrink
    }
    public class Disappearable : MonoBehaviour {
        [field: SerializeField, Tooltip("How the obstacle should disappear")] public DisappearStyle Style { get; private set; } = DisappearStyle.None;
        [field: SerializeField, Tooltip("How long should the disappear animation last"), Min(0.125f)] public float Duration { get; private set; }
        [field: SerializeField, Tooltip("Magnitude of animation (None: /, Fall: depth of fall, Expand: Size increased by x%, Shrink: / )"), Min(0f)] public float Magnitude { get; private set; }

        public UnityEvent OnDisappear = new();

        public void StartDisappear() => StartCoroutine(Disappear());

        IEnumerator Disappear() {
            Collider[] children = GetComponentsInChildren<Collider>(true);
            if (children is not null)
                foreach (Collider child in children) {
                    child.gameObject.layer = 2;
                    child.isTrigger = true;    
                }
            


            OnDisappear.Invoke();

            // animation goes here
            yield return Style switch {
                DisappearStyle.Fall => FallAnimation(),
                DisappearStyle.Expand => ExpandAnimation(),
                DisappearStyle.Shrink => ShrinkAnimation(),
                _ => NoneAnimation(),

            } ;



            Destroy(gameObject);
            yield break;
        }

        // functions called inside Disappear()

        IEnumerator NoneAnimation() {
            yield break;
        }

        IEnumerator FallAnimation() {
            yield return SimpleAnimation.Move(transform, Magnitude * Vector3.down, Duration);
        }

        IEnumerator ExpandAnimation() {
            StartCoroutine(SimpleAnimation.Rotate(transform, new(0, 360, 0), Duration));
            yield return SimpleAnimation.Rescale(transform, Magnitude * Vector3.one, Duration);
        }

        IEnumerator ShrinkAnimation() {
            StartCoroutine(SimpleAnimation.Rotate(transform, new(0, -360, 0), Duration));
            yield return SimpleAnimation.Rescale(transform, -transform.localScale, Duration);
        }
        

        
    }
}