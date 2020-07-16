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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VHPGaze)), CanEditMultipleObjects]
public class VHPGazeEditor : Editor
{
    private VHPGaze myVHTGaze;

    private SerializedProperty m_agentMode;
    private SerializedProperty m_interestFieldPrefab;

    private void OnEnable()
    {
        m_interestFieldPrefab = serializedObject.FindProperty("interestFieldPrefab");
        m_agentMode = serializedObject.FindProperty("agentMode");
    }

    public override void OnInspectorGUI()
    {
        myVHTGaze = (VHPGaze)target;

        serializedObject.Update();

        base.OnInspectorGUI();

        if (myVHTGaze.gazeBehavior == VHPGaze.GazeBehavior.PROBABILISTIC)
        {
            // Displaying an object field to add the interest field prefab if the gaze behavior of the character is set to probabilistic.
            string interestFieldPrefabTooltip = "Add the Gaze Interest Field Prefab(Assets/Virtual Human Toolkit/Prefabs/).";
            EditorGUILayout.PropertyField(m_interestFieldPrefab, new GUIContent("Interest Field Prefab", interestFieldPrefabTooltip));
            //myVHTGaze.interestFieldPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Interest Field Prefab", interestFieldPrefabTooltip), myVHTGaze.interestFieldPrefab, typeof(GameObject), false)

            string agentModeTooltip = "Enable the agent mode to override upper body rotations when looking at a target (IK pass must be enabled in the animator settings).";
            EditorGUILayout.PropertyField(m_agentMode, new GUIContent("Agent Mode", agentModeTooltip));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
