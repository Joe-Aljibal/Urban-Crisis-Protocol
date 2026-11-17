using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;
using System;
using Mono.Cecil.Cil;
using System.Data.SqlTypes;

public class AIAgent : Agent
{

    public event Action OnConstruire;
    public event Action OnDetruire;
    public event Action OnAmeliorer;

    private Dictionary<int, System.Action> actionMap;
    public override void Initialize()
    {
        actionMap = new Dictionary<int, System.Action>()
    {
        { 1, OnConstruire },
        { 2,  OnDetruire },
        { 3, OnAmeliorer}
    };
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(PlayerInfo.Instance.getMoney);
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        actionMap[action]?.Invoke();
    }

}
