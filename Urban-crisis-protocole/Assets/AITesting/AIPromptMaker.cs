using System;
using UnityEngine;

public class AIPromptMaker : MonoBehaviour
{
    AIInfo aiInfo = AIInfo.Instance;

    void Start()
    {

    }

    void Update()
    {

    }

    public string InfoPrompt()
    {
        return
        $"Population: {aiInfo.getPopulation}\n" +
        $"Wood: {aiInfo.getWood}\n" +
        $"Stone: {aiInfo.getStone}";

    }
}
