using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunCycle : MonoBehaviour
{
    [Header("Gün Döngüsü Ayarlarý")]
    public float cycleSpeed = 10f;
    public Gradient lightColorGradient;
    public AnimationCurve intensityCurve;

    private Light sunLight;
    private float timeOfDay = 0f;

    void Start()
    {
        sunLight = GetComponent<Light>();

        // Eðer editorde tanýmlanmadýysa, otomatik olarak gradient oluþtur
        if (lightColorGradient.colorKeys.Length == 0)
        {
            lightColorGradient = CreateDefaultGradient();
        }

        if (intensityCurve.length == 0)
        {
            intensityCurve = CreateDefaultCurve();
        }
    }

    void Update()
    {
        timeOfDay += Time.deltaTime * cycleSpeed / 100f;
        if (timeOfDay > 1f) timeOfDay = 0f;

        float sunRotation = timeOfDay * 360f - 90f;
        transform.rotation = Quaternion.Euler(sunRotation, 170f, 0f);

        sunLight.color = lightColorGradient.Evaluate(timeOfDay);
        sunLight.intensity = intensityCurve.Evaluate(timeOfDay);
    }

    private Gradient CreateDefaultGradient()
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[5];
        colorKeys[0].color = new Color(1.0f, 0.6f, 0.3f); // Gün doðumu
        colorKeys[0].time = 0.0f;

        colorKeys[1].color = Color.white; // Öðlen
        colorKeys[1].time = 0.25f;

        colorKeys[2].color = new Color(1.0f, 0.5f, 0.2f); // Gün batýmý
        colorKeys[2].time = 0.5f;

        colorKeys[3].color = new Color(0.2f, 0.25f, 0.4f); // Gece
        colorKeys[3].time = 0.75f;

        colorKeys[4].color = new Color(1.0f, 0.6f, 0.3f); // Sabah tekrar
        colorKeys[4].time = 1.0f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    private AnimationCurve CreateDefaultCurve()
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.3f);   // Sabah az ýþýk
        curve.AddKey(0.25f, 1.0f);  // Öðle en parlak
        curve.AddKey(0.5f, 0.5f);   // Gün batýmý düþüþ
        curve.AddKey(0.75f, 0.0f);  // Gece karanlýk
        curve.AddKey(1.0f, 0.3f);   // Sabah tekrar

        return curve;
    }
}
