using UnityEngine;
using System;
using NUnit.Framework;

public abstract class WeaponObject : MonoBehaviour {

    public event Action OnAttackStart;
    public event Action OnAttackEnd;
    public event Action OnAttackControlReset;
    public event Action OnBlockStart;
    public event Action OnBlockEnd;
    public event Action OnBlockControlReset;

    /// <summary>
    /// Triggered when attack button is clicked
    /// </summary>
    /// <param name="attackTime">1/(final attack speed)</param>
    public virtual void AttackClicked(float attackTime) {
        ResetAttackControl();
    }

    /// <summary>
    /// Triggered when attack button is lifted
    /// </summary>
    /// <param name="attackTime">1/(final attack speed)</param>
    public virtual void AttackLifted(float attackTime) {

    }

    public virtual void BlockClicked() {
        ResetBlockControl();
    }
    public virtual void BlockLifted() {

    }
    public virtual void BlockRotated(float angle) {

    }
    
    /// <summary>
    /// Must be called before an attack to invoke event
    /// </summary>
    protected void StartAttack() {
        OnAttackStart?.Invoke();
    }
    /// <summary>
    /// Must be called after an attack to invoke event
    /// </summary>
    protected void EndAttack() {
        OnAttackEnd?.Invoke();
    }
    /// <summary>
    /// Raises OnAttackControlReset
    /// </summary>
    protected void ResetAttackControl() {
        OnAttackControlReset?.Invoke();
    }
    /// <summary>
    /// Must be called after a block to invoke event
    /// </summary>
    protected void StartBlock() {
        OnBlockStart?.Invoke();
    }
    /// <summary>
    /// Must be called after a block to invoke event 
    /// </summary>
    protected void EndBlock() {
        OnBlockEnd?.Invoke();
    }
    protected void ResetBlockControl() {
        OnBlockControlReset?.Invoke();
    }
}
