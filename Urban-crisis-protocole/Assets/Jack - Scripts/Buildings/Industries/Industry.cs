using Unity.VisualScripting;
using UnityEngine;

public class Industry : MonoBehaviour
{
    float cooldown = 0;
    private void Update()
    {
        if (cooldown >= 1)
        {
            PlayerInfo.Instance.addWater(1);
            cooldown = 0;
        }
        cooldown += Time.deltaTime;
    }
}
