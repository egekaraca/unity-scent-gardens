using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AromaShooterManager_Deneme : MonoBehaviour
{
    public Transform player;
    public Transform rose;
    public float triggerDistance = 2f;

    private bool hasDiffusedRose = false;
    private int aromaShooterPort = 1003;

    void Update()
    {
        float distanceToRose = Vector3.Distance(player.position, rose.position);

        if (distanceToRose < triggerDistance && !hasDiffusedRose)
        {
            Debug.Log("Oyuncu güle yaklaþtý. Gül kokusu yayýlýyor...");

            string ip = SerialNumberManager.Instance.GetDeviceIP();
            if (!string.IsNullOrEmpty(ip))
                StartCoroutine(SendDiffuseRequest(ip));

            hasDiffusedRose = true;
            StartCoroutine(ResetDiffusionFlag(5f));
        }
    }

    IEnumerator SendDiffuseRequest(string ip)
    {
        string url = $"http://{ip}:{aromaShooterPort}/as2/diffuse";

        string json = @"
        {
            ""channels"": [3],
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
                Debug.LogError("Koku gönderme hatasý: " + request.error);
            }
            else
            {
                Debug.Log("Koku baþarýyla gönderildi: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator ResetDiffusionFlag(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        hasDiffusedRose = false;
    }
}
