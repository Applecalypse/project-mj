using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [Header("Collision Detection")]
    // private Rigidbody rb;
    private Collider col;
    private bool collisionEnabled = false;
    [SerializeField] private Team teamToDamage;
    private string teamToDamageString;

    [Header("Object Properties")]
    [SerializeField] private float damage = 0;
    [SerializeField] private bool breakOnCollision = true;
    [SerializeField] private bool onlyApplyDamageOnce = true;

    void Start()
    {
        // rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if (teamToDamage == Team.Human) { teamToDamageString = "Player"; }
        else if (teamToDamage == Team.Monster) { teamToDamageString = "Enemy"; }
        
    }

    public void EnableCollision() { collisionEnabled = true; }

    public void DisableCollision() { collisionEnabled = false; }

    void HandleCollision(GameObject other)
    {
        if (!collisionEnabled) { return; }
        Debug.Log("Damagable: Collided with " + teamToDamageString);
        other.gameObject.GetComponent<HealthController>().TakeDamage(damage);

        if (onlyApplyDamageOnce) { DisableCollision(); }
        if (breakOnCollision) { Destroy(gameObject); }
    }

    void OnCollisionEnter(Collision other)
    {
        if (col.isTrigger) { return; }
        Debug.Log("Damagable: Collision Detected " + other.gameObject.name);
        
        if (!other.gameObject.CompareTag(teamToDamageString)) { return; }
        HandleCollision(other.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!col.isTrigger) { return; }
        Debug.Log("Damagable: Trigger Detected " + other.gameObject.name);
        
        if (!other.gameObject.CompareTag(teamToDamageString)) { return; }
        HandleCollision(other.gameObject);
    }
}
