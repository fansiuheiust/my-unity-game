using System.Collections;
using UnityEngine;

namespace BuildingBlocks {
    public static class SimpleAnimation {
        /// <summary>
        /// Moves something
        /// </summary>
        /// <param name="x">Thing to move</param>
        /// <param name="delta">Change of position</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static IEnumerator Move(Transform x, Vector3 delta, float time) {
            for (float f = 0; f < time; f += Time.deltaTime) {
                if (x == null) yield break;
                x.localPosition += delta * Time.deltaTime / time;
                yield return null;
            }
            yield break;
        }

        /// <summary>
        /// Rotates something 
        /// </summary>
        /// <param name="x">Thing to rotate</param>
        /// <param name="delta">Change of angle in degree</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static IEnumerator Rotate(Transform x, Vector3 delta, float time) {
            for (float f = 0; f < time; f += Time.deltaTime) {
                if (x == null) yield break;
                x.localEulerAngles += delta * Time.deltaTime / time;
                yield return null;
            }
            yield break;
        }

        /// <summary>
        /// Rescales an object by its localScale
        /// </summary>
        /// <param name="x"></param>
        /// <param name="delta">change of size in each axis</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static IEnumerator Rescale(Transform x, Vector3 delta, float time) {
            for (float f = 0; f < time; f += Time.deltaTime) {
                if (x == null) yield break;
                x.localScale += delta * Time.deltaTime / time;
                yield return null;
            }
            yield break;
        }
    }
}
