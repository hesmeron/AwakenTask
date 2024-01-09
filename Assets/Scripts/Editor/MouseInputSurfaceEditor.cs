using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MouseInputSurface))]
public class MouseInputSurfaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MouseInputSurface mouseInputSurface = target as MouseInputSurface;

        if (GUILayout.Button("Build mesh"))
        {
            Undo.RecordObject(mouseInputSurface, "Build mesh");
            mouseInputSurface.MakeAreaCollider();
            EditorUtility.SetDirty(mouseInputSurface);
        }
    }
}
