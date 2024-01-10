using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoundaryController))]
public class BoundaryControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BoundaryController mouseInputSurface = target as BoundaryController;

        if (GUILayout.Button("Build mesh"))
        {
            Undo.RecordObject(mouseInputSurface, "Build mesh");
            mouseInputSurface.MakeAreaCollider();
            EditorUtility.SetDirty(mouseInputSurface);
        }
    }
}
