using UnityEngine;

public class RuzgarSalinimi : MonoBehaviour
{
    [Header("Salinim Parametreleri")]
    public float hiz = 1.2f;
    public float guc = 2f;
    public float rastgelelik = 1f;

    [Header("Varyasyon Ayari")]
    public float varyasyonHizi = 0.5f;
    public float varyasyonKuvveti = 0.7f;

    [Header("WindZone (istege bagli)")]
    public WindZone windZone;
    private float ruzgarKatsayisi = 1f;

    private Vector3 baslangicRotasyon;
    private float fazX, fazZ;
    private float noiseSeedX, noiseSeedZ;

    void Start()
    {
        baslangicRotasyon = transform.localEulerAngles;

        // Her objeye farkl� ba�lang�� de�eri
        fazX = Random.Range(0f, Mathf.PI * 2f);
        fazZ = Random.Range(0f, Mathf.PI * 2f);
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedZ = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float time = Time.time * hiz;

        if (windZone != null)
        {
            
            ruzgarKatsayisi = Mathf.Lerp(ruzgarKatsayisi, windZone.windMain, Time.deltaTime * 1.5f);
        }

        // Temel sal�n�m (sin dalgas� + varyasyon)
        float perlinX = Mathf.PerlinNoise(noiseSeedX, Time.time * varyasyonHizi) * varyasyonKuvveti;
        float perlinZ = Mathf.PerlinNoise(noiseSeedZ, Time.time * varyasyonHizi) * varyasyonKuvveti;

        float aciX = Mathf.Sin(time + fazX + transform.position.x * rastgelelik) * guc * 0.5f + perlinX;
        float aciZ = Mathf.Sin(time + fazZ + transform.position.z * rastgelelik) * guc + perlinZ;

        transform.localEulerAngles = baslangicRotasyon + new Vector3(aciX, 0f, aciZ) * ruzgarKatsayisi;
    }
}
