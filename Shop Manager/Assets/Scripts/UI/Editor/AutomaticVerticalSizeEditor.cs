using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AutomaticVerticalSize))]
public class AutomaticVerticalSizeEditor : Editor {

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if( GUILayout.Button("Recalculate Size") ){
            ((AutomaticVerticalSize)target).AdjustSize();
        }
    }
}
