using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ReturnPointManager : MonoBehaviour
{
    public static ReturnPointManager Instance;

    // sceneName -> (pos, rot)
    private readonly Dictionary<string, (Vector3 pos, Quaternion rot)> returnPoints
        = new Dictionary<string, (Vector3, Quaternion)>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetReturnPoint(string sceneName, Vector3 pos, Quaternion rot)
    {
        returnPoints[sceneName] = (pos, rot);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Bu sahne için bir return point kaydedilmiþ mi?
        if (!returnPoints.TryGetValue(scene.name, out var data)) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // CharacterController varsa taþýma öncesi devre dýþý býrak
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(data.pos, data.rot);

        if (cc != null) cc.enabled = true;
    }
}
