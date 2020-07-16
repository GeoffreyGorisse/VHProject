/********************************************************************
Filename    :   VHPProbabilisticGazeConfigurator.cs
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

public class VHPProbabilisticGazeConfigurator : EditorWindow
{
    public GameObject targetPrefab;
    public List<Transform> gazeTargets = new List<Transform>();

    private List<GameObject> m_sceneGazeTargets = new List<GameObject>();
    private string m_targetPrefabFolder = "Assets/Virtual Human Project/Prefabs/";
    private bool m_deleteTargetSafetyEnabled = false;

    [MenuItem("Window/Virtual Human Project/Probabilistic Gaze Configurator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<VHPProbabilisticGazeConfigurator>("Probabilistic Gaze Configurator");
    }

    private void OnEnable()
    {
        // Calling the function to load the target prefab from the VHP project folder.
        loadTargetPrefab();
        
        // Setting the collision matrix to avoid collisions between the gaze targets and other colliders.
        for (int i = 0; i < 32 ; i++)
            if(i != targetPrefab.layer)
                Physics.IgnoreLayerCollision(targetPrefab.layer, i, true);

        // Adding existing targets from the scene in a list.
        if (GameObject.FindGameObjectsWithTag("GazeTarget") != null)
            m_sceneGazeTargets.AddRange(GameObject.FindGameObjectsWithTag("GazeTarget"));
    }

    private void OnDisable()
    {
        // Clearing the list of existing targets.
        m_sceneGazeTargets.Clear();
    }

    private void OnGUI()
    {
        #region Gaze target settings

        GUILayout.Label("Gaze target settings", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Object field to expose/add the gaze target prefab to be instanciated on the objects of the target list.
        string targetPrefabTooltip = "Gaze target prefab to be instanciated to allow objects to be taken into account by the probabilistic eye gaze model. The target prefab must be located in the following folder: " + m_targetPrefabFolder;
        targetPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Gaze target prefab", targetPrefabTooltip), targetPrefab, typeof(GameObject), true);

        if(targetPrefab)
        {
            // Serialazation of the target list.
            EditorWindow thisEditorWindow = this;
            SerializedObject serializedWindows = new SerializedObject(thisEditorWindow);
            SerializedProperty gazeTargetsProperty = serializedWindows.FindProperty("gazeTargets");

            // Property field to display/edit the target list.
            string gazeTargetsTooltip = "Gaze targets to be considered by the probabilistic model.";
            EditorGUILayout.PropertyField(gazeTargetsProperty, new GUIContent("Gaze targets", gazeTargetsTooltip), true);
            serializedWindows.ApplyModifiedProperties();

            // Button to add the selected objects in the hierarchy to the target list.
            if (GUILayout.Button("Add selected objects"))
                AddObjectToTargetList();

            GUI.enabled = gazeTargets.Any() ? true : false;

            // Button to clear the target list.
            if (GUILayout.Button("Clear target list"))
                gazeTargets.Clear();

            GUI.enabled = true;
        }

        #endregion

        #region Scene targets options

        GUILayout.Space(10);
        GUILayout.Label("Scene targets options", EditorStyles.boldLabel);
        GUILayout.Space(5);

        GUI.enabled = gazeTargets.Any() ? true : false;

        // Button to call the function to instantiate target prefabs as child of each object in the target list.
        if (GUILayout.Button("Add target prefabs"))
                AddTargetPrefabToObjects();

        GUI.enabled = true;

        GUI.enabled = m_sceneGazeTargets.Any() ? true : false;

        // Button enabling the warning message to remove all the target prefabs from the scene.
        if (GUILayout.Button("Remove all targets"))
            m_deleteTargetSafetyEnabled = true;

        GUI.enabled = true;

        // Displaying the warning message and asking for a confirmation to delete existing targets.
        if (m_deleteTargetSafetyEnabled)
        {
            EditorGUILayout.HelpBox("All gaze targets will be removed!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
                m_deleteTargetSafetyEnabled = false;

            if (GUILayout.Button("Continue"))
            {
                DestroyTargetPrefabs();
                m_deleteTargetSafetyEnabled = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }

    // Function to load the target prefab from the VHP project folder.
    private void loadTargetPrefab()
    {
        string targetPrefabFileName = "Gaze_Target.prefab";
        string targetPrefabPath = m_targetPrefabFolder + targetPrefabFileName;

        if (AssetDatabase.LoadAssetAtPath(targetPrefabPath, typeof(GameObject)))
            targetPrefab = AssetDatabase.LoadAssetAtPath(targetPrefabPath, typeof(GameObject)) as GameObject;

        else
            Debug.LogWarning("Cannot load the gaze target prefab to be instanciated at path: " + targetPrefabPath + ". Please assign the target prefab manually.");
    }

    // Function to instantiate target prefabs as child of each object in the target list.
    private void AddObjectToTargetList()
    {
        // Getting all the selected transforms.
        Transform[] selectedTransforms = Selection.transforms;

        // Adding the selected transforms to the list if they are not already contained.
        foreach (Transform selectedTransform in selectedTransforms)
            if(!gazeTargets.Contains(selectedTransform))
                gazeTargets.Add(selectedTransform);
    }

    // Function to instantiate target prefabs as child of each object in the target list.
    private void AddTargetPrefabToObjects()
    {
        if (gazeTargets.Any())
            // Adding a target prefab to each object in the list if it doesn't already contain one.
            foreach (Transform gazeTarget in gazeTargets)
            {
                bool alreadyContainsTarget = false;

                foreach (Transform child in gazeTarget)
                {
                    if (child.CompareTag("GazeTarget"))
                        alreadyContainsTarget = true;
                    break;
                }

                if(!alreadyContainsTarget)
                {
                    GameObject gazeTargetPrefabInstance = Instantiate(targetPrefab);
                    gazeTargetPrefabInstance.name = "Gaze_Target";
                    gazeTargetPrefabInstance.transform.parent = gazeTarget;
                    gazeTargetPrefabInstance.transform.position = gazeTarget.position;

                    // Matching the size of the collider with the object's bound size.
                    if(gazeTarget.transform.GetComponent<Renderer>())
                    {
                        Vector3 targetBoundsSize = gazeTarget.transform.GetComponent<Renderer>().bounds.size;
                        float maxSizeValue = Mathf.Max(targetBoundsSize.x, targetBoundsSize.y, targetBoundsSize.z);
                        gazeTargetPrefabInstance.transform.GetComponent<SphereCollider>().radius = maxSizeValue / 2;
                    }

                    m_sceneGazeTargets.Add(gazeTargetPrefabInstance);
                }
            }

        else
            Debug.LogWarning("Empty gaze targets list, no target will be added.");
    }

    // Function to destroy target prefabs in the scene.
    private void DestroyTargetPrefabs()
    {
        foreach (GameObject gazeTarget in m_sceneGazeTargets)
            DestroyImmediate(gazeTarget);

        m_sceneGazeTargets.Clear();
    }
}
