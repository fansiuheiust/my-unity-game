using Combat;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace BuildingBlocks {
    /// <summary>
    /// An abstract class that allows blocks to do something when mobs enter it, do not directly from it
    /// </summary>
    public abstract class Enterable: MonoBehaviour {
        [field: SerializeField, Tooltip("Time between trigger, if it is 0, it only triggers once per collision"), Min(0f)]
        public float Interval { get; private set; }

        /// <summary>
        /// Mobs that are currently triggering effect from the Enterable
        /// </summary>
        LinkedList<Mob> _affecteds = new();

        /// <summary>
        /// Triggers when a mob enters the block
        /// </summary>
        public UnityEvent<Mob> OnMobEnter = new();
        /// <summary>
        /// Triggers when a mob exits the block
        /// </summary>
        public UnityEvent<Mob> OnMobExit = new();
        /// <summary>
        /// Triggers when a mob triggers effect in the block
        /// </summary>
        public UnityEvent<Mob> OnEffectTrigger = new();

        /// <summary>
        /// only called when a mob enters the enterable
        /// </summary>
        /// <param name="m"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MobEnteredInternal(Mob m) {
            // failsafe as double trigger can happen with GroundCollider
            if (_affecteds.Contains(m)) return;
            OnMobEnter.Invoke(m);
            OnMobEntered(m);
            _affecteds.AddLast(m);
            
            
            if (Interval == 0f)
                TriggerEffectInternal(m);
            else
                StartCoroutine(EffectTriggerer(m));
        }

        /// <summary>
        /// Coded behaviour of the enterable when a mob enters the block
        /// </summary>
        /// <param name="m">mob that enters</param>
        protected virtual void OnMobEntered(Mob m) {

        }

        /// <summary>
        /// only called when a mob exits the enterable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MobExitedInternal(Mob m) {
            // failsafe as double trigger can happen with GroundCollider
            if (!_affecteds.Contains(m)) return;
            OnMobExit.Invoke(m);
            OnMobExited(m);
            _affecteds.Remove(m);
        }
        /// <summary>
        /// Behaviour of the enterable when mob exits
        /// </summary>
        /// <param name="m">mob that exits</param>
        protected virtual void OnMobExited(Mob m) {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void TriggerEffectInternal(Mob m) {
            OnEffectTrigger.Invoke(m);
            TriggerEffect(m);
        }
        /// <summary>
        /// Behaviour of the enterable when the effect should be triggered on the mob
        /// </summary>
        /// <param name="m">The target mob</param>
        protected abstract void TriggerEffect(Mob m);

        IEnumerator EffectTriggerer(Mob m) {
            do {
                // TODO: implement a better mob dead detector, and use it
                if (m.IsDestroyed()) {
                    _affecteds.Remove(m);
                    yield break;
                }
                TriggerEffectInternal(m);
                yield return new WaitForSeconds(Interval);
            } while (_affecteds.Contains(m));
            yield break;
        }


        protected virtual void OnTriggerEnter(Collider c) {
            if (c.CompareTag("GroundCollider"))
                MobEnteredInternal(Mob.FindParentingMob(c.transform));
        }

        protected virtual void OnTriggerExit(Collider c) {
            if (c.CompareTag("GroundCollider"))
                MobExitedInternal(Mob.FindParentingMob(c.transform));
        }
    }

    /// <summary>
    /// Enterable via collision. TODO: make collision from above possible even with floating
    /// </summary>
    public abstract class CollisionEnterable: Enterable {
        void OnCollisionEnter(Collision collision) {
            if (collision.collider.TryGetComponent(out Mob m))
                MobEnteredInternal(m);
        }

        void OnCollisionExit(Collision collision) {
            if (collision.collider.TryGetComponent(out Mob m))
                MobExitedInternal(m);
        }
    }

    public abstract class TriggerEnterable: Enterable {
        protected override void OnTriggerEnter(Collider other) {
            base.OnTriggerEnter(other);
            if (other.TryGetComponent(out Mob m)) {
                MobEnteredInternal(m);
            }
        }

        protected override void OnTriggerExit(Collider other) {
            base.OnTriggerExit(other);
            if (other.TryGetComponent(out Mob m))
                MobExitedInternal(m);
        }
    }
}