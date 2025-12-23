using UnityEngine;

public interface IEntity {
    float GetHealth();
    Vector3 GetPosition();
    Vector3 GetHorizontalPosition();
    Vector3 GetVelocity();
    Vector3 GetHorizontalVelocity();
    Vector3 GetDesiredDirection();
    Vector3 GetFacingDirection();
    bool IsGrounded();
    void HoldWeapon(Weapon weapon);
    void DropWeapon();
    void TakeDamage(float damage);
    void AddForce(Vector3 force, ForceMode forceMode);
    void GetPushed(Vector3 impulse);
}