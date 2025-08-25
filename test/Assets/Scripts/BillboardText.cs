using UnityEngine;

public class BillboardText : MonoBehaviour
{
    public Camera targetCamera; 

    void LateUpdate()
    {
        
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null) return;

        
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
    }
}
