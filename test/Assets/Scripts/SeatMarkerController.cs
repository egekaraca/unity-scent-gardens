using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering.Universal; // DecalProjector için (URP)

public class SeatMarkerController : MonoBehaviour
{
    [Header("Visual Root (recommended)")]
    [Tooltip("Only visuals under this GameObject will be hidden/shown. Leave empty to use children of this object.")]
    public GameObject visualRoot;

    [Header("Emission / Anim (optional)")]
    [ColorUsage(true, true)] public Color baseEmissionColor = new Color(0f, 1f, 1f, 1f);
    [Range(0f, 5f)] public float emissionIntensity = 1.25f;
    [Range(0f, 5f)] public float hoverEmissionBoost = 1.0f;
    public bool scalePulse = true;
    public float pulseSpeed = 2f;
    [Range(0.1f, 2f)] public float pulseMin = 0.95f;
    [Range(0.1f, 2f)] public float pulseMax = 1.05f;
    public bool rotate = true;
    public float rotateSpeed = 30f;

    private Renderer[] _renderers;
    private DecalProjector[] _decals;
    private ParticleSystem[] _particles;
    private MaterialPropertyBlock _mpb;
    private Transform _pulseTarget;
    private Vector3 _baseScale;
    private bool _highlight;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Reset()
    {
        // öneri: visualRoot olarak bir "RingVisualRoot" alt objesi oluþtur ve onu ata
        if (!visualRoot && transform.childCount > 0)
            visualRoot = transform.GetChild(0).gameObject;
    }

    void Awake()
    {
        if (!visualRoot) visualRoot = gameObject; // fallback

        // Görsel elemanlarý topla
        _renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        _decals = visualRoot.GetComponentsInChildren<DecalProjector>(true);
        _particles = visualRoot.GetComponentsInChildren<ParticleSystem>(true);

        // Pulse/dönüþ hangi objede olacak?
        _pulseTarget = visualRoot.transform;
        _baseScale = _pulseTarget.localScale;

        _mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        // Sadece anim tarafý; görünürlük SetVisible ile kontrol ediliyor
        if (scalePulse && visualRoot.activeInHierarchy)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f);
            float pulse = Mathf.Lerp(pulseMin, pulseMax, t);
            if (_highlight) pulse *= 1f + hoverEmissionBoost * 0.25f;
            _pulseTarget.localScale = _baseScale * pulse;
        }

        if (rotate && visualRoot.activeInHierarchy)
            _pulseTarget.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // Emission’ý MPB ile it
        Color emission = baseEmissionColor * (emissionIntensity * (_highlight ? (1f + hoverEmissionBoost) : 1f));
        foreach (var r in _renderers)
        {
            if (!r) continue;
            _mpb.Clear();
            if (r.sharedMaterial && r.sharedMaterial.HasProperty(EmissionColorID))
            {
                _mpb.SetColor(EmissionColorID, emission);
            }
            if (r.sharedMaterial && r.sharedMaterial.HasProperty(BaseColorID))
            {
                _mpb.SetColor(BaseColorID, baseEmissionColor);
            }
            r.SetPropertyBlock(_mpb);
        }
    }

    // XR Interactable Events -> baðla
    public void OnHoverEnter(HoverEnterEventArgs _)
    {
        _highlight = true;
    }

    public void OnHoverExit(HoverExitEventArgs _)
    {
        _highlight = false;
    }

    public void OnSelectEntered(SelectEnterEventArgs _)
    {
        // Kritik nokta: burada gerçekten çaðrýlýyor mu? Log kontrolü
        Debug.Log($"[SeatMarkerController] SelectEntered on {name} -> hide visuals");
        SetVisible(false);
    }

    public void OnSelectExited(SelectExitEventArgs _)
    {
        SetVisible(true);
    }

    private void SetVisible(bool v)
    {
        // GameObject’in tamamýný kapatmýyoruz; yoksa event alamaz.
        // Sadece ALT görselleri kapatýyoruz:
        if (visualRoot)
            visualRoot.SetActive(v);

        // Yine de güvenlik için renderer/decals/particles bazýnda da yönetelim:
        if (_renderers != null) foreach (var r in _renderers) if (r) r.enabled = v;
        if (_decals != null) foreach (var d in _decals) if (d) d.enabled = v;

        if (_particles != null)
        {
            foreach (var p in _particles)
            {
                if (!p) continue;
                if (v) { if (!p.isPlaying) p.Play(); }
                else { if (p.isPlaying) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
            }
        }
    }
}
