using UnityEngine;

public interface IEntity {
    float GetHealth();
    Vector3 GetPosition();
    void HoldWeapon(Weapon weapon);
    void DropWeapon();
    void TakeDamage(float damage);
    void AddForce(Vector2 force, float zForce, ForceMode2D forceModeZ);
    void GetPushed(Vector2 impulse, float zImpulse);
}