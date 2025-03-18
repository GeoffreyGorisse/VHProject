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

    private List<SkinnedMeshRenderer> _skinnedMeshRenderersWithBlendShapes = new List<SkinnedMeshRenderer>();
    private VHPEmotions _VHPEmotions;
    private VHPGaze _VHPGaze;
    private VHPLipSync _VHPLipSync;
    private float[] _emotionBlendShapeValues;
    private float[] _gazeBlendShapeValues;
    private float[] _lipBlendShapeValues;
    private float[] _prioritizedBlendShapeValues;
    private float[] _previousPrioritizedBlendShapeValues;

    private void Awake()
    {
        if (!blendShapesMapperPreset)
        {
            Debug.LogWarning("No blend shapes mapper preset! Please assign a mapper to enable procedural animations.");
            return;
        }

        GetSkinnedMeshRenderersWithBlendShapes(gameObject);
    }

    private void OnEnable()
    {
        if (gameObject.GetComponent<VHPEmotions>())
        {
            _VHPEmotions = gameObject.GetComponent<VHPEmotions>();
            _VHPEmotions.OnEmotionsChange += GetEmotionBlendShapeValues;
        }

        if (gameObject.GetComponent<VHPGaze>())
        {
            _VHPGaze = gameObject.GetComponent<VHPGaze>();
            _VHPGaze.OnGazeChange += GetGazeBlendShapeValues;
        }

        if (gameObject.GetComponent<VHPLipSync>())
        {
            _VHPLipSync = gameObject.GetComponent<VHPLipSync>();
            _VHPLipSync.OnLipChange += GetLipBlendShapeValues;
        }

        _emotionBlendShapeValues = new float[TotalCharacterBlendShapes];
        _gazeBlendShapeValues = new float[TotalCharacterBlendShapes];
        _lipBlendShapeValues = new float[TotalCharacterBlendShapes];

        _prioritizedBlendShapeValues = new float[TotalCharacterBlendShapes];
        _previousPrioritizedBlendShapeValues = new float[TotalCharacterBlendShapes];
    }

    private void OnDisable()
    {
        if (_VHPEmotions)
            _VHPEmotions.OnEmotionsChange -= GetEmotionBlendShapeValues;

        if (_VHPGaze)
            _VHPGaze.OnGazeChange -= GetGazeBlendShapeValues;

        if (_VHPLipSync)
            _VHPLipSync.OnLipChange -= GetLipBlendShapeValues;

        ResetBlendShapeValues();
    }

    private void Update()
    {
        PrioritizeBlendShapeValues();
    }

    // Get the skinned mesh renderers with blend shapes of the character.
    private void GetSkinnedMeshRenderersWithBlendShapes(GameObject character)
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                _skinnedMeshRenderersWithBlendShapes.Add(skinnedMeshRenderer);
                TotalCharacterBlendShapes += skinnedMeshRenderer.sharedMesh.blendShapeCount;
            }
        }

        if (!_skinnedMeshRenderersWithBlendShapes.Any())
            Debug.LogWarning("No skinned mesh renderer with blend shapes detected on the character!");
    }

    private void GetEmotionBlendShapeValues(float[] blendShapeValues)
    {
        _emotionBlendShapeValues = blendShapeValues;
    }

    private void GetGazeBlendShapeValues(float[] blendShapeValues)
    {
        _gazeBlendShapeValues = blendShapeValues;
    }

    private void GetLipBlendShapeValues(float[] blendShapeValues)
    {
        _lipBlendShapeValues = blendShapeValues;
    }

    // Prioritizes the blend shape values during concurrent activations (e.g., emotions, gaze, and lip sync blend shapes).
    private void PrioritizeBlendShapeValues()
    {
        if (_skinnedMeshRenderersWithBlendShapes.Any())
        {
            // Prioritizes lip sync blend shapes first. Emotion blend shapes are applied next, ensuring they don't override lip sync values, followed by gaze blend shapes.
            for (int i = 0; i < TotalCharacterBlendShapes; i++)
            {
                if (_lipBlendShapeValues[i] != 0)
                    _prioritizedBlendShapeValues[i] = _lipBlendShapeValues[i];

                else if (_emotionBlendShapeValues[i] != 0)
                    _prioritizedBlendShapeValues[i] = _emotionBlendShapeValues[i];

                else if (_gazeBlendShapeValues[i] != 0)
                    _prioritizedBlendShapeValues[i] = _gazeBlendShapeValues[i];

                else
                    _prioritizedBlendShapeValues[i] = 0;
            }

            // Updates the blend shape values only if they differ from the previous ones.
            if (_prioritizedBlendShapeValues != _previousPrioritizedBlendShapeValues)
            {
                StopAllCoroutines();
                StartCoroutine(LerpBlendShapeValues(_prioritizedBlendShapeValues));

                System.Array.Copy(_prioritizedBlendShapeValues, _previousPrioritizedBlendShapeValues, TotalCharacterBlendShapes);
            }
        }

        else
            Debug.LogWarning("No skinned mesh renderers with blend shapes!");
    }

    // Interpolates the blend shape values to create a smooth transition between their previous and target values.
    private IEnumerator LerpBlendShapeValues(float[] blendShapeValues)
    {
        List<float> initialBlenshapeValues = new List<float>();
        float elapsedTime = 0;
        float lerpDuration = 0.05f;
        float currentBlendShapeValue;

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in _skinnedMeshRenderersWithBlendShapes)
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                initialBlenshapeValues.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));

        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;

            int blendShapeIndex = 0;

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in _skinnedMeshRenderersWithBlendShapes)
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

    // Resets the character's blend shape values to their default state.
    private void ResetBlendShapeValues()
    {
        if (_skinnedMeshRenderersWithBlendShapes.Any())
        {
            int blendshapeIndex = 0;

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in _skinnedMeshRenderersWithBlendShapes)
            {
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(i, 0);

                    blendshapeIndex++;
                }
            }
        }

        else
            Debug.LogWarning("No skinned mesh renderers with blend shapes!");
    }
}