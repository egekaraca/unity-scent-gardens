using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneLoaderXR : MonoBehaviour
{
    public string sceneName; 
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.activated.AddListener(OnTriggerActivated);
    }

     public void OnTriggerActivated(ActivateEventArgs args)
    {
        Debug.Log($"Trigger ile {sceneName} sahnesi yukleniyor...");
        SceneManager.LoadScene(sceneName);
    }
}
