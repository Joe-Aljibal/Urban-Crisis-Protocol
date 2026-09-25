using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class is responsible for managing the amount of moving NPCs in game.
/// </summary>

public class MovingNpcManager : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int maxMovingNPC = 10;
    private int movingNpcCount;
    
    private List<GameObject> movingNpcs = new List<GameObject>();

    void Update()
    {
         if (Keyboard.current.tabKey.isPressed)
        {
            Time.timeScale = 100.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
        CalculateNpcsFromPopulation();
        ManageMovingNpcs();
        //Debug.Log("Available Population: " + playerInfo.getAvailablePopulation + " Moving NPCs: " + movingNpcCount);
    }

    private void CalculateNpcsFromPopulation()
    {
        movingNpcCount = Mathf.Clamp(playerInfo.getAvailablePopulation / 5, 0, maxMovingNPC);
    }

    private void ManageMovingNpcs()
    {
        if (movingNpcCount > movingNpcs.Count)
        {
            int npcsToSpawn = movingNpcCount - movingNpcs.Count;
            for (int i = 0; i < npcsToSpawn; i++)
            {
                GameObject newNpc = Instantiate(npcPrefab, transform.position, Quaternion.identity);
                movingNpcs.Add(newNpc);
            }
        }
        else if (movingNpcCount < movingNpcs.Count)
        {
            int npcsToRemove = movingNpcs.Count - movingNpcCount;
            for (int i = 0; i < npcsToRemove; i++)
            {
                GameObject npcToRemove = movingNpcs[movingNpcs.Count - 1];
                movingNpcs.RemoveAt(movingNpcs.Count - 1);
                Destroy(npcToRemove);
            }
        }
    }
}
