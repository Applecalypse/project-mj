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
        ulong[] uids = GameManager.Instance.uidToTeam.Keys.ToArray();
        foreach (ulong uid in uids)
        {
            Debug.Log(uid);
            Vector3 randomPositionPlayer = new Vector3(Random.Range(55, 70), 5, Random.Range(55, 70));
            Vector3 randomPositionEnemy = new Vector3(Random.Range(-55, -70), 20, Random.Range(-55, -70));
            Debug.Log(randomPositionPlayer);
            if (GameManager.Instance.uidToTeam[uid] == Team.Human)
            {
                Debug.Log("Human");
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, uid, true, false,
                    position: randomPositionPlayer, rotation: Quaternion.identity);
            }
            else
            {
                Debug.Log("Monster");
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(enemyPrefab, uid, true, false,
                    position: randomPositionEnemy, rotation: Quaternion.identity);
            }
        }
    }
}
