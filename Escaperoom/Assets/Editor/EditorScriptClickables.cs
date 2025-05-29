using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Clickable))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Cast the target object to your script type
        Clickable clickable = (Clickable)target;

        // Always draw the enum dropdown
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onClickList"));

        // Conditionally draw fields based on the enum's current value
        switch (clickable.onClickList)
        {
            case Clickable.OnClickList.ActivateGO:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activatedGO"));
                break;

        }

        // Draw any common fields
        // EditorGUILayout.PropertyField(serializedObject.FindProperty("commonField"));

        // Apply any changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}