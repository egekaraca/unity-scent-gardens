using UnityEngine;
using UnityEditor;

public class FenceArranger : EditorWindow
{
    GameObject parentObject;
    float radius = 5f;
    float angleStart = 0f;
    float angleEnd = 180f;

    [MenuItem("Tools/Circular Fence Arranger")]
    static void Init()
    {
        FenceArranger window = (FenceArranger)EditorWindow.GetWindow(typeof(FenceArranger));
        window.Show();
    }

    void OnGUI()
    {
        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
        radius = EditorGUILayout.FloatField("Radius", radius);
        angleStart = EditorGUILayout.FloatField("Start Angle", angleStart);
        angleEnd = EditorGUILayout.FloatField("End Angle", angleEnd);

        if (GUILayout.Button("Arrange Circular"))
        {
            ArrangeCircular();
        }
    }

    void ArrangeCircular()
    {
        if (parentObject == null) return;

        int count = parentObject.transform.childCount;
        if (count == 0) return;

        float angleStep = (angleEnd - angleStart) / (count - 1);

        for (int i = 0; i < count; i++)
        {
            Transform child = parentObject.transform.GetChild(i);
            float angle = angleStart + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
            child.localPosition = pos;
            child.LookAt(parentObject.transform.position);
        }
    }
}
