using System;
using TMPro;
using UnityEngine;

public class ResourceUIManager : MonoBehaviour
{
    [SerializeField] private PlayerResources playerResource;
    [SerializeField] private TMP_Text woodText;

    void Awake()
    {
       playerResource.OnWoodAdded += UpdateWoodText;
    }
    private void UpdateWoodText()
    {
        woodText.text = $"Wood: {playerResource.Wood}";
    }
}