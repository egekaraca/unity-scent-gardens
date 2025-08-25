using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 Angle = new Vector3(0, 10, 0); // Sadece yatay dönme

    void Update()
    {
        transform.Rotate(Angle * Time.deltaTime);
    }
}
