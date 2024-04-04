using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class HealthController : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100;

    [Header("Team")]
    // maybe can be used for checking Friendly fire
    [SerializeField] private NetworkVariable<Team> myTeam = new NetworkVariable<Team>(writePerm: NetworkVariableWritePermission.Owner);

    [Header("Network - Debug dun tath")]
    [SerializeField] private NetworkVariable<float> currentHealth = new NetworkVariable<float>(100);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; 
            return;
        }

        CheckTeam();
        
        // TODO: Make hp not hard set aka use serverrpc to change dis plz ty hahahahaha
        // currentHealth.Value = maxHealth;
        base.OnNetworkSpawn();
    }

    void CheckTeam()
    {
        // automatically assign team based on the controller
        PlayerController playerController = GetComponent<PlayerController>();
        EnemyController enemyController = GetComponent<EnemyController>();
        
        if (playerController != null) { myTeam.Value = Team.Human; }
        else if (enemyController != null) { myTeam.Value = Team.Monster; }
        else { Debug.LogError("Wtf how did we reach here?"); }
    }

    public void SetCurrentHealth(float val)
    {
        //currentHealth.Value = val;
        Debug.Log( currentHealth.Value );
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, Team _team)
    {
        currentHealth.Value -= damage;
        Debug.Log("DAMAGED");
        if (currentHealth.Value <= 0)
        {
            if (_team == Team.Monster)
            {
                Debug.LogWarning("COWABUNGA 1");
                GameManager.Instance.MonsterDead();
            }
            else if (_team == Team.Human)
            {
                Debug.LogWarning("COWABUNGA 2");
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
        TakeDamageServerRpc(damage, myTeam.Value);
    }

}
