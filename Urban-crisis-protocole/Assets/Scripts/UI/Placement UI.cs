using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementUI : MonoBehaviour
{
    bool closingUI = false;
    PointerHandler script;

    private void Start()
    {
        script = FindFirstObjectByType<PointerHandler>();
    }

    public void CloseUI()
    {
        if (!closingUI)
            StartCoroutine(pullUIDown());
    }

    IEnumerator pullUIDown()
    {
        closingUI = true;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 position = rectTransform.position;
        float originalYPosition = rectTransform.position.y;
        float height = rectTransform.sizeDelta.y;

        while (rectTransform.position.y > originalYPosition - height)
        {
            rectTransform.position -= new Vector3(0, height / 25, 0);
            yield return  new WaitForSeconds(.01f);
        }
        gameObject.SetActive(false);
        closingUI = false;
    }


    public void selectBuilding()
    {
        string buildingName = EventSystem.current.currentSelectedGameObject.name;
        GameObject ghost = Resources.Load<GameObject>($"All Buildings/{buildingName} Ghost");
        GameObject building = Resources.Load<GameObject>($"All Buildings/{buildingName}");

        if (script.currentGhost != null)
            Destroy(script.currentGhost.gameObject);
        
        script.currentGhost = Instantiate(ghost);
        script.currentBuilding = building;
    }
}