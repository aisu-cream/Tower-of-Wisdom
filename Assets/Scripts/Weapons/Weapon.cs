using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {

    [field: SerializeField] public float damage { get; private set; }
    protected IEntity holder;

    public void SetHolder(IEntity holder) {
        this.holder = holder;
    }

    public abstract void Damage(Collider other);

    public abstract void EndAction();

    /**
     * Wield this weapon
     * Returns the time this action finishes
     */
    public virtual void Wield() {
        throw new NotSupportedException();
    }

    /**
     * Throw this weapon; Physical displacement of the weapon
     * Returns the time this action finishes
     */
    public virtual void Throw() {
        throw new NotSupportedException();
    }

    /**
     * Charges the weapon
     * Returns the time this action finishes
     */
    public virtual void StartCharge() {
        throw new NotSupportedException();
    }

    /**
     * Release the charge; Requires StartCharge method to be implemented/used
     * Returns the time this action finishes
     */
    public virtual void Release() {
        throw new NotSupportedException();
    }
}
