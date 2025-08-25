using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerUI : MonoBehaviour
{
    [Header("Return Point (optional)")]
    [Tooltip("Butona basıldığında oyuncunun o anki konumu yerine bu sabit noktayı kaydetmek istersen doldur.")]
    public Transform fixedReturnPoint;
    public bool usePlayerCurrentTransform = true; // true: o anki konum, false: fixedReturnPoint

    public void GoToScene(string sceneName)
    {
        // 1) Mevcut sahne adını al
        string currentScene = SceneManager.GetActiveScene().name;

        // 2) Geri dönüş noktası olarak ya oyuncunun o anki konumunu ya da sabit noktayı kaydet
        if (ReturnPointManager.Instance != null)
        {
            Transform t = null;

            if (usePlayerCurrentTransform)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) t = player.transform;
            }
            else
            {
                t = fixedReturnPoint; // Inspector'dan atamalısın
            }

            if (t != null)
            {
                ReturnPointManager.Instance.SetReturnPoint(currentScene, t.position, t.rotation);
            }
        }

        // 3) Hedef sahneyi yükle
        Debug.Log($"Sahne seçildi. Yükleniyor: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void GoToSceneA() => GoToScene("papatya_bahcesi_new");
    public void GoToSceneB() => GoToScene("Gul_bahcesi");
    public void GoToSceneC() => GoToScene("Lavanta_bahcesi_new");
    public void GoToSceneD() => GoToScene("Sakura_Bahcesi");
    public void GoToSceneE() => GoToScene("Zambak_bahcesi");
}
