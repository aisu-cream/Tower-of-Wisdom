using System;
using UnityEngine;

public abstract class BaseState<Estate> where Estate : Enum {

    public Estate stateKey { get; private set; }

    public BaseState(Estate key) {
        stateKey = key;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }
    public virtual void LateUpdateState() { }
    public abstract Estate GetNextState();
    public virtual void OnTriggerEnter2D(Collider2D other) { }
    public virtual void OnTriggerStay2D(Collider2D other) { }
    public virtual void OnTriggerExit2D(Collider2D other) { }
}
