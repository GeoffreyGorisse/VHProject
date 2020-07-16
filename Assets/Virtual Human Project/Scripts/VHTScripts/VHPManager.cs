/********************************************************************
Filename    :   VHPManager.cs
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

public class VHPManager : MonoBehaviour
{
    [Header("Preset settings:")]
    [Tooltip("Blend shapes preset matching the character's template. Use Window -> Virtual Human Project -> Blend Shapes Mapper Editor to create a new preset.")]
    public BlendShapesMapper blendShapesMapperPreset;

    public int TotalCharacterBlendShapes { get; private set; } = 0;

    private List<SkinnedMeshRenderer> m_skinnedMeshRenderersWithBlendShapes = new List<SkinnedMeshRenderer>();

    private VHPEmotions m_VHPEmotions;
    private VHPGaze m_VHPGaze;
    private VHPLipSync m_VHPLipSync;

    private float[] m_emotionBlendShapeValues;
    private float[] m_gazeBlendShapeValues;
    private float[] m_lipBlendShapeValues;

    private float[] m_prioritizedBlendShapeValues;
    private float[] m_previousPrioritizedBlendShapeValues;

    private void Awake()
    {
        // Checking that a blenshapes mapper preset is added to the VHP manager.
        if (!blendShapesMapperPreset)
        {
            Debug.LogWarning("No blend shapes mapper preset. Please assign a mapper to enable procedural facial animations.");
            return;
        }

        // Getting the skinned mesh renderers with blend shapes of the character to use procedural facial animations.
        // Has to be executed in the Awake function as other VHP scripts require the total blend shape number to set their respective blenshapes values lists.
        GetSkinnedMeshRenderersWithBlendShapes(gameObject);
    }

    private void OnEnable()
    {
        GetVHPComponents();

        m_emotionBlendShapeValues = new float[TotalCharacterBlendShapes];
        m_gazeBlendShapeValues = new float[TotalCharacterBlendShapes];
        m_lipBlendShapeValues = new float[TotalCharacterBlendShapes];

        m_prioritizedBlendShapeValues = new float[TotalCharacterBlendShapes];
        m_previousPrioritizedBlendShapeValues = new float[TotalCharacterBlendShapes];

        SubscribeToBlendShapesEvent();
    }

    private void Update()
    {
        PrioritizeBlendShapeValues();
    }

    private void OnDisable()
    {
        UnsubscribeToBlendShapesEvent();
        ResetBlendShapeValues();
    }

    // Function to detect and add to a list the skinned mesh renderers with blend shapes of the character.
    private void GetSkinnedMeshRenderersWithBlendShapes(GameObject character)
    {
        // Getting all the child objects with a skinned mesh renderer.
        SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();

        // Each skinned mesh renderer containing blend shapes is added to the list of skinned mesh renderers with blend shapes.
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                m_skinnedMeshRenderersWithBlendShapes.Add(skinnedMeshRenderer);
                TotalCharacterBlendShapes += skinnedMeshRenderer.sharedMesh.blendShapeCount;
            }
        }

        // A warning message is displayed if the character does not contain any skinned mesh renderer with blendshapes.
        if (!m_skinnedMeshRenderersWithBlendShapes.Any())
            Debug.LogWarning("No skinned mesh renderer with blend shapes detected on the character.");
    }

    // Function to get all the VHP components.
    private void GetVHPComponents()
    {
        if(gameObject.GetComponent<VHPEmotions>())
            m_VHPEmotions = gameObject.GetComponent<VHPEmotions>();

        if (gameObject.GetComponent<VHPGaze>())
            m_VHPGaze = gameObject.GetComponent<VHPGaze>();

        if (gameObject.GetComponent<VHPLipSync>())
            m_VHPLipSync = gameObject.GetComponent<VHPLipSync>();
    }

    // Function to subscribe to all the events updating the blend shape values.
    private void SubscribeToBlendShapesEvent()
    {
        if(m_VHPEmotions)
            m_VHPEmotions.OnEmotionsChange += CollectEmotionBlendShapeValues;

        if(m_VHPGaze)
            m_VHPGaze.OnGazeChange += CollectGazeBlendShapeValues;

        if (m_VHPLipSync)
            m_VHPLipSync.OnLipChange += CollectLipBlendShapeValues;
    }

    // Function to unsubscribe to all the events updating the blendshapes values.
    private void UnsubscribeToBlendShapesEvent()
    {
        if (m_VHPEmotions)
            m_VHPEmotions.OnEmotionsChange -= CollectEmotionBlendShapeValues;

        if (m_VHPGaze)
            m_VHPGaze.OnGazeChange -= CollectGazeBlendShapeValues;

        if (m_VHPLipSync)
            m_VHPLipSync.OnLipChange -= CollectLipBlendShapeValues;
    }

    private void CollectEmotionBlendShapeValues(float[] blendShapeValues)
    {
        m_emotionBlendShapeValues = blendShapeValues;
    }

    private void CollectGazeBlendShapeValues(float[] blendShapeValues)
    {
        m_gazeBlendShapeValues = blendShapeValues;
    }

    private void CollectLipBlendShapeValues(float[] blendShapeValues)
    {
        m_lipBlendShapeValues = blendShapeValues;
    }

    // Function to prioritize the collected blend shape values (emotion, gaze and lip sync blend shapes).
    private void PrioritizeBlendShapeValues()
    {
        if (m_skinnedMeshRenderersWithBlendShapes.Any())
        {
            // Lip sync blend shapes are considered first. Then, emotions blend shapes are applied if they do not override the lip sync ones.
            // The same process is repeated with the gaze blend shapes. Finally 0 is added if no lip sync/emotion/gaze blend shape is applied.
            for (int i = 0; i < TotalCharacterBlendShapes; i++)
            {
                if (m_lipBlendShapeValues[i] != 0)
                    m_prioritizedBlendShapeValues[i] = m_lipBlendShapeValues[i];

                else if (m_emotionBlendShapeValues[i] != 0)
                    m_prioritizedBlendShapeValues[i] = m_emotionBlendShapeValues[i];

                else if (m_gazeBlendShapeValues[i] != 0)
                    m_prioritizedBlendShapeValues[i] = m_gazeBlendShapeValues[i];

                else
                    m_prioritizedBlendShapeValues[i] = 0;
            }

            // Starting the coroutine to update the character's blend shapes if the values are different from the previous ones.
            if (m_prioritizedBlendShapeValues != m_previousPrioritizedBlendShapeValues)
            {
                StopAllCoroutines();
                StartCoroutine(LerpBlendShapeValues(m_prioritizedBlendShapeValues));

                System.Array.Copy(m_prioritizedBlendShapeValues, m_previousPrioritizedBlendShapeValues, TotalCharacterBlendShapes);
            }
        }

        else
            Debug.LogWarning("No skinned mesh renderers with blend shapes");
    }

    public static int counter = 0;
    // Coroutine to interpolate the blend shape values to create a progressive transition between their intial and their targeted values.
    private IEnumerator LerpBlendShapeValues(float[] blendShapeValues)
    {
        List<float> initialBlenshapeValues = new List<float>();
        float elapsedTime = 0;
        float lerpDuration = 0.05f;
        float currentBlendShapeValue;

        // Storing the initial blend shape values.
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in m_skinnedMeshRenderersWithBlendShapes)
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                initialBlenshapeValues.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));

        // Updating smoothly the blend shape values from their initial values to the desired ones.
        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;

            int blendShapeIndex = 0;

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in m_skinnedMeshRenderersWithBlendShapes)
            {
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    if (initialBlenshapeValues[blendShapeIndex] != blendShapeValues[blendShapeIndex])
                    {
                        currentBlendShapeValue = Mathf.Lerp(initialBlenshapeValues[blendShapeIndex], blendShapeValues[blendShapeIndex], (elapsedTime / lerpDuration));
                        skinnedMeshRenderer.SetBlendShapeWeight(i, currentBlendShapeValue);
                    }

                    blendShapeIndex++;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    // Function to reset the character blend shape values.
    private void ResetBlendShapeValues()
    {
        if (m_skinnedMeshRenderersWithBlendShapes.Any())
        {
            int blendshapeIndex = 0;

            // For each skinned mesh renderer of the character all the blend shapes are updated according to list of values sent to the VHP manager.
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in m_skinnedMeshRenderersWithBlendShapes)
            {
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(i, 0);

                    blendshapeIndex++;
                }
            }
        }

        else
            Debug.LogWarning("No skinned mesh renderers with blend shapes");
    }
}
