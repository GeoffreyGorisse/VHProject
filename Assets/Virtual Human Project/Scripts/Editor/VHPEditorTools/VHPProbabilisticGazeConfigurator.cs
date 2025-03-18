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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class VHPProbabilisticGazeConfigurator : EditorWindow
{
    public GameObject TargetPrefab;
    public List<Transform> GazeTargets = new List<Transform>();

    private List<GameObject> _sceneGazeTargets = new List<GameObject>();
    private string _targetPrefabFolder = "Assets/Virtual Human Project/Prefabs/";
    private bool _deleteTargetSafetyEnabled = false;

    [MenuItem("Window/Virtual Human Project/Probabilistic Gaze Configurator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<VHPProbabilisticGazeConfigurator>("Probabilistic Gaze Configurator");
    }

    private void OnEnable()
    {
        // Load the target prefab from the VHP project folder.
        loadTargetPrefab();

        // Sets the collision matrix to prevent collisions between gaze targets and other colliders.
        for (int i = 0; i < 32 ; i++)
            if(i != TargetPrefab.layer)
                Physics.IgnoreLayerCollision(TargetPrefab.layer, i, true);

        // Adds existing targets from the scene to a list.
        if (GameObject.FindGameObjectsWithTag("GazeTarget") != null)
            _sceneGazeTargets.AddRange(GameObject.FindGameObjectsWithTag("GazeTarget"));
    }

    private void OnDisable()
    {
        // Clears the list of existing targets.
        _sceneGazeTargets.Clear();
    }

    private void OnGUI()
    {
        #region Gaze target settings

        GUILayout.Label("Gaze target settings", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Object field to expose/add the gaze target prefab to be instanciated on the objects of the target list.
        string targetPrefabTooltip = "Gaze target prefab to be instantiated, allowing objects to be considered by the probabilistic eye gaze model.The target prefab must be located in the following folder: " + _targetPrefabFolder;
        TargetPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Gaze target prefab", targetPrefabTooltip), TargetPrefab, typeof(GameObject), true);

        if(TargetPrefab)
        {
            // Serialization of the target list.
            EditorWindow thisEditorWindow = this;
            SerializedObject serializedWindows = new SerializedObject(thisEditorWindow);
            SerializedProperty gazeTargetsProperty = serializedWindows.FindProperty("GazeTargets");

            // Property field to display and edit the target list.
            string gazeTargetsTooltip = "Gaze targets to be considered by the probabilistic model.";
            EditorGUILayout.PropertyField(gazeTargetsProperty, new GUIContent("Gaze targets", gazeTargetsTooltip), true);
            serializedWindows.ApplyModifiedProperties();

            if (GUILayout.Button("Add selected objects"))
                AddObjectToTargetList();

            GUI.enabled = GazeTargets.Any() ? true : false;

            if (GUILayout.Button("Clear target list"))
                GazeTargets.Clear();

            GUI.enabled = true;
        }

        #endregion

        #region Scene targets options

        GUILayout.Space(10);
        GUILayout.Label("Scene targets options", EditorStyles.boldLabel);
        GUILayout.Space(5);

        GUI.enabled = GazeTargets.Any() ? true : false;

        if (GUILayout.Button("Add target prefabs"))
                AddTargetPrefabToObjects();

        GUI.enabled = true;

        GUI.enabled = _sceneGazeTargets.Any() ? true : false;

        if (GUILayout.Button("Remove all targets"))
            _deleteTargetSafetyEnabled = true;

        GUI.enabled = true;

        // Displays a warning message asking for confirmation before deleting existing targets.
        if (_deleteTargetSafetyEnabled)
        {
            EditorGUILayout.HelpBox("All gaze targets will be removed!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
                _deleteTargetSafetyEnabled = false;

            if (GUILayout.Button("Continue"))
            {
                DestroyTargetPrefabs();
                _deleteTargetSafetyEnabled = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }

    // Loads the target prefab from the VHP project folder.
    private void loadTargetPrefab()
    {
        string targetPrefabFileName = "Gaze_Target.prefab";
        string targetPrefabPath = _targetPrefabFolder + targetPrefabFileName;

        if (AssetDatabase.LoadAssetAtPath(targetPrefabPath, typeof(GameObject)))
            TargetPrefab = AssetDatabase.LoadAssetAtPath(targetPrefabPath, typeof(GameObject)) as GameObject;

        else
            Debug.LogWarning("Cannot load the gaze target prefab to be instantiated at path: " + targetPrefabPath + ". Please assign the target prefab manually.");
    }

    // Instantiates target prefabs as children of each object in the target list.
    private void AddObjectToTargetList()
    {
        Transform[] selectedTransforms = Selection.transforms;

        // Adds the selected transforms to the list if they are not already contained.
        foreach (Transform selectedTransform in selectedTransforms)
            if(!GazeTargets.Contains(selectedTransform))
                GazeTargets.Add(selectedTransform);
    }

    // Instantiates target prefabs as children of each object in the target list.
    private void AddTargetPrefabToObjects()
    {
        if (GazeTargets.Any())
        {
            // Adds a target prefab to each object in the list if it doesn't already have one.
            foreach (Transform gazeTarget in GazeTargets)
            {
                bool alreadyContainsTarget = false;

                foreach (Transform child in gazeTarget)
                {
                    if (child.CompareTag("GazeTarget"))
                        alreadyContainsTarget = true;
                    break;
                }

                if (!alreadyContainsTarget)
                {
                    GameObject gazeTargetPrefabInstance = Instantiate(TargetPrefab);
                    gazeTargetPrefabInstance.name = "Gaze_Target";
                    gazeTargetPrefabInstance.transform.parent = gazeTarget;
                    gazeTargetPrefabInstance.transform.position = gazeTarget.position;

                    // Matches the size of the collider to the object's bounds size.
                    if (gazeTarget.transform.GetComponent<Renderer>())
                    {
                        Vector3 targetBoundsSize = gazeTarget.transform.GetComponent<Renderer>().bounds.size;
                        float maxSizeValue = Mathf.Max(targetBoundsSize.x, targetBoundsSize.y, targetBoundsSize.z);
                        gazeTargetPrefabInstance.transform.GetComponent<SphereCollider>().radius = maxSizeValue / 2;
                    }

                    _sceneGazeTargets.Add(gazeTargetPrefabInstance);
                }
            }
        }

        else
            Debug.LogWarning("Empty gaze targets list. No target will be added!");
    }

    // Destroys target prefabs in the scene.
    private void DestroyTargetPrefabs()
    {
        foreach (GameObject gazeTarget in _sceneGazeTargets)
            DestroyImmediate(gazeTarget);

        _sceneGazeTargets.Clear();
    }
}