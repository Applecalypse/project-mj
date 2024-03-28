using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField] private float damage = 0;
    private bool collisionEnabled = false;

    void EnableCollision() { collisionEnabled = true; }

    void DisableCollision() { collisionEnabled = false; }

    void OnCollisionEnter(Collision other)
    {
        if (!collisionEnabled) { return; }
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Throwable: Collidded with Enemy");
            other.gameObject.GetComponent<HealthController>().TakeDamage(damage);
            
        }
        else { collisionEnabled = false; Debug.Log("Throwable: Collided with something else"); }

        Destroy(gameObject);
    }
}
