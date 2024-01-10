using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoundaryController))]
public class BoundaryControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BoundaryController boundaryController = target as BoundaryController;

        if (GUILayout.Button("Build mesh"))
        {
            Undo.RecordObject(boundaryController, "Build mesh");
            boundaryController.MakeAreaCollider();
            EditorUtility.SetDirty(boundaryController);
        }
    }
}
