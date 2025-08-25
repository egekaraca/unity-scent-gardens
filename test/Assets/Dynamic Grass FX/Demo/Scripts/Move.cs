using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 0.8f;      // Yürüme hızı (m/sn)
    public float turnSpeed = 90f;       // Dönüş hızı (derece/sn)
    public float walkTimeMin = 2f;      // Minimum yürüme süresi
    public float walkTimeMax = 5f;      // Maksimum yürüme süresi
    public float idleTimeMin = 2f;      // Minimum durma süresi
    public float idleTimeMax = 4f;      // Maksimum durma süresi

    private bool isWalking = false;
    private float stateTimer;
    private float fixedY;

    void Start()
    {
        fixedY = transform.position.y;
        ChangeState();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        if (isWalking)
        {
            // İleri hareket
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            // Hafif rastgele dönüş
            float randomTurn = Random.Range(-1f, 1f); // -1 sola, 1 sağa
            transform.Rotate(Vector3.up * randomTurn * turnSpeed * Time.deltaTime);
        }

        // Süre bitince yeni duruma geç
        if (stateTimer <= 0)
        {
            ChangeState();
        }

        // Y konumunu sabitle (havada gitmesin)
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    void ChangeState()
    {
        if (isWalking)
        {
            // Yürürken durma moduna geç
            isWalking = false;
            stateTimer = Random.Range(idleTimeMin, idleTimeMax);
        }
        else
        {
            // Dururken yürümeye başla
            isWalking = true;
            stateTimer = Random.Range(walkTimeMin, walkTimeMax);

            // Rastgele yeni yön belirle
            float newYRotation = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0, newYRotation, 0);
        }
    }
}
