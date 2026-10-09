using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class InfoPanelManager : MonoBehaviour
{
    // text prefabs 
    [SerializeField] private TMP_Text textPrefab;
    [SerializeField] private GameObject context;
    

    

    // a UidataModele is juste oen way to represnte one info(text) but it has to value (1) a datatype that represente what the info is about 
    // (2) the actual value (ex: the price , the name , number of workers etc.. )
  
    void Update()
    {}
    
    // find a way to handle the fact that while buildings are making progresse i need to refresh

    // hint do it in playerinteraction with the current selectedbuilding
    public void ReformatePanel(IBuildingUiData selectedBuilding)
    {

        foreach (Transform child in context.transform.GetComponentInChildren<Transform>())  Destroy(child.gameObject);

        // create text

        List<UiDataText> list = selectedBuilding.GetUiTextList();
        foreach (UiDataText uiText in list)
        {
            AddText(uiText.GetValue);
        }

        // create interaction button


    }

    void AddText(string text)
    {
        TMP_Text newText = Instantiate(textPrefab, context.transform);
        newText.text = text;
    }

    
}
