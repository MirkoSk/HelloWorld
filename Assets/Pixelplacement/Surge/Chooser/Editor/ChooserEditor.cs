/// <summary>
/// SURGE FRAMEWORK
/// Author: Bob Berkebile
/// Email: bobb@pixelplacement.com
/// 
/// Custom inspector Chooser.
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pixelplacement
{
	[CustomEditor(typeof(Chooser))]
	[CanEditMultipleObjects]
	public class ChooserEditor : Editor
	{
		#region Private Variables
		Chooser _target;
		#endregion

		#region Init
		void OnEnable()
		{
			_target = target as Chooser;
		}
		#endregion

		#region Inspector GUI
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("source"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("raycastDistance"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("layermask"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("method"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pressedInput"), true);

			_target._cursorPropertiesFolded = EditorGUILayout.Foldout(_target._cursorPropertiesFolded, "Cursor Properties", true);
			if (_target._cursorPropertiesFolded)
			{
				EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cursor"));
				GUI.enabled = _target.cursor != null;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceOffset"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("idleDistance"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilityMaxDelta"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("snapToMaxDelta"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("stableSpeed"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("unstableSpeed"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("flipForward"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("matchSurfaceNormal"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("autoHide"));
				EditorGUI.indentLevel = 0;
				GUI.enabled = true;
				EditorGUILayout.Space();
			}

			_target._unityEventsFolded = EditorGUILayout.Foldout(_target._unityEventsFolded, "Unity Events", true);
			if (_target._unityEventsFolded)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSelected"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPressed"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnReleased"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeselected"));
				EditorGUILayout.Space();
			}

			serializedObject.ApplyModifiedProperties();
		}
		#endregion
	}
}