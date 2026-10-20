using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System;
using LLMUnity;
using System.Collections;
using System.Threading;

[AttributeUsage(AttributeTargets.Method)] 
public class AICallableAttribute : Attribute { } // Custom attribute to find when calling functions
    public static class AIEvents
    {
        public static event Action OnAISpawnHouse;
        public static event Action OnAISpawnGasStation;
        public static event Action OnAICollectWood;
        public static event Action OnAICollectStone;

        [AICallable]
        public static void SpawnHouse()
        {
            OnAISpawnHouse?.Invoke();
        }
        [AICallable]
        public static void SpawnGasStation()
        {
            OnAISpawnGasStation?.Invoke();
        }
        /*[AICallable]
        public static void CollectWood()
        {
            OnAICollectWood?.Invoke();
        }
        [AICallable]
        public static void CollectStone()
        {
            OnAICollectStone?.Invoke();
        }*/
    }
public class FunctionCalling : MonoBehaviour
{
    [SerializeField] private LLMAgent llmAgent;

    [SerializeField] private AIPromptMaker promptMaker;

    private bool running;
    private bool busy;

    Coroutine decisionCoroutine;

    void Start()
    {
        llmAgent.grammar = MultipleChoiceGrammar();

        decisionCoroutine = StartCoroutine(DecisionRoutine());
    }

    string[] GetFunctionNames()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(AIEvents).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if(function.GetCustomAttribute<AICallableAttribute>() == null) continue;
            functionNames.Add(function.Name);
        }
        return functionNames.ToArray();
    }

    string ConstructPrompt(string message)
    {
        string gameplayPrompt = "Game info:\n\n" + promptMaker.InfoPrompt() + "\n\n\n";
        string prompt = "Decide what you want to do based on the input?\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames()) prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice";
        return gameplayPrompt + prompt;
    }

    void CallFunction(string functionName)
    {
        typeof(AIEvents).GetMethod(functionName).Invoke(null, null);
    }

    string MultipleChoiceGrammar()
    {
        return "root ::= (\"" + string.Join("\" | \"", GetFunctionNames()) + "\")";
    }

    async Task MakeDecision()
    {
        if(busy)
        {
            Debug.LogWarning("Could not make decision because busy");
            return;
        }
        
        busy = true;
        try
        {
            string decision = await llmAgent.Chat(ConstructPrompt("Spawn a house or a gas station"));
            if (this == null) return; // destroyed while waiting
            Debug.Log(ConstructPrompt("Spawn a house or a gas station"));
            Debug.Log($"Decision: {decision}");
            CallFunction(decision);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            busy = false;
        }
    }

    bool TryMakeDecision()
    {
        if (decisionCoroutine != null)
            return false;

        decisionCoroutine = StartCoroutine(DecisionRoutine());
        return true;
    }

    IEnumerator DecisionRoutine()
    {
        Task decisionTask = MakeDecision();
        while (!decisionTask.IsCompleted)
            yield return null;

        yield return new WaitForSeconds(3);

        decisionCoroutine = null;

        if (!TryMakeDecision())
        {
            StartCoroutine(RetryRoutine());
        }
    }

    IEnumerator RetryRoutine()
    {
        yield return new WaitForSeconds(2);

        TryMakeDecision();
    }
}
