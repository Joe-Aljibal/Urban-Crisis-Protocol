using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    private const float HoursPerDay = 24f;
    private const float DegreesPerRotation = 360f;
    private const float SunriseHour = 6f;
    private const float SunAngleOffset = 90f;
    private const float FullRotationInRadians = Mathf.PI * 2f;
    private const float DefaultDayDuration = 60f;

    [SerializeField] private float fullDayDuration = DefaultDayDuration;
    [SerializeField] private float startingHour = 8f;
    [SerializeField] private float sunYaw = -30f;
    [SerializeField] private float dayIntensity = 1.1f;
    [SerializeField] private float nightIntensity = 0.08f;
    [SerializeField] private float dayAmbientIntensity = 1f;
    [SerializeField] private float nightAmbientIntensity = 0.2f;
    [SerializeField] private float debugTimeJump = 1f;
    [SerializeField] private Color dayColor = new Color(1f, 0.96f, 0.84f);
    [SerializeField] private Color nightColor = new Color(0.25f, 0.35f, 0.55f);
    [SerializeField] private Light[] nightLights = new Light[0];
    [SerializeField, Range(0f, 24f)] private float lightsOnHour = 18f;
    [SerializeField, Range(0f, 24f)] private float lightsOffHour = 6f;

    private Light sun;
    private float currentHour;
    private bool? nightLightsEnabled;
    private bool powerAvailable = true;

    public void SetCityPower(bool available)
    {
        powerAvailable = available;
        nightLightsEnabled = null;
        UpdateLighting();
    }

    public float GetCurrentHour()
    {
        return currentHour;
    }

    public bool IsNight => lightsOnHour > lightsOffHour
        ? currentHour >= lightsOnHour || currentHour < lightsOffHour
        : currentHour >= lightsOnHour && currentHour < lightsOffHour;

    public void RegisterNightLight(Light cityLight)
    {
        if (cityLight == null) return;
        var lights = new System.Collections.Generic.List<Light>(nightLights);
        if (lights.Contains(cityLight)) return;
        lights.Add(cityLight);
        nightLights = lights.ToArray();
        cityLight.enabled = IsNight && powerAvailable;
    }

    public float GetDebugTimeJump()
    {
        return debugTimeJump;
    }

    private void Awake()
    {
        if (fullDayDuration <= 0f)
        {
            fullDayDuration = DefaultDayDuration;
        }

        sun = GetComponent<Light>();
        currentHour = Mathf.Repeat(startingHour, HoursPerDay);
        RenderSettings.sun = sun;
        UpdateLighting();
    }

    private void Update()
    {
        float hoursPerSecond = HoursPerDay / fullDayDuration;
        currentHour = Mathf.Repeat(
            currentHour + hoursPerSecond * Time.deltaTime,
            HoursPerDay
        );

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AdvanceTime();
            return;
        }

        UpdateLighting();
    }

    public void AdvanceTime()
    {
        currentHour = Mathf.Repeat(
            currentHour + debugTimeJump,
            HoursPerDay
        );

        UpdateLighting();
    }

    private void UpdateLighting()
    {
        float sunAngle =
            currentHour / HoursPerDay * DegreesPerRotation - SunAngleOffset;
        transform.rotation = Quaternion.Euler(sunAngle, sunYaw, 0f);

        float daylight = Mathf.Sin(
            (currentHour - SunriseHour) / HoursPerDay * FullRotationInRadians
        );

        if (daylight < 0f)
        {
            daylight = 0f;
        }

        sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, daylight);
        sun.color = Color.Lerp(nightColor, dayColor, daylight);
        RenderSettings.ambientIntensity = Mathf.Lerp(
            nightAmbientIntensity,
            dayAmbientIntensity,
            daylight
        );

        bool isNight = IsNight && powerAvailable;
        if (nightLightsEnabled != isNight)
        {
            nightLightsEnabled = isNight;
            foreach (Light cityLight in nightLights)
            {
                if (cityLight != null)
                    cityLight.enabled = isNight;
            }
        }
    }
}
