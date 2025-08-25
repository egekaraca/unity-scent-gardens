using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AromaShooterTestManager : MonoBehaviour
{
    public TMP_Text statusText;
    private int aromaShooterPort = 1003;

    public void TestAromaShooter()
    {
        string ip = SerialNumberManager.Instance.GetDeviceIP();

        if (string.IsNullOrEmpty(ip))
        {
            statusText.text = "Status: No serial number set!";
            statusText.color = Color.red;
            return;
        }

        statusText.text = "Status: Sending test scent...";
        statusText.color = Color.yellow;
        StartCoroutine(SendDiffuseRequest(ip));
    }

    IEnumerator SendDiffuseRequest(string ip)
    {
        string url = $"http://{ip}:{aromaShooterPort}/as2/diffuse";

        string json = @"
        {
            ""channels"": [3],
            ""intensities"": [100],
            ""durations"": [2500],
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
                statusText.text = "Status: Failed to send scent.";
                statusText.color = Color.red;
                Debug.LogError("Test scent error: " + request.error);
            }
            else
            {
                statusText.text = "Status: Scent sent!";
                statusText.color = Color.green;
                Debug.Log("Scent successfully sent: " + request.downloadHandler.text);
            }
        }
    }
}
