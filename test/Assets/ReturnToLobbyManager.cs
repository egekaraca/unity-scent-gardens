using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobbyManager : MonoBehaviour
{
    public GameObject returnPanel;
    public Collider triggerCollider; // ← Eklenen alan: trigger collider’ı kontrol için

    public void GoToLobby()
    {
        SceneManager.LoadScene("Lobi"); // Lobi sahne adı!
    }

    public void Cancel()
    {
        if (returnPanel != null)
        {
            returnPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Yeni eklenen kısım: trigger alanını geçici devre dışı bırak
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
            Invoke(nameof(ReenableTrigger), 1f); // 1 saniye sonra yeniden aktif et
        }
    }

    private void ReenableTrigger()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }
}