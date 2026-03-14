using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EncounterCamera))]
public class EncounterCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EncounterCamera encounterCamera = (EncounterCamera)target;
        Camera cam = encounterCamera.GetComponent<Camera>();
        if (cam == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Reposition to Current OrthoSize"))
        {
            Undo.RecordObject(cam.transform, "Reposition Camera");
            RepositionCamera(cam);
        }

        EditorGUI.BeginChangeCheck();
        float newSize = EditorGUILayout.FloatField("Zoom Level", cam.orthographicSize);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(new Object[] { cam, cam.transform }, "Set Camera Zoom");
            cam.orthographicSize = newSize;
            RepositionCamera(cam);
        }

        if (GUILayout.Button("Reset to Default Zoom"))
        {
            Undo.RecordObjects(new Object[] { cam, cam.transform }, "Reset Camera Zoom");
            cam.orthographicSize = EncounterCamera.DEFAULT_CAMERA_SIZE;
            RepositionCamera(cam);
        }
    }

    private void RepositionCamera(Camera cam)
    {
        float yPos = cam.orthographicSize - EncounterCamera.DEFAULT_CAMERA_SIZE;
        float xPos = yPos * cam.aspect;
        cam.transform.position = new Vector3(xPos, yPos, cam.transform.position.z);
        EditorUtility.SetDirty(cam);
        EditorUtility.SetDirty(cam.transform);
    }
}
