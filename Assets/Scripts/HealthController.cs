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
    private Team myTeam;

    [Header("Network")]
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; 
            return;
        }

        CheckTeam();
        
        currentHealth.Value = maxHealth;
        base.OnNetworkSpawn();
    }

    void CheckTeam()
    {
        // automatically assign team based on the controller
        PlayerController playerController = GetComponent<PlayerController>();
        EnemyController enemyController = GetComponent<EnemyController>();
        
        if (playerController != null) { myTeam = playerController.team.Value; }
        else if (enemyController != null) { myTeam = enemyController.team.Value; }
        else { Debug.LogError("Wtf how did we reach here?"); }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRPC(float damage)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            if (myTeam == Team.Monster)
            {
                GameManager.Instance.MonsterDead();
            }
            if (myTeam == Team.Human)
            {
                GetComponent<PlayerController>().isDead = true;
                GameManager.Instance.OnPlayerDeath();
            }
            else
            {
                Debug.LogError("Wtf how did we reach here?");
            }
            Debug.Log(transform.name + " is dead");
            // Die();
        }
    }

    public void RevivePlayer()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.Revive();
        currentHealth.Value = maxHealth;
        GameManager.Instance.OnPlayerRevive();
    }

    public void TakeDamage(float damage)
    {
        Debug.Log(transform.name + ": taking damage = " + damage);
        TakeDamageServerRPC(damage);
    }

}
