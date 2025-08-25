using UnityEngine;

public class ReturnToLobbyZone : MonoBehaviour
{
    public GameObject returnPanel;

    private void Start()
    {
        if (returnPanel != null)
        {
            returnPanel.SetActive(false); // Menü sahne başında gizli olsun
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && returnPanel != null)
        {
            returnPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && returnPanel != null)
        {
            returnPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}