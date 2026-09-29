//This class lets to spawn the atomic bomb
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject bombPrefab;

    [SerializeField]
    private Transform spawnPoint;
    private GameObject currentBomb;

    private void Start()
    {
        SpawnBomb();
    }

    private void OnMoneyChanged(int newAmount)
    {
        bool canAppear = CheckMoneyRequirement(newAmount);

        if (canAppear)
        {
            SpawnBomb();
        }
    }

    //For the moment the money is the only criteria to spawn a bomb
    private bool CheckMoneyRequirement(int currentMoney)
    {
        bool enough = false;
        int requiredAmount = 1000000;

        if (currentMoney <= requiredAmount)
        {
            return enough;
        }

        return enough;
    }

    private GameObject SpawnBomb()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = GetSpawnRotation();
        GameObject newBomb = Instantiate(bombPrefab, spawnPosition, spawnRotation);
        SetCurrentBomb(newBomb);
        return newBomb;
    }

    private Vector3 GetSpawnPosition()
    {
        return spawnPoint.position;
    }

    private Quaternion GetSpawnRotation()
    {
        return spawnPoint.rotation;
    }

    private GameObject CreateBomb(Vector3 position, Quaternion rotation)
    {
        GameObject newBomb = Instantiate(bombPrefab, position, rotation);
        return newBomb;
    }

    private void SetCurrentBomb(GameObject bomb)
    {
        currentBomb = bomb;
    }
}
