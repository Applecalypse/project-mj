using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthController : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100;

    [Header("Team")]
    // maybe can be used for checking Friendly fire
    [SerializeField] private Team myTeam;

    [Header("Network")]
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; 
            return;
        }
        
        currentHealth.Value = maxHealth;
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    public void TakeDamageServerRPC(float damage)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            if (myTeam == Team.Monster)
            {
                GameManager.Instance.MonsterDead();
            }
            else
            {
                GetComponent<PlayerController>().isDead = true;
                GameManager.Instance.OnPlayerDeath();
            }
            Debug.Log(transform.name + " is dead");
            // Die();
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log(transform.name + ": taking damage = " + damage);
        TakeDamageServerRPC(damage);
    }

}
