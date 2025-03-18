/********************************************************************
Filename    :   VHPBlendShapesMapperEditor.cs
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class VHPBlendShapesMapperEditor : EditorWindow
{
    public BlendShapesMapper BlendShapesMapper;
    public BlendShapesMapper.FacialExpression FacialPoseToEdit = BlendShapesMapper.FacialExpression.DEFAULT;
    public GameObject Character;
    public List<SkinnedMeshRenderer> SkinnedMeshRenderersWithBlendShapes = new List<SkinnedMeshRenderer>();

    private bool _eraseDataSafetyEnabled = false;
    private bool _unsavedDataSafetyEnabled = false;

    [MenuItem("Window/Virtual Human Project/Blend Shapes Mapper Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<VHPBlendShapesMapperEditor>("Blend Shapes Mapper Editor");
    }

    private void OnGUI()
    {
        #region Blendshapes mapper settings

        GUILayout.Label("Blend shapes mapper settings", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Object field to add the blend shapes mapper to be edited.
        string blendShapesMapperTooltip = "Blend shapes preset to edit. Use right click -> create -> Virtual Human Project -> BlendShapesMapper in the project window to create a new mapper preset.";
        BlendShapesMapper = (BlendShapesMapper)EditorGUILayout.ObjectField(new GUIContent("Mapper preset", blendShapesMapperTooltip), BlendShapesMapper, typeof(BlendShapesMapper), true);

        GUI.enabled = BlendShapesMapper;
  
        string facialPoseToEditTooltip = "Facial expression preset to edit.";
        // Enumeration to choose the facial expression preset to be edited.
        FacialPoseToEdit = (BlendShapesMapper.FacialExpression)EditorGUILayout.EnumPopup(new GUIContent("Facial Expression", facialPoseToEditTooltip), FacialPoseToEdit);

        GUI.enabled = true;

        #endregion

        #region Character settings

        GUILayout.Space(10);
        GUILayout.Label("Character settings", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Object field to assign a character GameObject and retrieve its skinned mesh renderers with blend shapes.
        string characterTooltip = "Character's root GameObject to retrieve the children skinned mesh renderers' blend shapes.";
        Character = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Character's root", characterTooltip), Character, typeof(GameObject), true);

        if(Character)
        {
            // Retrieves the skinned mesh renderers with blend shapes if the list is empty.
            if (SkinnedMeshRenderersWithBlendShapes.Count == 0)
                GetSkinnedMeshRenderersWithBlendShapes(Character);

            // Serialization  of the skinned mesh renderer list.
            EditorWindow thisEditorWindow = this;
            SerializedObject serializedWindows = new SerializedObject(thisEditorWindow);
            SerializedProperty skinnedMeshRenderersProperty = serializedWindows.FindProperty("SkinnedMeshRenderersWithBlendShapes");

            // Property field to display and edit the skinned mesh renderers for extracting their values.
            string skinnedMeshRendererTooltip = "Skinned mesh renderers to extract the blenshapes values from.";
            EditorGUILayout.PropertyField(skinnedMeshRenderersProperty, new GUIContent("Skinned mesh renderers", skinnedMeshRendererTooltip), true);
            serializedWindows.ApplyModifiedProperties();
        }

        else
        {
            // Clears the skinned mesh renderer list if it contains data when no character is assigned to the object field.
            if (SkinnedMeshRenderersWithBlendShapes.Any())
                SkinnedMeshRenderersWithBlendShapes.Clear();
        }

        #endregion

        #region Blend shapes options

        GUILayout.Space(10);
        GUILayout.Label("Blend shapes options", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Save and reset buttons are enabled when a blend shapes mapper and a skinned mesh renderer are added.
        GUI.enabled = BlendShapesMapper && SkinnedMeshRenderersWithBlendShapes.Any() ? true : false;

        if (GUILayout.Button("Load blend shape values"))
            _unsavedDataSafetyEnabled = true;

        if(_unsavedDataSafetyEnabled)
        {
            EditorGUILayout.HelpBox("Current character blend shape values won't be saved!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
                _unsavedDataSafetyEnabled = false;

            if (GUILayout.Button("Continue"))
            {
                LoadBlendShapesValues();
                _unsavedDataSafetyEnabled = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Save blend shape values"))
        {
            // Enables a warning message if existing values are about to be overwritten.
            if (BlendShapesMapper.GetBlenShapeValues(FacialPoseToEdit).Any())
                _eraseDataSafetyEnabled = true;

            else
                SaveBlendShapesValues();
        }

        // Displays a warning message and asks for confirmation before overwriting existing data.
        if (_eraseDataSafetyEnabled)
        {
            EditorGUILayout.HelpBox("Existing "+ FacialPoseToEdit.ToString() + " blend shape values will be overwritten!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
                _eraseDataSafetyEnabled = false;

            if (GUILayout.Button("Continue"))
            {
                SaveBlendShapesValues();
                _eraseDataSafetyEnabled = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Reset blend shape values"))
            ResetBlendShapeValues();

        GUI.enabled = true;

        #endregion
    }

    // Detect and add the character's skinned mesh renderers with blend shapes to a list.
    private void GetSkinnedMeshRenderersWithBlendShapes(GameObject character)
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                SkinnedMeshRenderersWithBlendShapes.Add(skinnedMeshRenderer);
        }
    }

    // Load the current character's blend shape values from the preset.
    private void LoadBlendShapesValues()
    {
        List<float> blenshapesValues = new List<float>(BlendShapesMapper.GetBlenShapeValues(FacialPoseToEdit));

        int blendShapeIndex = 0;

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in SkinnedMeshRenderersWithBlendShapes)
        {
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
            {
                if (blendShapeIndex < blenshapesValues.Count)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(i, blenshapesValues[blendShapeIndex]);
                    blendShapeIndex++;
                }
            }
        }
    }

    // Save the current character's blend shape values.
    private void SaveBlendShapesValues()
    {
        List<float> blenShapeValues = new List<float>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in SkinnedMeshRenderersWithBlendShapes)
        {
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                blenShapeValues.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));
        }

        BlendShapesMapper.SetBlendShapeValues(FacialPoseToEdit, blenShapeValues);

        EditorUtility.SetDirty(BlendShapesMapper);
    }

    // Reset the character's blend shape values in the scene.
    private void ResetBlendShapeValues()
    {
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in SkinnedMeshRenderersWithBlendShapes)
        {
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
        }

        EditorUtility.SetDirty(Character);
    }
}