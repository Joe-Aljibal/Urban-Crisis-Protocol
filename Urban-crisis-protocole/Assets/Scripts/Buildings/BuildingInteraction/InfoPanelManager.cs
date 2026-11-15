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
    

    // i divide the info in a text (text that give context || actuel value)
    // this dictionnare gives me the first part of the info depending on the datatype which we can get when we receive a UidataModele

    // a UidataModele is juste oen way to represnte one info(text) but it has to value (1) a datatype that represente what the info is about 
    // (2) the actual value (ex: the price , the name , number of workers etc.. )
    Dictionary<UiDataType, string> texts = new Dictionary<UiDataType, string>();
    void Awake()
    {

        texts[UiDataType.NAME] = "";
        texts[UiDataType.PRICE] = "Current price : ";
    }

  
    void Update()
    {}
    
    // find a way to handle the fact that while buildings are making progresse i need to refresh

    // hint do it in playerinteraction with the current selectedbuilding
    public void ReformatePanel(IBuildingUiData selectedBuilding)
    {

        foreach (Transform child in context.transform.GetComponentInChildren<Transform>())  Destroy(child.gameObject);

        List<UiDataModel> list = selectedBuilding.GetUiDataList();
        foreach (UiDataModel uiText in list)
        {
            AddText(texts[uiText.GetUiDataType] + uiText.GetValue);
        }
    }

    void AddText(string text)
    {
        TMP_Text newText = Instantiate(textPrefab, context.transform);
        newText.text = text;
    }

    
}
