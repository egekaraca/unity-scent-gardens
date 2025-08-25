using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverColorChange : MonoBehaviour
{
    [SerializeField] private Material normalMat;
    [SerializeField] private Material hoverMat;

    private Renderer rend;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Start()
    {
       
        rend = GetComponentInChildren<Renderer>();

      
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEnter);
            interactable.hoverExited.AddListener(OnHoverExit);
        }
        else
        {
            Debug.LogError("XRBaseInteractable bulunamadi! Objeye XR Simple Interactable ekle.");
        }
    }

     void OnHoverEnter(HoverEnterEventArgs args)
    {
        rend.material = hoverMat;
    }

     void OnHoverExit(HoverExitEventArgs args)
    {
        rend.material = normalMat;
    }
}
