using UnityEngine;

public class HostileMelee : MobAI {
    [SerializeField] protected float attackRadius = 2;
    protected override bool Predicate(Mob m) => m is Player;

    /// <summary>
    /// vector from self to target, updated at the start of <c>Update</c>
    /// </summary>
    Vector3 _delta;

    void Update() {
        if (Target == null) return;
        _delta = Target.transform.position - transform.position;
        FollowTarget();
        AttackCloseTarget();
    }

    /// <summary>
    /// Moves towards target every update
    /// </summary>
    void FollowTarget() {
        MoveDirection = _delta.magnitude < attackRadius? Vector3.zero : Vector3.Scale(_delta, new Vector3(1, 0, 1));
    }

    /// <summary>
    /// indicates whether attack 'control' is reset
    /// </summary>
    bool _canAttack = true;
    
    /// <summary>
    /// Uses attack if the target is close enough and the wepaon is not on cooldown
    /// </summary>
    void AttackCloseTarget() {
        if (_canAttack && _delta.magnitude < attackRadius) {
            _canAttack = false;
            ClickAttack();
        }
    }

    protected override void OnAttackControlReset() {
        _canAttack = true;
    }
}
