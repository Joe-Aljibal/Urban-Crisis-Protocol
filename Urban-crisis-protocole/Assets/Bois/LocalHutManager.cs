using UnityEngine;

public class LocalHutManager : MonoBehaviour
{
    [SerializeField] HutData hutData;
    private GameObject ResourceManager { get; set; }
    private HutStation hutStation;

    void Start()
    {
        hutStation = GetComponent<HutStation>();
       // hutStation.InitializeBuilding()
    }

    void Update()
    {
        
    }
}