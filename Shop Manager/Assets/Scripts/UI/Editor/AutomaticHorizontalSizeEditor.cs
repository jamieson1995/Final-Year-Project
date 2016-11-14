using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AutomaticHorizontalSize))]
public class AutomaticHorizontalSizeEditor : Editor {

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if( GUILayout.Button("Recalculate Size") ){
			((AutomaticHorizontalSize)target).AdjustSize();
        }
    }
}
