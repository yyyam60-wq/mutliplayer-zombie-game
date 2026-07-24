using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ZombieSpawner : MonoBehaviourPunCallbacks
{
    public static ZombieSpawner instance;

    private Player playerHealth;
    [HideInInspector]public PhotonView ThePhotonView;

    public Zombie ZombieOffline;

    public Transform[] SpawnPoints;

    [Header("Spawning Settings")]

    public int maxZombieSpawn;
    public int ZombieSpawned;
    public float spawnDurations;

    [Header("Zombie Power")]

    public float ZHealth = 100f;
    public float ZDamage = 20f;
    public float ZAgentSpeed = 1;
    public float ZAnimationSpeed = 1f;

    private void Awake()
    {
        instance = this;
        ThePhotonView = GetComponent<PhotonView>();
    }

    public void RedefineData() 
    {
        playerHealth = FindObjectOfType<Player>();

        StartCoroutine(newSpawn());
        StartCoroutine(IncreasePower());
    }

    public void SetDefualtData() 
    {
        StopAllCoroutines();

        //Spawning Settings
        maxZombieSpawn = 5;
        ZombieSpawned = 0;
        spawnDurations = 10f;

        //Zombie Power
        ZHealth = 100f;
        ZDamage = 20f;
        ZAgentSpeed = 1;
        ZAnimationSpeed = 1f;
    }

    IEnumerator newSpawn() 
    {
        if (playerHealth.isDead) yield break;

        yield return new WaitForSeconds(spawnDurations);
        GetSpawnPoint();
    }

    void GetSpawnPoint()
    {
        if (!photonView.IsMine) return;

        if (ZombieSpawned >= maxZombieSpawn)
        {
            StartCoroutine(newSpawn());
            return;
        }
        else StartCoroutine(newSpawn());

        int RandomSpawnPoint = Random.Range(0, SpawnPoints.Length);

        PhotonNetwork.Instantiate("Zombie", SpawnPoints[RandomSpawnPoint].position, SpawnPoints[RandomSpawnPoint].rotation);

        ZombieSpawned++;
    }

    IEnumerator IncreasePower()
    {
        while (true) 
        {
            yield return new WaitForSeconds(45f);
            if(spawnDurations > 2) spawnDurations -= 2.4f;
            if(maxZombieSpawn < 55) maxZombieSpawn += 5;
            if(ZHealth < 300f) ZHealth += 20f;
            if(ZDamage < 90f) ZDamage += 10f;
            if (ZAgentSpeed < 8) ZAgentSpeed += 1f;
            if (ZAnimationSpeed < 8f) ZAnimationSpeed += 1f;
        }
    }
}
