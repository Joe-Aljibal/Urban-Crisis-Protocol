using TMPro;
using UnityEngine;

/// <summary>
/// This class is responsible for managing the UI of the WorkStation buildings. 
/// It handles the assignment and removal of workers to and from the building, 
/// as well as verifying the input from the user.
/// </summary>

public class WorkStationUI : MonoBehaviour
{
    private PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] private GameObject CustomNumberInputField;
    private WorkStation currentBuilding;

    public void SetCurrentBuilding(WorkStation building)
    {
        currentBuilding = building;
    }

    public void AssignWorkersToStation()
    {
        int workerCount = VerifyInputField();
        if (!CanAddWorkers(workerCount))
        {
            return;
        }
        if (workerCount > currentBuilding.BuildingCapacity - currentBuilding.Workers)
        {
            workerCount = currentBuilding.BuildingCapacity - currentBuilding.Workers;
        }
        currentBuilding.AssignWorkers(workerCount);
    }

    public void RemoveWorkersFromStation()
    {
        if (currentBuilding.Workers == 0) return;
        currentBuilding.RemoveWorkers(VerifyInputField());
    }

    public void FillStation()
    {
        if (currentBuilding.Workers == currentBuilding.BuildingCapacity) return;
        int workersToAdd = currentBuilding.BuildingCapacity - currentBuilding.Workers;
        if (CanAddWorkers(workersToAdd))
        {
            currentBuilding.AssignWorkers(workersToAdd);
        }
    }

    public void EmptyStation()
    {
        if (currentBuilding.Workers == 0) return;
        currentBuilding.RemoveWorkers(currentBuilding.Workers);
    }

    private int VerifyInputField()
    {
        if (int.TryParse(CustomNumberInputField.
        GetComponent<TMP_InputField>().text, out int workerCount) && workerCount > 0)
        {
            return workerCount;
        }
        return 1;
    }

    private bool CanAddWorkers(int workerCount)
    {
        return workerCount <= playerInfo.getPopulation && workerCount <= currentBuilding.BuildingCapacity - currentBuilding.Workers;
    }

    public void ResetPlaceHolder()
    {
        CustomNumberInputField.GetComponent<TMP_InputField>().text = "Custom Number";
    }

    /*public void ModifyWorkersInStation()
    {
        int workerCount = VerifyInputField();
        if(workerCount > 0)
        {
            currentBuilding.AssignWorkers(workerCount);
        }
        else
        {
            currentBuilding.RemoveWorkers(workerCount);
        }
    }*/
}
