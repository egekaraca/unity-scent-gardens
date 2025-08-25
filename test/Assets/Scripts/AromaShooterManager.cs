using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class AromaShooterManager : MonoBehaviour
{
    public enum LogLevel { Off, ErrorsOnly, Verbose }

    [Header("Debug")]
    public LogLevel debugLogs = LogLevel.Verbose;
    public bool autoTriggerOnProximity = true;

    [Header("Detection")]
    public Transform player;
    public Transform scentSource;
    public float triggerDistance = 2f;
    [Min(0.1f)] public float cooldownSeconds = 5f;

    [Header("Host Source")]
    public bool useHostFromConfig = true;
    [Tooltip("Manual or resolved host: e.g. ASN2A01058.local or 192.168.1.50")]
    public string aromaShooterHost = "ASN2A01058.local";
    public int aromaShooterPort = 1003;

    [Header("Auto-Discover (if .local fails)")]
    [Tooltip("If true, when .local cannot be resolved on Android, scan LAN for port 1003 and use the found IP.")]
    public bool autoDiscoverOnLocalFail = true;
    [Tooltip("TCP connect timeout per IP during scan (ms).")]
    public int discoverTimeoutMs = 250;
    [Tooltip("Give up discovery after N seconds.")]
    public float discoverBudgetSeconds = 12f;

    [Header("Scent Settings")]
    [Range(1, 6)] public int channel = 1;
    [Range(0, 100)] public int intensity = 100;
    [Min(1)] public int durationMs = 2000;
    public bool booster = true;

    // ---- internal ----
    bool _coolingDown, _isSending, _isDiscovering;
    WaitForSeconds _cooldownWait;

    // PlayerPrefs keys (ConnectManager ile ortak)
    const string PREF_HOST = "aroma_host";
    const string PREF_PORT = "aroma_port";

    void Reset() => scentSource = transform;

    void Awake()
    {
        if (scentSource == null) scentSource = transform;
        if (cooldownSeconds < 0.1f) cooldownSeconds = 0.1f;
        _cooldownWait = new WaitForSeconds(cooldownSeconds);
        V($"[Aroma] Awake | host='{aromaShooterHost}' port={aromaShooterPort}");
    }

    void Start()
    {
        // 1) Kullanýcýnýn daha önce girdiði IP varsa, onu öne al
        if (TryLoadHost(out var savedIp, out var savedPort) && IsIPv4(savedIp))
        {
            aromaShooterHost = savedIp.Trim();
            aromaShooterPort = savedPort;
            V($"[Aroma] Loaded saved IP {aromaShooterHost}:{aromaShooterPort}");
        }
        // 2) Yoksa, config’ten çek (seri numarasý vs.)
        else if (useHostFromConfig && SerialNumberManager.Instance != null)
        {
            var host = SerialNumberManager.Instance.GetDeviceIP(); // .local gelebilir
            if (!string.IsNullOrWhiteSpace(host)) aromaShooterHost = host.Trim();
        }

        if (player == null) W("[Aroma] 'player' not set");
        if (scentSource == null) W("[Aroma] 'scentSource' not set (using self)");
    }

    float _lastDistLog;
    void Update()
    {
        if (!autoTriggerOnProximity || player == null || scentSource == null) return;

        float dist = Vector3.Distance(player.position, scentSource.position);
        if (debugLogs == LogLevel.Verbose && Time.time - _lastDistLog > 0.5f)
        {
            V($"[Aroma] Dist={dist:F2} (<{triggerDistance}) | cooling={_coolingDown} | sending={_isSending} | host='{aromaShooterHost}'");
            _lastDistLog = Time.time;
        }

        if (!_coolingDown && !_isSending && dist < triggerDistance)
        {
            StartCoroutine(SendDiffuseRequestOrDiscover());
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown()
    {
        _coolingDown = true;
        yield return _cooldownWait;
        _coolingDown = false;
    }

    [ContextMenu("Force Diffuse Once")]
    public void ForceDiffuseOnce()
    {
        if (_isSending) return;
        StartCoroutine(SendDiffuseRequestOrDiscover());
    }

    IEnumerator SendDiffuseRequestOrDiscover()
    {
        // Android’de .local ile asla istek atma; önce IP bulmaya çalýþ
#if UNITY_ANDROID && !UNITY_EDITOR
        if (IsLocalHost(aromaShooterHost))
        {
            V("[Aroma] Host is .local on Android; attempting discovery...");
            if (autoDiscoverOnLocalFail && !_isDiscovering)
            {
                yield return StartCoroutine(AutoDiscoverAndAssignHost());
            }
            // Keþiften sonra hâlâ .local ise istek atmayý iptal et
            if (IsLocalHost(aromaShooterHost))
            {
                E("[Aroma] Still .local; aborting request. Please set IP (manual or discovery).");
                yield break;
            }
        }
#endif
        yield return StartCoroutine(SendDiffuseRequest());
    }

    IEnumerator SendDiffuseRequest()
    {
        var host = SanitizeHost(aromaShooterHost);
        if (string.IsNullOrEmpty(host))
        {
            E("[Aroma] Host is empty. Set IP or enable discovery.");
            yield break;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsIPv4(host))
        {
            E($"[Aroma] Non-IPv4 host on Android ('{host}'). Aborting.");
            yield break;
        }
#endif

        string url = $"http://{host}:{aromaShooterPort}/as2/diffuse";

        var payload = new DiffusePayload
        {
            channels = new[] { Mathf.Clamp(channel, 1, 6) },
            intensities = new[] { Mathf.Clamp(intensity, 0, 100) },
            durations = new[] { Mathf.Max(1, durationMs) },
            booster = booster
        };
        string json = JsonUtility.ToJson(payload);

        _isSending = true;
        V($"[Aroma] POST {url} body={json}");

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 8;

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool ok = (req.result == UnityWebRequest.Result.Success);
#else
            bool ok = !(req.isNetworkError || req.isHttpError);
#endif
            int code = (int)req.responseCode;
            string resp = req.downloadHandler != null ? req.downloadHandler.text : "";

            if (!ok)
            {
                // Tipik: UnknownHostException -> hostname çözülemedi
                E($"[Aroma] Diffuse ERROR: {req.error} (HTTP {code}) resp='{resp}' | host='{host}'");
            }
            else
            {
                if (code >= 200 && code < 300)
                {
                    V($"[Aroma] Diffuse OK ({code}) resp='{resp}'");
                    // Baþarýlý olduysa ve IPv4 ise kalýcý kaydet (diðer sahneler kullansýn)
                    if (IsIPv4(host)) SaveHost(host, aromaShooterPort);
                }
                else
                {
                    W($"[Aroma] Diffuse non-2xx ({code}) resp='{resp}'");
                }
            }
        }
        _isSending = false;
    }

    // ----------- Auto-discover: .local çözülmezse port 1003 tarar -----------
    IEnumerator AutoDiscoverAndAssignHost()
    {
        _isDiscovering = true;
        string subnet = GetLocalSubnetPrefix(); // "192.168.1."
        if (string.IsNullOrEmpty(subnet))
        {
            W("[Aroma] Cannot get local subnet; skipping discovery.");
            _isDiscovering = false;
            yield break;
        }

        V($"[Aroma] Auto-discover on subnet {subnet}* (port {aromaShooterPort}) ...");
        float startT = Time.time;
        const int startIP = 1, endIP = 254;

        for (int i = startIP; i <= endIP; i++)
        {
            if (Time.time - startT > discoverBudgetSeconds)
            {
                W("[Aroma] Discovery time budget exceeded.");
                break;
            }

            string ip = subnet + i;
            bool open = false;

            try
            {
                using (var client = new TcpClient())
                {
                    var ar = client.BeginConnect(ip, aromaShooterPort, null, null);
                    bool done = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Mathf.Clamp(discoverTimeoutMs, 80, 1000)));
                    if (done)
                    {
                        client.EndConnect(ar);
                        open = true;
                    }
                }
            }
            catch { open = false; }

            if (open)
            {
                aromaShooterHost = ip;                 // IP'yi sabitle
                SaveHost(aromaShooterHost, aromaShooterPort); // kalýcý kaydet
                V($"[Aroma] Device candidate: {ip}:{aromaShooterPort} (port open)");
                break;
            }

            if ((i % 2) == 0) yield return null; // VR’da stutter olmasýn
        }

        if (IsIPv4(aromaShooterHost))
            V($"[Aroma] Discovery success. Using {aromaShooterHost}:{aromaShooterPort}");
        else
            W("[Aroma] Discovery failed. Could not find device on LAN.");

        _isDiscovering = false;
    }

    // ---- Helpers ----
    static bool IsLocalHost(string host) =>
        !string.IsNullOrEmpty(host) && host.Trim().ToLowerInvariant().EndsWith(".local");

    static bool IsIPv4(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split('.');
        if (parts.Length != 4) return false;
        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out int v) || v < 0 || v > 255) return false;
        }
        return true;
    }

    static string SanitizeHost(string host)
    {
        if (string.IsNullOrEmpty(host)) return host;
        host = host.Trim();
        // görünmez unicode boþluklarýný temizle
        host = host.Replace("\u200B", "").Replace("\u200E", "").Replace("\u200F", "");
        return host;
    }

    static string GetLocalSubnetPrefix()
    {
        try
        {
            string ip = GetLocalIPAddress();
            if (IsIPv4(ip))
            {
                int lastDot = ip.LastIndexOf('.');
                if (lastDot > 0) return ip.Substring(0, lastDot + 1); // "192.168.1."
            }
        }
        catch { }
        return null;
    }

    static string GetLocalIPAddress()
    {
        // 1) Standart DNS yolu
        try
        {
            var host = System.Net.Dns.GetHostName();
            var addrs = System.Net.Dns.GetHostAddresses(host);
            foreach (var a in addrs)
                if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return a.ToString();
        }
        catch { }

#if UNITY_ANDROID && !UNITY_EDITOR
        // 2) Android WifiManager’dan çek
        try
        {
            using (var jp = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var act = jp.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var wifi = act.Call<AndroidJavaObject>("getSystemService", "wifi"))
            using (var info = wifi.Call<AndroidJavaObject>("getConnectionInfo"))
            {
                int ipInt = info.Call<int>("getIpAddress"); // little-endian
                byte[] b = BitConverter.GetBytes(ipInt);
                string ip = $"{b[0] & 0xFF}.{b[1] & 0xFF}.{b[2] & 0xFF}.{b[3] & 0xFF}";
                return ip;
            }
        }
        catch { }
#endif
        return null;
    }

    static void SaveHost(string ip, int port)
    {
        if (string.IsNullOrWhiteSpace(ip)) return;
        PlayerPrefs.SetString(PREF_HOST, ip.Trim());
        PlayerPrefs.SetInt(PREF_PORT, port);
        PlayerPrefs.Save();
        Debug.Log($"[Aroma] Saved host to prefs: {ip}:{port}");
    }

    static bool TryLoadHost(out string ip, out int port)
    {
        ip = PlayerPrefs.GetString(PREF_HOST, "");
        port = PlayerPrefs.GetInt(PREF_PORT, 1003);
        return !string.IsNullOrWhiteSpace(ip);
    }

    [Serializable]
    private class DiffusePayload
    {
        public int[] channels;
        public int[] intensities;
        public int[] durations;
        public bool booster;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var t = scentSource != null ? scentSource : transform;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawSphere(t.position, triggerDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(t.position, triggerDistance);
    }
#endif

    // logs
    void V(string m) { if (debugLogs == LogLevel.Verbose) Debug.Log(m); }
    void W(string m) { if (debugLogs != LogLevel.Off) Debug.LogWarning(m); }
    void E(string m) { Debug.LogError(m); }

    // (opsiyonel) Hýzlý temizlik için:
    [ContextMenu("Clear Saved Host (PlayerPrefs)")]
    void ClearSavedHost()
    {
        PlayerPrefs.DeleteKey(PREF_HOST);
        PlayerPrefs.DeleteKey(PREF_PORT);
        PlayerPrefs.Save();
        W("[Aroma] Cleared saved host from PlayerPrefs.");
    }
}
