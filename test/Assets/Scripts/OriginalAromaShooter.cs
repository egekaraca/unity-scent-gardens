using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class OriginalAromaShooter : MonoBehaviour
{
    public Transform player; // Oyuncu nesnesi
    public Transform rose;   // G�l nesnesi
    public float triggerDistance = 2f; // Mesafe e�i�i

    private bool hasDiffusedRose = false;

    // Aroma Shooter IP ve port
    private string aromaShooterIP = "ASN2A01058.local";
    private int aromaShooterPort = 1003;

    void Update()
    {
        float distanceToRose = Vector3.Distance(player.position, rose.position);

        if (distanceToRose < triggerDistance && !hasDiffusedRose)
        {
            Debug.Log("Oyuncu g�le yakla�t�. G�l kokusu yay�l�yor...");
            StartCoroutine(SendDiffuseRequest());
            hasDiffusedRose = true;
            StartCoroutine(ResetDiffusionFlag(5f));
        }
    }

    IEnumerator SendDiffuseRequest()
    {
        string url = $"http://{aromaShooterIP}:{aromaShooterPort}/as2/diffuse";

        // curl �rne�in JSON'u tam uyumlu �ekilde
        string json = @"
        {
            ""channels"": [1],
            ""intensities"": [100],
            ""durations"": [2000],
            ""booster"": true
        }";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("Koku g�nderme hatas�: " + request.error);
            }
            else
            {
                Debug.Log("Koku ba�ar�yla g�nderildi: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator ResetDiffusionFlag(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        hasDiffusedRose = false;
    }
}