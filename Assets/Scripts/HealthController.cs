using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealth;

    // maybe can be used for checking Friendly fire
    [SerializeField] private Team myTeam;

    private void Start()
    {
        currentHealth = maxHealth;

        // get team here
        // myTeam = GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId ? Team.Human : Team.Monster;

    }

    public void TakeDamage(int damage)
    {
        Debug.Log(transform.name + ": taking damage = " + damage);
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Debug.Log(transform.name + " is dead");
            // Die();
        }
    }

}
