using Combat;
using Interactable;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// one-time use objects mainly to trigger events
/// </summary>
public class Lever : MonoBehaviour, IInteractable {

    public UnityEvent OnInteract;

    bool _isUntouched = true;

    Transform _rotatable;
    void Awake() {
        _rotatable = transform.Find("Rotatable");
    }
    
    public bool IsInteractable => _isUntouched;
    public void Interact(Mob _) {
        _isUntouched = false;
        OnInteract.Invoke();
        StartCoroutine(LeverAnimation());
    }

    IEnumerator LeverAnimation() {
        yield return SimpleAnimation.Rotate(_rotatable, new(90, 0, 0), 0.3f);
        yield break;
    }
}
