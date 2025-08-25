using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVR : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Start Game clicked. Loading scene: Lobi");
        SceneManager.LoadScene("Lobi"); // Sahne ad�n� tam do�ru yaz (Build Settings�e eklenmi� olmal�)
    }

    public void OpenSettings()
    {
        Debug.Log("Ayarlar men�s� a��l�yor...");
        // �ste�e ba�l� olarak ayarlar paneli a��labilir
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapat�l�yor...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
