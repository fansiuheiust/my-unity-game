using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;


namespace Combat.Abilities {
    public class Clawler : AbilityObject {
        GameObject clawPrefab;

        Projectile activeClaw = null;
        float clawDmg;
        float searchRadius;
        float initialSearchRadius;
        int redirectCount;
        float clawSpeed;
        readonly HashSet<Mob> hitMobs = new();

        protected override void SetFields(Ability ability) {
            clawPrefab = ability.Prefab("Claw");
            clawDmg = ability["Claw Damage"];
            searchRadius = ability["Search Radius"];
            initialSearchRadius = ability["Initial Search Radius"];
            redirectCount = (int)ability["Redirect Count"];
            clawSpeed = ability["Claw Speed"];
        }

        protected override void SubscribeToOwner() {
        }

        protected override void AbilityBehaviour() {
            if (activeClaw != null) {
                ResetClaw();
            }
            activeClaw = Instantiate(clawPrefab).GetComponent<Projectile>();
            activeClaw.transform.position = Owner.transform.position + Owner.Rotatable.forward;
            activeClaw.Set(Owner, clawDmg, Vector3.zero, (uint)redirectCount);
            activeClaw.onHit.AddListener(OnClawHit);
            activeClaw.onDelete.AddListener(OnClawDeleted);
            FindTarget(true);
            
        }

        protected override void AbilityRemovalBehaviour() {
            if (activeClaw != null) ResetClaw();
        }

        void OnClawHit(Mob m) {
            if (activeClaw.PierceLeft > 1) {
                hitMobs.Add(m);
                FindTarget(false);
            }
        }

        void FindTarget(bool initial) {
            Mob[] mobs = Physics.OverlapSphere(activeClaw.transform.position, initial? initialSearchRadius: searchRadius)
                .Where(x => x.GetComponent<Mob>() != null)
                .Select(x=>x.gameObject.GetComponent<Mob>())
                .OrderBy(x=>(x.transform.position-activeClaw.transform.position).magnitude)
                .ToArray();
            foreach (Mob m in mobs) {
                if (!hitMobs.Contains(m) && Owner.CanAttack(m)) {
                    activeClaw.velocity = clawSpeed * (m.transform.position-activeClaw.transform.position).normalized;
                    return;
                }
            }
            // no target
            ResetClaw();
        }

        void ResetClaw() {
            activeClaw.Delete();
        }

        void OnClawDeleted() {
            activeClaw.onHit.RemoveListener(OnClawHit);
            activeClaw.onDelete.RemoveListener(OnClawDeleted);
            hitMobs.Clear();
            activeClaw = null;
        }
    }
}