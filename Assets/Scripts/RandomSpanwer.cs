using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Netcode;
using Unity.VisualScripting;

public class RandomSpanwer : MonoBehaviour
{

    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private NetworkObject enemyPrefab;

    private void Start()
    {
        Debug.Log("spawn");
        ulong[] uids = GameManager.Instance.uidToTeam.Keys.ToArray();
        foreach (ulong uid in uids)
        {
            Vector3 randomPositionPlayer = new Vector3(Random.Range(55, 70), 10, Random.Range(-70, 70));
            Vector3 randomPositionEnemy = new Vector3(Random.Range(-55, -70), 10, Random.Range(-70, 70));
            if (GameManager.Instance.uidToTeam[uid] == Team.Human)
            {
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, uid, true, false,
                    position: randomPositionPlayer, rotation: Quaternion.identity);
            }
            else
            {
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(enemyPrefab, uid, true, false,
                    position: randomPositionEnemy, rotation: Quaternion.identity);
            }
        }
    }
}