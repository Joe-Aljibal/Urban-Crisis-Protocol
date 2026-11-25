using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GamblingPanelUI : MonoBehaviour
{
    private readonly Image[] reels = new Image[3];
    private TMP_Text totalMoneyText;
    private TMP_Text resultText;
    private Button gambleButton;
    private GambleLogic gambleLogic;
    private bool isSpinning;

    private static readonly Color[] ReelColors =
    {
        new Color(0.15f, 0.75f, 0.25f),
        new Color(0.95f, 0.75f, 0.1f),
        new Color(0.85f, 0.2f, 0.18f)
    };

    public static GamblingPanelUI Create(GambleLogic owner)
    {
        EnsureEventSystem();
        GameObject canvasObject = new GameObject("Gambling Panel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GamblingPanelUI panel = canvasObject.AddComponent<GamblingPanelUI>();
        panel.gambleLogic = owner;
        panel.Build();
        return panel;
    }

    public void Show(float totalMoney)
    {
        gameObject.SetActive(true);
        UpdateMoney(totalMoney);
        resultText.text = "Match all three green squares to win!";
    }

    public void Close()
    {
        StopAllCoroutines();
        isSpinning = false;
        gambleButton.interactable = true;
        gameObject.SetActive(false);
    }

    public void ShowResult(float totalMoney, float change)
    {
        UpdateMoney(totalMoney);
        resultText.text = change >= 0f ? $"You won {change:0.##}!" : $"You lost {Mathf.Abs(change):0.##}.";
    }

    private void Build()
    {
        RectTransform panelRect = CreateRect("Panel", transform);
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(390f, 300f);
        Image background = panelRect.gameObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.10f, 0.14f, 0.96f);

        totalMoneyText = CreateText("Total Money", panelRect, new Vector2(0f, 112f), new Vector2(350f, 42f), 25f);
        resultText = CreateText("Result", panelRect, new Vector2(0f, -12f), new Vector2(350f, 38f), 18f);

        for (int i = 0; i < reels.Length; i++)
        {
            RectTransform reel = CreateRect($"Reel {i + 1}", panelRect);
            reel.anchorMin = reel.anchorMax = new Vector2(0.5f, 0.5f);
            reel.pivot = new Vector2(0.5f, 0.5f);
            reel.anchoredPosition = new Vector2((i - 1) * 86f, 52f);
            reel.sizeDelta = new Vector2(64f, 64f);
            reels[i] = reel.gameObject.AddComponent<Image>();
            reels[i].color = ReelColors[Random.Range(0, ReelColors.Length)];
        }

        RectTransform buttonRect = CreateRect("Play Button", panelRect);
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -92f);
        buttonRect.sizeDelta = new Vector2(190f, 48f);
        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.48f, 0.78f);
        gambleButton = buttonRect.gameObject.AddComponent<Button>();
        gambleButton.targetGraphic = buttonImage;
        gambleButton.onClick.AddListener(() => { if (!isSpinning) StartCoroutine(Spin()); });
        CreateText("Button Label", buttonRect, Vector2.zero, new Vector2(180f, 40f), 20f).text = "Play";

        RectTransform closeRect = CreateRect("Close Casino Button", panelRect);
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-6f, -6f);
        closeRect.sizeDelta = new Vector2(44f, 44f);
        Image closeImage = closeRect.gameObject.AddComponent<Image>();
        closeImage.color = new Color(0.7f, 0.12f, 0.12f);
        Button closeButton = closeRect.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(Close);
        CreateText("Close Label", closeRect, Vector2.zero, new Vector2(40f, 40f), 28f).text = "X";
    }

    private IEnumerator Spin()
    {
        isSpinning = true;
        gambleButton.interactable = false;
        resultText.text = "Spinning...";
        int[] outcomes = new int[reels.Length];
        float[] stopTimes = new float[reels.Length];
        float[] elapsedTimes = new float[reels.Length];
        float[] changeTimers = new float[reels.Length];
        bool[] stopped = new bool[reels.Length];

        for (int i = 0; i < reels.Length; i++)
            stopTimes[i] = Random.Range(2f, 3f);

        float spinElapsed = 0f;
        while (spinElapsed < 3f)
        {
            float deltaTime = Time.deltaTime;
            spinElapsed += deltaTime;
            for (int i = 0; i < reels.Length; i++)
            {
                if (stopped[i])
                    continue;

                elapsedTimes[i] += deltaTime;
                if (elapsedTimes[i] >= stopTimes[i])
                {
                    outcomes[i] = Random.Range(0, ReelColors.Length);
                    reels[i].color = ReelColors[outcomes[i]];
                    stopped[i] = true;
                }
                else
                {
                    float slowdownStart = stopTimes[i] * 0.6f;
                    float slowdownProgress = Mathf.InverseLerp(slowdownStart, stopTimes[i], elapsedTimes[i]);
                    float changeInterval = Mathf.Lerp(0.07f, 0.35f, slowdownProgress);
                    changeTimers[i] += deltaTime;

                    if (changeTimers[i] >= changeInterval)
                    {
                        reels[i].color = ReelColors[Random.Range(0, ReelColors.Length)];
                        changeTimers[i] = 0f;
                    }
                }
            }

            yield return null;
        }

        bool won = outcomes[0] == 0 && outcomes[1] == 0 && outcomes[2] == 0;
        gambleLogic.Gamble(won);
        isSpinning = false;
        gambleButton.interactable = true;
    }

    private void UpdateMoney(float amount) => totalMoneyText.text = $"Total money: {amount:0.##}";

    private static TMP_Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        if (text.fontSharedMaterial != null)
        {
            text.fontMaterial = new Material(text.fontSharedMaterial);
            text.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
        }
        text.fontSize = fontSize;
        text.color = Color.white;
        text.faceColor = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child.GetComponent<RectTransform>();
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
}
