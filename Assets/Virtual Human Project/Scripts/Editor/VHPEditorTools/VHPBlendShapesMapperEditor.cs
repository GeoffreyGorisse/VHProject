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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class VHPBlendShapesMapperEditor : EditorWindow
{
    public BlendShapesMapper blendShapesMapper;
    public BlendShapesMapper.FacialExpression facialPoseToEdit = BlendShapesMapper.FacialExpression.DEFAULT;

    public GameObject character;
    public List<SkinnedMeshRenderer> skinnedMeshRenderersWithBlendShapes = new List<SkinnedMeshRenderer>();

    private bool m_eraseDataSafetyEnabled = false;
    private bool m_notSavedDataSafetyEnabled = false;

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
        blendShapesMapper = (BlendShapesMapper)EditorGUILayout.ObjectField(new GUIContent("Mapper preset", blendShapesMapperTooltip), blendShapesMapper, typeof(BlendShapesMapper), true);

        GUI.enabled = blendShapesMapper;
  
        string facialPoseToEditTooltip = "Facial expression preset to edit.";
        // Enumeration to choose the facial expression preset to be edited.
        facialPoseToEdit = (BlendShapesMapper.FacialExpression)EditorGUILayout.EnumPopup(new GUIContent("Facial Expression", facialPoseToEditTooltip), facialPoseToEdit);

        GUI.enabled = true;

        #endregion

        #region Character settings

        GUILayout.Space(10);
        GUILayout.Label("Character settings", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Object field to add a character gameobject to get its skinned mesh renderers with blend shapes.
        string characterTooltip = "Character's root gameobject to extract children skinned mesh renderer blend shapes.";
        character = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Character root", characterTooltip), character, typeof(GameObject), true);

        if(character)
        {
            // Getting the skinned mesh renderers with blend shapes if the list is empty.
            if (skinnedMeshRenderersWithBlendShapes.Count == 0)
                GetSkinnedMeshRenderersWithBlendShapes(character);

            // Serialazation of the skinned mesh renderer list.
            EditorWindow thisEditorWindow = this;
            SerializedObject serializedWindows = new SerializedObject(thisEditorWindow);
            SerializedProperty skinnedMeshRenderersProperty = serializedWindows.FindProperty("skinnedMeshRenderersWithBlendShapes");

            // Property field to display/edit the skinned mesh renderers to extract the values from.
            string skinnedMeshRendererTooltip = "Skinned mesh renderers to extract the blenshapes values from.";
            EditorGUILayout.PropertyField(skinnedMeshRenderersProperty, new GUIContent("Skinned mesh renderers", skinnedMeshRendererTooltip), true);
            serializedWindows.ApplyModifiedProperties();
        }

        else
        {
            // Clearing the skinned mesh renderer list if it contains data if no character is added to the object field.
            if (skinnedMeshRenderersWithBlendShapes.Any())
                skinnedMeshRenderersWithBlendShapes.Clear();
        }

        #endregion

        #region Blend shapes options

        GUILayout.Space(10);
        GUILayout.Label("Blend shapes options", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // The save and reset buttons are enabled if a blend shapes mapper and a skinned mesh renderer are added.
        GUI.enabled = blendShapesMapper && skinnedMeshRenderersWithBlendShapes.Any() ? true : false;

        // Button calling the function to save the current character blend shapes values in the preset.
        if (GUILayout.Button("Load blend shape values"))
            m_notSavedDataSafetyEnabled = true;

        if(m_notSavedDataSafetyEnabled)
        {
            EditorGUILayout.HelpBox("Current character blend shape values won't be saved!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
                m_notSavedDataSafetyEnabled = false;

            if (GUILayout.Button("Continue"))
            {
                LoadBlendShapesValues();
                m_notSavedDataSafetyEnabled = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        // Button calling the function to save the current character blend shape values in the preset.
        if (GUILayout.Button("Save blend shape values"))
        {
            // Enabling a warning message if existing values will be overwritten.
            if(blendShapesMapper.GetBlenShapeValues(facialPoseToEdit).Any())
                m_eraseDataSafetyEnabled = true;

            else
                SaveBlendShapesValues();
        }

        // Displaying the warning message and asking for a confirmation to overwrite existing data.
        if (m_eraseDataSafetyEnabled)
        {
            EditorGUILayout.HelpBox("Existing "+ facialPoseToEdit.ToString() + " blend shape values will be overwritten!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
                m_eraseDataSafetyEnabled = false;

            if (GUILayout.Button("Continue"))
            {
                SaveBlendShapesValues();
                m_eraseDataSafetyEnabled = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        // Button calling the function to reset the character blend shape values in the scene.
        if (GUILayout.Button("Reset character blend shapes"))
            ResetBlendShapeValues();

        GUI.enabled = true;

        #endregion
    }

    // Function to detect and add in a list the skinned mesh renderers with blend shapes of the character.
    private void GetSkinnedMeshRenderersWithBlendShapes(GameObject character)
    {
        // Getting all the child objects with a skinned mesh renderer.
        SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();

        // Each skinned mesh renderer containing blend shapes is added to the list of skinned mesh renderers with blend shapes.
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                skinnedMeshRenderersWithBlendShapes.Add(skinnedMeshRenderer);
        }
    }

    // Function allowing to load the current character blend shape values contained in the preset.
    private void LoadBlendShapesValues()
    {
        // Temporary list to store the character blend shape values.
        List<float> blenshapesValues = new List<float>(blendShapesMapper.GetBlenShapeValues(facialPoseToEdit));

        int blendShapeIndex = 0;

        // For each skinned mesh renderer of the character all the blend shape values are loaded from the preset.
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderersWithBlendShapes)
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

    // Function allowing to save the current character blend shape values.
    private void SaveBlendShapesValues()
    {
        // Temporary list to store the character blend shape values.
        List<float> blenShapeValues = new List<float>();

        // Current character blend shape values are added to the temporary list.
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderersWithBlendShapes)
        {
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                blenShapeValues.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));
        }

        // The temporary list of values is sent with its corresponding facial expression to the blend shapes mapper to be saved.
        blendShapesMapper.SetBlendShapeValues(facialPoseToEdit, blenShapeValues);

        EditorUtility.SetDirty(blendShapesMapper);
    }

    // Function allowing to reset the character blend shape values in the scene.
    private void ResetBlendShapeValues()
    {
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderersWithBlendShapes)
        {
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
        }

        EditorUtility.SetDirty(character);
    }
}
