using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    // text prefabs 
    [SerializeField] private TMP_Text textPrefab;

    [SerializeField] private Button[] interactionButtonPrefabs;
    Dictionary<UiInteractionType, Button> interactionList = new Dictionary<UiInteractionType, Button>();

    [SerializeField] private GameObject context;
    

    
    void Awake()
    {
        interactionList[UiInteractionType.SELL] = interactionButtonPrefabs[0];

    }
    // a UidataModele is juste oen way to represnte one info(text) but it has to value (1) a datatype that represente what the info is about 
    // (2) the actual value (ex: the price , the name , number of workers etc.. )
  
    void Update()
    {}
    
    // find a way to handle the fact that while buildings are making progresse i need to refresh

    // hint do it in playerinteraction with the current selectedbuilding
    public void ReformatePanel(IBuildingUiData selectedBuilding, GameObject buildingSelected)
    {
        foreach (Transform child in context.transform.GetComponentInChildren<Transform>())  Destroy(child.gameObject);

        // create text 

        List<UiDataText> listText = selectedBuilding.GetUiTextList();
        foreach (UiDataText uiText in listText)
        {
            AddText(uiText.GetValue);
        }

        // create interaction button

        List<UiDataInteraction> listInteraction = selectedBuilding.GetUiInteractionList();

        foreach (UiDataInteraction interactionData in listInteraction)
        {
            CreateInteractionButton(interactionData, buildingSelected);
        }


    }

    void AddText(string text)
    {
        TMP_Text newText = Instantiate(textPrefab, context.transform);
        newText.text = text;
    }

    void CreateInteractionButton(UiDataInteraction interactionData, GameObject buildingSelected)
    {
        
       Button btn = Instantiate(interactionList[interactionData.GetUiInteractionType], context.transform);
       btn.GetComponent<Button>().onClick.AddListener(() =>
            {
               interactionData.GetUiInteraction.Interact(buildingSelected);
            });
    }
    
}
