using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class ConnectManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField serialInput;
    public TMP_Text statusText;

    [Header("Device Host")]
    public string manualHost = "";      // IP/host override
    public int port = 1003;

    [Header("Scent Payload")]
    [Range(1, 6)] public int channel = 1;
    [Range(0, 100)] public int intensity = 100;
    [Min(1)] public int durationMs = 2000;
    public bool booster = true;

    [Header("HTTP")]
    public string diffusePath = "/as2/diffuse";
    public int timeoutSeconds = 6;

    // PlayerPrefs keys (diðer sahnelerde kullanmak için)
    const string PREF_HOST = "aroma_host";
    const string PREF_PORT = "aroma_port";

    public void ConnectToDevice()
    {
        if (serialInput == null || statusText == null) { Debug.LogError("UI refs null"); return; }

        string text = (serialInput.text ?? "").Trim();

        if (string.IsNullOrEmpty(text))
        {
            SetStatus("Status: Please enter a serial or IP!", Color.red);
            return;
        }

        // IP girildiyse: SerialNumberManager'ý atla, direkt manualHost'a yaz + KAYDET
        if (IsIPv4(text))
        {
            manualHost = SanitizeHost(text);
            PlayerPrefs.SetString(PREF_HOST, manualHost);
            PlayerPrefs.SetInt(PREF_PORT, port);
            PlayerPrefs.Save();

            SetStatus($"Status: Saved IP {manualHost}", Color.green);
            Debug.Log($"[ConnectManager] Manual IP set & saved: {manualHost}:{port}");
            return;
        }

        // Seri girildiyse: mevcut davranýþ (seri -> .local)
        if (SerialNumberManager.Instance == null)
        {
            Debug.LogError("SerialNumberManager not found");
            SetStatus("Status: Internal error.", Color.red);
            return;
        }
        SerialNumberManager.Instance.SetSerial(text, autoComputeHost: true);
        SetStatus("Status: Saved Device (serial)", Color.green);
    }

    public void TestScent()
    {
        if (statusText == null)
        {
            Debug.LogError("ConnectManager: statusText is null.");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android'de .local desteklenmiyor: manualHost IPv4 ise onu zorla kullan
        if (!string.IsNullOrWhiteSpace(manualHost) && IsIPv4(manualHost.Trim()))
        {
            // Kullanýcý IP'si kalýcý dursun
            SavePrefsIfIPv4(manualHost);
            StartCoroutine(SendDiffuse(SanitizeHost(manualHost)));
            return;
        }
#endif

        // Öncelik: SerialNumberManager -> manualHost -> input(.local ekle)
        string host = SerialNumberManager.Instance != null
            ? SerialNumberManager.Instance.GetDeviceIP()
            : null;

        if (string.IsNullOrWhiteSpace(host))
            host = manualHost;

        if (string.IsNullOrWhiteSpace(host) && serialInput != null)
        {
            string serial = serialInput.text.Trim();
            if (!string.IsNullOrEmpty(serial) && !serial.EndsWith(".local"))
                serial += ".local";
            host = serial;
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            SetStatus("Status: Device host not set.", Color.red);
            return;
        }

        host = SanitizeHost(host);

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android: .local reddet, IP iste
        if (host.ToLowerInvariant().EndsWith(".local"))
        {
            SetStatus("Status: Android'de .local desteklenmiyor. Lütfen cihazýn IP adresini kullanýn (ör. 192.168.x.x).", Color.red);
            return;
        }
#endif
        // Host IPv4 ise kalýcý kaydet (diðer sahneler kullansýn)
        SavePrefsIfIPv4(host);

        StartCoroutine(SendDiffuse(host));
    }

    private IEnumerator SendDiffuse(string host)
    {
        SetStatus("Status: Testing scent...", new Color(1f, 0.84f, 0f)); // sarý

        string url = $"http://{host}{(port > 0 ? $":{port}" : "")}{NormalizePath(diffusePath)}";

        var payload = new DiffusePayload
        {
            channels = new[] { Mathf.Clamp(channel, 1, 6) },
            intensities = new[] { Mathf.Clamp(intensity, 0, 100) },
            durations = new[] { Mathf.Max(1, durationMs) },
            booster = booster
        };
        string json = JsonUtility.ToJson(payload);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
#if UNITY_2020_2_OR_NEWER
            req.timeout = Mathf.Max(1, timeoutSeconds);
#endif
            Debug.Log($"[ConnectManager] POST {url} body={json}");
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            bool ok = req.result == UnityWebRequest.Result.Success;
#else
            bool ok = !req.isHttpError && !req.isNetworkError;
#endif
            int code = (int)req.responseCode;
            string bodyText = req.downloadHandler != null ? req.downloadHandler.text : "";

            if (!ok)
            {
                string reason = req.error;
                SetStatus($"Status: Test FAILED ({code}) - {reason}", Color.red);
                Debug.LogError($"[ConnectManager] Diffuse FAIL code={code}, err={reason}, resp={bodyText}");
            }
            else
            {
                if (code >= 200 && code < 300)
                {
                    SetStatus($"Status: Test PASSED (scent sent) | Response: {Short(bodyText)}", Color.green);
                    Debug.Log($"[ConnectManager] Diffuse OK code={code}, resp={bodyText}");
                }
                else
                {
                    SetStatus($"Status: Test UNKNOWN ({code})", new Color(1f, 0.84f, 0f));
                    Debug.LogWarning($"[ConnectManager] Diffuse UNKNOWN code={code}, resp={bodyText}");
                }
            }
        }
    }

    // ---- helpers ----
    private void SetStatus(string msg, Color color)
    {
        if (statusText != null)
        {
            statusText.text = msg;
            statusText.color = color;
        }
        Debug.Log(msg);
    }

    private static string NormalizePath(string p)
    {
        if (string.IsNullOrEmpty(p)) return "/";
        return p.StartsWith("/") ? p : "/" + p;
    }

    private static string Short(string s, int max = 160)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }

    // Host'u temizle: görünmez unicode boþluklarý ve çevresel boþluklarý at
    private static string SanitizeHost(string host)
    {
        if (string.IsNullOrEmpty(host)) return host;
        host = host.Trim();
        host = host.Replace("\u200B", "").Replace("\u200E", "").Replace("\u200F", "");
        return host;
    }

    // Basit IPv4 doðrulayýcý
    private static bool IsIPv4(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var parts = s.Split('.');
        if (parts.Length != 4) return false;
        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out int v)) return false;
            if (v < 0 || v > 255) return false;
        }
        return true;
    }

    private void SavePrefsIfIPv4(string host)
    {
        if (IsIPv4(host))
        {
            PlayerPrefs.SetString(PREF_HOST, host);
            PlayerPrefs.SetInt(PREF_PORT, port);
            PlayerPrefs.Save();
            Debug.Log($"[ConnectManager] Saved host to prefs: {host}:{port}");
        }
    }

    [System.Serializable]
    private class DiffusePayload
    {
        public int[] channels;
        public int[] intensities;
        public int[] durations;
        public bool booster;
    }
}
