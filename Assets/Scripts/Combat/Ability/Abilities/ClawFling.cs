using System.Collections;
using UnityEngine;

namespace Combat.Abilities {
    public class ClawFling : AbilityObject {
        float clawDmg;
        float clawSpeed;
        int numClaws;

        GameObject clawPrefab;

        Projectile[] spawnedClaws;
        protected override void SetFields(Ability ability) {
            numClaws = (int)ability["Num Claws"];
            clawDmg = ability["Claw Damage"];
            clawSpeed = ability["Claw Speed"];
            clawPrefab = ability.Prefab("Claw");
        }

        protected override void SubscribeToOwner() { }

        protected override void AbilityBehaviour() {
            StartCoroutine(Claws());
        }

        IEnumerator Claws() {
            spawnedClaws = new Projectile[numClaws];
            for (int i = 0; i < numClaws; i++) {
                spawnedClaws[i] = Instantiate(clawPrefab).GetComponent<Projectile>();
                Vector3 front = Quaternion.Euler(0, 360f / numClaws * i, 0) * Vector3.forward;
                spawnedClaws[i].transform.position = transform.position + 1.5f * front;
                spawnedClaws[i].transform.forward = front;
            }
            yield return new WaitForSeconds(1);

            for (int i = 0; i < numClaws; i++)
                spawnedClaws[i].Set(Owner, clawDmg, clawSpeed * (Quaternion.Euler(0, 360f/numClaws*i, 0) * Vector3.forward), 1);

            yield return new WaitForSeconds(2);
            foreach (Projectile p in spawnedClaws)
                if (p != null)
                    Destroy(p.gameObject);
            spawnedClaws = null;
        }

        protected override void AbilityRemovalBehaviour() {
            StopAllCoroutines();
            if (spawnedClaws != null)
                foreach (Projectile p in spawnedClaws)
                    if (p != null)
                        Destroy(p.gameObject);
        }

    }
}