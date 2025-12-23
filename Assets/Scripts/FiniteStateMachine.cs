using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class FiniteStateMachine<Estate> : MonoBehaviour where Estate : Enum {

    private Dictionary<Estate, BaseState<Estate>> states = new Dictionary<Estate, BaseState<Estate>>();
    private BaseState<Estate> currentState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start() {
        currentState.EnterState();
    }

    // Update is called once per frame
    protected virtual void Update() {
        Estate nextStateKey = currentState.GetNextState();

        if (!nextStateKey.Equals(currentState.stateKey))
            TransitionToState(nextStateKey);

        currentState.UpdateState();
    }

    protected virtual void FixedUpdate() {
        currentState.FixedUpdateState();
    }

    protected void LateUpdate() {
        currentState.LateUpdateState();
    }

    protected void AddState(Estate stateKey, BaseState<Estate> state) {
        states.Add(stateKey, state);
    }

    protected void SetCurrentState(Estate stateKey) {
        currentState = states[stateKey];
    }

    public void TransitionToState(Estate stateKey) {
        currentState.ExitState();
        SetCurrentState(stateKey);
        currentState.EnterState();
    }

    protected void OnTriggerEnter(Collider other) {
        currentState.OnTriggerEnter(other);
    }

    protected void OnTriggerStay(Collider other) {
        currentState.OnTriggerStay(other);
    }

    protected void OnTriggerExit(Collider other) {
        currentState.OnTriggerExit(other);
    }
}
