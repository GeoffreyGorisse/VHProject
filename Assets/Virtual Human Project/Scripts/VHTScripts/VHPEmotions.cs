/********************************************************************
Filename    :   VHPEmotions.cs
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
using UnityEngine;

[RequireComponent(typeof(VHPManager))]
public class VHPEmotions : MonoBehaviour
{
    public enum Emotions
    {
        ANGER,
        DISGUST,
        FEAR,
        HAPPINESS,
        SADNESS,
        SURPRISE,
        NONE
    }

    // Intensity values for emotions, used to adjust the associated blend shapes.
    [Header("Emotion intensity")]
    [Range(0, 100)] public float AngerIntensity = 0f;
    [Range(0, 100)] public float DisgustIntensity = 0f;
    [Range(0, 100)] public float FearIntensity = 0f;
    [Range(0, 100)] public float HappinessIntensity = 0f;
    [Range(0, 100)] public float SadnessIntensity = 0f;
    [Range(0, 100)] public float SurpriseIntensity = 0f;

    // Delegate and event allowing the VHP manager to subscribe a function that updates the character's blend shapes with new emotion values.
    public delegate void OnEmotionChangeDelegate(float[] currentEmotionsBlendShapeValues);
    public event OnEmotionChangeDelegate OnEmotionsChange;

    private VHPManager _VHPmanager;

    private List<float> _angerBlendShapeValues = new List<float>();
    private List<float> _disgustBlendShapeValues = new List<float>();
    private List<float> _fearBlendShapeValues = new List<float>();
    private List<float> _happinessBlendShapeValues = new List<float>();
    private List<float> _sadnessBlendShapeValues = new List<float>();
    private List<float> _surpriseBlendShapeValues = new List<float>();

    [Range(0, 100)] private float _currentAngerIntensity;
    [Range(0, 100)] private float _currentDisgustIntensity;
    [Range(0, 100)] private float _currentFearIntensity;
    [Range(0, 100)] private float _currentHappinessIntensity;
    [Range(0, 100)] private float _currentSadnessIntensity;
    [Range(0, 100)] private float _currentSurpriseIntensity;

    private Emotions _currentEmotion = Emotions.NONE;

    private void Awake()
    {
        _VHPmanager = gameObject.GetComponent<VHPManager>();

        LoadBlendShapeValues();
    }

    private void OnDisable()
    {
        _currentAngerIntensity = 0f;
        _currentDisgustIntensity = 0f;
        _currentFearIntensity = 0f;
        _currentHappinessIntensity = 0f;
        _currentSadnessIntensity = 0f;
        _currentSurpriseIntensity = 0f;

        SyncEmotionsIntensityValues();
    }

    private void Update()
    {
        SetCurrentEmotionIntensityValues();
    }

    #region Blend shape values initilization

    // Loads the maximum emotion blend shape values from the blend shapes mapper.
    private void LoadBlendShapeValues()
    {
        if (_VHPmanager.blendShapesMapperPreset)
        {
            BlendShapesMapper blendShapesMapper = _VHPmanager.blendShapesMapperPreset;

            // Calling the function to copy the values from the blendshapes mapper added to the VHP manager.
            CopyBlendshapesMapperValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.ANGER), _angerBlendShapeValues);
            CopyBlendshapesMapperValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.DISGUST), _disgustBlendShapeValues);
            CopyBlendshapesMapperValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.FEAR), _fearBlendShapeValues);
            CopyBlendshapesMapperValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.HAPPINESS), _happinessBlendShapeValues);
            CopyBlendshapesMapperValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.SADNESS), _sadnessBlendShapeValues);
            CopyBlendshapesMapperValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.SURPRISE), _surpriseBlendShapeValues);
        }

        else
        {
            Debug.LogWarning("No blend shapes preset! Procedural emotions will not be initialized.");
            return;
        }
    }

    // Copies the values from the blend shapes mapper.
    private void CopyBlendshapesMapperValues(List<float> blendShapesMapperValues, List<float> emotionBlendShapeValues)
    {
        for (int i = 0; i < blendShapesMapperValues.Count; i++)
        {
            if (blendShapesMapperValues[i] >= 0 && blendShapesMapperValues[i] <= 100)
                emotionBlendShapeValues.Add(blendShapesMapperValues[i]);

            else
                emotionBlendShapeValues.Add(0);
        }
    }

    #endregion

    #region Setting emotions blenshapes intensity

    // Sets the current emotion blend shape values.
    private void SetCurrentEmotionIntensityValues()
    {
        float[] currentEmotionsIntensityValues = { _currentAngerIntensity, _currentDisgustIntensity, _currentFearIntensity, _currentHappinessIntensity, _currentSadnessIntensity, _currentSurpriseIntensity };
        float[] requestedEmotionsIntensityValues = { AngerIntensity, DisgustIntensity, FearIntensity, HappinessIntensity, SadnessIntensity, SurpriseIntensity };

        for (int i = 0; i < currentEmotionsIntensityValues.Length; i++)
        {
            // Detects a significant difference in the requested emotion intensities to update the blend shape values.
            if (requestedEmotionsIntensityValues[i] > currentEmotionsIntensityValues[i] + 1 || requestedEmotionsIntensityValues[i] < currentEmotionsIntensityValues[i] - 1)
            {
                currentEmotionsIntensityValues[i] = requestedEmotionsIntensityValues[i];
                _currentEmotion = (Emotions)i;

                // All intensity values are set to 0, as they are mutually exclusive.
                for (int j = 0; j < currentEmotionsIntensityValues.Length; j++)
                    if (j != (int)_currentEmotion)
                        currentEmotionsIntensityValues[j] = 0;

                _currentAngerIntensity = currentEmotionsIntensityValues[0];
                _currentDisgustIntensity = currentEmotionsIntensityValues[1];
                _currentFearIntensity = currentEmotionsIntensityValues[2];
                _currentHappinessIntensity = currentEmotionsIntensityValues[3];
                _currentSadnessIntensity = currentEmotionsIntensityValues[4];
                _currentSurpriseIntensity = currentEmotionsIntensityValues[5];

                SyncEmotionsIntensityValues();
                UpdateEmotionBlendShapeValues();

                break;
            }
        }
    }

    // Synchronizes the current and requested emotion intensity values.
    private void SyncEmotionsIntensityValues()
    {
        AngerIntensity = _currentAngerIntensity;
        DisgustIntensity = _currentDisgustIntensity;
        FearIntensity = _currentFearIntensity;
        HappinessIntensity = _currentHappinessIntensity;
        SadnessIntensity = _currentSadnessIntensity;
        SurpriseIntensity = _currentSurpriseIntensity;
    }

    // Updates the current emotion blend shape values and triggers the event to allow the VHP manager to update the character's blend shapes with the new gaze values.
    private void UpdateEmotionBlendShapeValues()
    {
        float[] currentEmotionBlendShapeValues = new float[_VHPmanager.TotalCharacterBlendShapes];

        switch (_currentEmotion)
        {
            case Emotions.ANGER:
                for (int i = 0; i < _angerBlendShapeValues.Count; i++)
                    currentEmotionBlendShapeValues[i] = (Mathf.Clamp((_currentAngerIntensity * _angerBlendShapeValues[i]) / 100, 0f, 100f));
                break;
            case Emotions.DISGUST:
                for (int i = 0; i < _disgustBlendShapeValues.Count; i++)
                    currentEmotionBlendShapeValues[i] = (Mathf.Clamp((_currentDisgustIntensity * _disgustBlendShapeValues[i]) / 100, 0f, 100f));
                break;
            case Emotions.FEAR:
                for (int i = 0; i < _fearBlendShapeValues.Count; i++)
                    currentEmotionBlendShapeValues[i] = (Mathf.Clamp((_currentFearIntensity * _fearBlendShapeValues[i]) / 100, 0f, 100f));
                break;
            case Emotions.HAPPINESS:
                for (int i = 0; i < _happinessBlendShapeValues.Count; i++)
                    currentEmotionBlendShapeValues[i] = (Mathf.Clamp((_currentHappinessIntensity * _happinessBlendShapeValues[i]) / 100, 0f, 100f));
                break;
            case Emotions.SADNESS:
                for (int i = 0; i < _sadnessBlendShapeValues.Count; i++)
                    currentEmotionBlendShapeValues[i] = (Mathf.Clamp((_currentSadnessIntensity * _sadnessBlendShapeValues[i]) / 100, 0f, 100f));
                break;
            case Emotions.SURPRISE:
                for (int i = 0; i < _surpriseBlendShapeValues.Count; i++)
                    currentEmotionBlendShapeValues[i] = (Mathf.Clamp((_currentSurpriseIntensity * _surpriseBlendShapeValues[i]) / 100, 0f, 100f));
                break;
            case Emotions.NONE:
                break;
        }

        // If any function is subscribed to the emotions change event, the associated delegate is invoked with the current emotion blend shape values as parameter.
        OnEmotionsChange?.Invoke(currentEmotionBlendShapeValues);
    }

    #endregion
}