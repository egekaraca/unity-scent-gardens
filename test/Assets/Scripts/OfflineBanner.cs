using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // <-- UI tipleri için þart

public class OfflineBanner : MonoBehaviour
{
    [Header("UI")]
    public Camera targetCamera;                  // Boþ býrakýlýrsa otomatik MainCamera bulunur
    public float distance = 1.2f;                // Kameranin onunde konum
    public Vector2 panelSize = new Vector2(800, 160);
    public int fontSize = 48;
    public string offlineText = "No Internet Connection.\nPlease check Wi-Fi.";

    [Header("Check Settings")]
    public float checkInterval = 3f;             // saniye
    public bool doHttpProbe = true;              // gercek internet testi

    // Dahili
    Canvas canvas;
    RectTransform panel;
    Text uiText;                                 // TMP yoksa klasik Text kullan
#if TMP_PRESENT
    TMPro.TextMeshProUGUI tmpText;
#endif

    void Awake()
    {
        if (!targetCamera)
        {
            var cam = Camera.main;
            if (cam == null) cam = FindObjectOfType<Camera>();
            targetCamera = cam;
        }

        BuildWorldspaceCanvas();
        SetVisible(false);
    }

    void OnEnable() { StartCoroutine(NetLoop()); }
    void OnDisable() { StopAllCoroutines(); }

    IEnumerator NetLoop()
    {
        while (true)
        {
            bool online = IsReachableQuick();

            if (online && doHttpProbe)
            {
                bool probeOk = false;
                // Coroutine bool donduremez; sonucu callback ile aliyoruz
                yield return StartCoroutine(HttpProbe(result => probeOk = result));
                online = probeOk;
            }

            SetVisible(!online);
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // 1) Hýzlý kontrol
    bool IsReachableQuick()
    {
        var r = Application.internetReachability;
        return r != NetworkReachability.NotReachable;
    }

    // 2) Hafif HTTP probe (generate_204: 204 doner, icerik yok, hizli)
    IEnumerator HttpProbe(System.Action<bool> done)
    {
        using (var req = UnityWebRequest.Get("https://www.gstatic.com/generate_204"))
        {
            req.timeout = 4;
            yield return req.SendWebRequest();

            bool ok = (req.result == UnityWebRequest.Result.Success) || (req.responseCode == 204);
            done?.Invoke(ok);
        }
    }

    void LateUpdate()
    {
        // Canvas'i her frame kameranin onunde tut
        if (!targetCamera || !canvas) return;

        var t = canvas.transform;
        t.position = targetCamera.transform.position + targetCamera.transform.forward * distance;
        t.rotation = Quaternion.LookRotation(
            t.position - targetCamera.transform.position,
            targetCamera.transform.up
        );
    }

    void SetVisible(bool v)
    {
        if (!panel) return;
        panel.gameObject.SetActive(v);
    }

    void BuildWorldspaceCanvas()
    {
        // Canvas
        var go = new GameObject("OfflineCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = targetCamera;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 2f;
        go.layer = LayerMask.NameToLayer("UI");

        // Panel
        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel = panelGO.GetComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);
        panel.sizeDelta = panelSize;

        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.72f); // yarý saydam siyah arkaplan

        // Text (TMP varsa onu kullan, yoksa klasik Text)
#if TMP_PRESENT
        var textGO = new GameObject("TextTMP", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        var rt = textGO.GetComponent<RectTransform>();
        rt.SetParent(panel, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(24, 16);
        rt.offsetMax = new Vector2(-24, -16);

        tmpText = textGO.GetComponent<TMPro.TextMeshProUGUI>();
        tmpText.text = offlineText;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        tmpText.enableWordWrapping = true;
        tmpText.color = new Color(1f, 0.3f, 0.3f, 1f);
#else
        var textGO = new GameObject("TextUI", typeof(RectTransform), typeof(Text));
        var rt = textGO.GetComponent<RectTransform>();
        rt.SetParent(panel, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(24, 16);
        rt.offsetMax = new Vector2(-24, -16);

        uiText = textGO.GetComponent<Text>();
        uiText.text = offlineText;
        uiText.fontSize = fontSize;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.supportRichText = true;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        uiText.color = new Color(1f, 0.3f, 0.3f, 1f);
#endif
    }
}
