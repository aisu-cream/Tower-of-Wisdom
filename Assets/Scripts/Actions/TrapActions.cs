using UnityEngine;

public class TrapActions : MonoBehaviour
{
    public void SpawnProjectile()
    {
        Debug.Log("Firing a dart!");
        // Logic for instantiating a projectile goes here
    }

    public void PlaySound(AudioSource source)
    {
        source.Play();
    }

    public void DestroyObject(GameObject target)
    {
        Destroy(target);
    }
}