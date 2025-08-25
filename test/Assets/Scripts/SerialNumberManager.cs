using UnityEngine;

public class SerialNumberManager : MonoBehaviour
{
    public static SerialNumberManager Instance { get; private set; }

    const string KEY_SERIAL = "SNM_SERIAL";
    const string KEY_HOST = "SNM_HOST";

    [SerializeField] private string currentSerial; // �rn: ASN2A01058
    [SerializeField] private string currentHost;   // �rn: ASN2A01058.local veya 192.168.1.50

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Persist edilen de�erleri y�kle
        currentSerial = PlayerPrefs.GetString(KEY_SERIAL, currentSerial ?? "");
        currentHost = PlayerPrefs.GetString(KEY_HOST, currentHost ?? "");
    }

    // ����� API �����

    /// Seri numaras�n� ayarla (�rn: ASN2A01058). Bo� de�ilse host�u <seri>.local yapar.
    public void SetSerial(string serial, bool autoComputeHost = true)
    {
        currentSerial = (serial ?? "").Trim();
        PlayerPrefs.SetString(KEY_SERIAL, currentSerial);

        if (autoComputeHost && !string.IsNullOrEmpty(currentSerial))
        {
            var host = currentSerial.EndsWith(".local") ? currentSerial : currentSerial + ".local";
            SetDeviceIP(host);
        }
    }

    public string GetSerial() => currentSerial;

    /// Host/IP�i do�rudan elle ayarla (�rn: 192.168.1.50 veya ASN2A01058.local)
    public void SetDeviceIP(string host)
    {
        currentHost = (host ?? "").Trim();
        PlayerPrefs.SetString(KEY_HOST, currentHost);
    }

    /// Host/IP�i al (AromaShooter burada bunu kullan�r)
    public string GetDeviceIP() => currentHost;

    /// Yard�mc�: Mevcut serial�dan .local host �retir.
    public string ComputeLocalHostFromSerial()
    {
        if (string.IsNullOrEmpty(currentSerial)) return "";
        return currentSerial.EndsWith(".local") ? currentSerial : currentSerial + ".local";
    }
}
