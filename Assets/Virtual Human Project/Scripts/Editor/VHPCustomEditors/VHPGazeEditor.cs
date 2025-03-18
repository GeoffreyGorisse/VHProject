/********************************************************************
Filename    :   VHPGazeEditor.cs
Created     :   July 16th, 2020
Copyright   :   Geoffrey Gorisse.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see<https://www.gnu.org/licenses/>.
********************************************************************/
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VHPGaze)), CanEditMultipleObjects]
public class VHPGazeEditor : Editor
{
    private VHPGaze _myVHTGaze;
    private SerializedProperty _agentMode;
    private SerializedProperty _interestFieldPrefab;

    private void OnEnable()
    {
        _interestFieldPrefab = serializedObject.FindProperty("_interestFieldPrefab");
        _agentMode = serializedObject.FindProperty("_agentMode");
    }

    public override void OnInspectorGUI()
    {
        _myVHTGaze = (VHPGaze)target;

        serializedObject.Update();

        base.OnInspectorGUI();

        if (_myVHTGaze.GazeBehaviorMode == VHPGaze.GazeBehavior.PROBABILISTIC) 
        {
            // Displays an object field for the gaze interest field prefab.
            string interestFieldPrefabTooltip = "Add the Default Gaze Interest Field Prefab(Assets/Virtual Human Project/Prefabs/).";
            EditorGUILayout.PropertyField(_interestFieldPrefab, new GUIContent("Interest Field Prefab", interestFieldPrefabTooltip));
            //myVHTGaze.interestFieldPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Interest Field Prefab", interestFieldPrefabTooltip), myVHTGaze.interestFieldPrefab, typeof(GameObject), false)

            // Enables agent mode activation to allow IK-based upper body rotations when looking at a target.
            string agentModeTooltip = "Enable the agent mode to override upper body rotations when looking at a target (IK pass must be enabled in the animator settings).";
            EditorGUILayout.PropertyField(_agentMode, new GUIContent("Agent Mode", agentModeTooltip));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
