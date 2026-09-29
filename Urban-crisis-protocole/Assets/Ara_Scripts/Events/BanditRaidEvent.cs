using System.Collections;
using UnityEngine;

public class BanditRaidEvent : MonoBehaviour
{
    [System.Serializable]
    public class MoneyBuilding
    {
        public Transform building;
        public Transform[] approachPoints;
        public Transform insidePoint;
    }

    [SerializeField]
    private Bandit banditPrefab;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private MoneyBuilding[] moneyBuildings;

    [SerializeField]
    private int banditCount = 3;

    [SerializeField]
    private float spawnInterval = 1.5f;

    [SerializeField]
    private float moneyPerBandit = 100;

    [SerializeField]
    private bool startAutomatically = true;

    [SerializeField]
    private float startDelay = 2;

    private bool raidRunning;
    private int remainingBandits;
    private float totalStolen;
    private string message = "Prêt pour le raid.";

    private IEnumerator Start()
    {
        if (startAutomatically)
        {
            yield return new WaitForSeconds(startDelay);
            StartRaid();
        }
    }

    public void StartRaid()
    {
        if (raidRunning)
            return;

        raidRunning = true;
        remainingBandits = banditCount;
        totalStolen = 0;

        message = "Les bandits arrivent !";

        StartCoroutine(SpawnBandits());
    }

    private IEnumerator SpawnBandits()
    {
        for (int i = 0; i < banditCount; i++)
        {
            // Répartit les bandits entre les bâtiments configurés.
            MoneyBuilding target = moneyBuildings[i % moneyBuildings.Length];

            Bandit bandit = Instantiate(
                banditPrefab,
                spawnPoint.position,
                Quaternion.identity,
                transform
            );

            bandit.BeginRaid(this, target, spawnPoint.position);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public float StealMoney(MoneyBuilding target)
    {
        float availableMoney = Mathf.Max(0, PlayerInfo.Instance.getMoney);
        float stolenMoney = Mathf.Min(moneyPerBandit, availableMoney);

        PlayerInfo.Instance.addMoney(-stolenMoney);
        totalStolen += stolenMoney;

        message = target.building.name + " : " + stolenMoney + " $ volés.";

        return stolenMoney;
    }

    public void BanditEscaped()
    {
        remainingBandits--;

        if (remainingBandits == 0)
        {
            raidRunning = false;
            message = "Le raid est terminé.";
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(15, 15, 330, 200), GUI.skin.box);

        GUILayout.Label("RAID DES BANDITS");
        GUILayout.Label("Argent : " + PlayerInfo.Instance.getMoney + " $");
        GUILayout.Label("Argent volé : " + totalStolen + " $");
        GUILayout.Label("Bandits restants : " + remainingBandits);
        GUILayout.Label(message);

        if (!raidRunning && GUILayout.Button("Lancer le raid"))
        {
            StartRaid();
        }

        GUILayout.EndArea();
    }
}
