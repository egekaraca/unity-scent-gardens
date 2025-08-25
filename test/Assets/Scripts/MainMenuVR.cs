using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVR : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Start Game clicked. Loading scene: Lobi");
        SceneManager.LoadScene("Lobi"); // Sahne adýný tam doðru yaz (Build Settings’e eklenmiþ olmalý)
    }

    public void OpenSettings()
    {
        Debug.Log("Ayarlar menüsü açýlýyor...");
        // Ýsteðe baðlý olarak ayarlar paneli açýlabilir
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapatýlýyor...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
