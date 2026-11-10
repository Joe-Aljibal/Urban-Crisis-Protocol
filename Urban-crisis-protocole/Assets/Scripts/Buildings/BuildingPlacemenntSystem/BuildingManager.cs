
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


public class BuildingManager : MonoBehaviour 
{

    public GameObject CreateBuilding(GameObject buildingPrefab, Vector3 position, Quaternion rotation)
    {
        return Instantiate(buildingPrefab, position, rotation);
    }
    
}









