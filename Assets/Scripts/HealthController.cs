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

    public void SetCurrentHealth(float val)
    {
        //currentHealth.Value = val;
        Debug.Log( currentHealth.Value );
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        currentHealth.Value -= damage;
        Debug.Log("DAMAGED");
        if (currentHealth.Value <= 0)
        {
            if (myTeam == Team.Monster)
            {
                GameManager.Instance.MonsterDead();
            }
            if (myTeam == Team.Human)
            {
                GetComponent<PlayerController>().OnDead();
                GameManager.Instance.OnPlayerDeath();
                StartCoroutine(KillCoroutine());
            }
            else
            {
                Debug.LogError("Wtf how did we reach here?");
            }
            Debug.Log(transform.name + " is dead");
            // Die();
        }
    }

    IEnumerator KillCoroutine()
    {
        yield return new WaitForSeconds(3f);
        KillPlayer();
    }
    private void KillPlayer()
    {
        GetComponent<PlayerController>().setSpectator(true);
    }

    public void RevivePlayer()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.Revive();
        currentHealth.Value = maxHealth;
        StopCoroutine(KillCoroutine());
        GameManager.Instance.OnPlayerRevive();
    }

    public void TakeDamage(float damage)
    {
        Debug.Log(transform.name + ": taking damage = " + damage);
        TakeDamageServerRpc(damage);
    }

}
