/********************************************************************
Filename    :   VHPManagerEditor.cs
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

[CustomEditor(typeof(VHPManager))]
public class VHPManagerEditor : Editor
{
    private VHPManager _myVHPManager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _myVHPManager = (VHPManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Add procedural controllers:", EditorStyles.boldLabel);

        DrawButton("Emotions", typeof(VHPEmotions));
        DrawButton("Gaze", typeof(VHPGaze));
        DrawButton("Lip Sync", typeof(VHPLipSync));
    }

    private void DrawButton(string buttonName, System.Type scriptTypeToAdd)
    {
        if (GUILayout.Button(buttonName))

            if (!_myVHPManager.gameObject.GetComponent(scriptTypeToAdd))
                _myVHPManager.gameObject.AddComponent(scriptTypeToAdd);

        else
            Debug.Log(scriptTypeToAdd.ToString() + " already added to the character.");
    }
}
