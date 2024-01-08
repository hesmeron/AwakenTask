using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DiceController))]
public class DragAndRollInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DiceController diceController = target as DiceController;

        if (GUILayout.Button("Find Dice Faces"))
        {
            Undo.RecordObject(diceController, "Dice faces found");
            diceController.FindDiceFaces();
            EditorUtility.SetDirty(diceController);
        }
    }
}
