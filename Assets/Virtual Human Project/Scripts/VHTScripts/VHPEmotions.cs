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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Public emotions values to allow other script to access them and to expose the sliders in the inspector.
    [Header("Emotion intensity")]
    [Range(0, 100)] public float anger = 0f;
    [Range(0, 100)] public float disgust = 0f;
    [Range(0, 100)] public float fear = 0f;
    [Range(0, 100)] public float happiness = 0f;
    [Range(0, 100)] public float sadness = 0f;
    [Range(0, 100)] public float surprise = 0f;
  
    // Delegate and event allowing the VHP manager to subscribe with a function that updates the character's blend shapes with the new emotions values as soon as they get updated.
    public delegate void OnEmotionChangeDelegate(float[] currentEmotionsBlendShapeValues);
    public event OnEmotionChangeDelegate OnEmotionsChange;

    private VHPManager m_VHPmanager;

    // Lists to copy the max values for each emotion from the blend shapes preset added to the VHP manager.
    private List<float> m_angerBlendShapeValues = new List<float>();
    private List<float> m_disgustBlendShapeValues = new List<float>();
    private List<float> m_fearBlendShapeValues = new List<float>();
    private List<float> m_happinessBlendShapeValues = new List<float>();
    private List<float> m_sadnessBlendShapeValues = new List<float>();
    private List<float> m_surpriseBlendShapeValues = new List<float>();

    // Private values to set the emotions' intensity.
    [Range(0, 100)] private float m_anger;
    [Range(0, 100)] private float m_disgust;
    [Range(0, 100)] private float m_fear;
    [Range(0, 100)] private float m_happiness;
    [Range(0, 100)] private float m_sadness;
    [Range(0, 100)] private float m_surprise;

    // Current emotion state of the character.
    private Emotions m_currentEmotion = Emotions.NONE;

    private void Awake()
    {
        m_VHPmanager = gameObject.GetComponent<VHPManager>();

        LoadBlendShapeValues();
    }

    private void Update()
    {
        SetCurrentEmotionBlendShapesIntensity();
    }

    private void OnDisable()
    {
        m_anger = 0f;
        m_disgust = 0f;
        m_fear = 0f;
        m_happiness = 0f;
        m_sadness = 0f;
        m_surprise = 0f;

        SyncEmotionsIntensityValues();
    }

    #region Loading blend shape values

    // Function to load the emotions max values from the blend shapes mapper added to the VHP manager.
    private void LoadBlendShapeValues()
    {
        // Loading the blend shapes mapper values to be used for procedural emotions if a mapper preset is added to the VHP manager.
        if (m_VHPmanager.blendShapesMapperPreset)
        {
            BlendShapesMapper blendShapesMapper = m_VHPmanager.blendShapesMapperPreset;

            // Calling the function to copy the values from the blendshapes mapper added to the VHP manager.
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.ANGER), m_angerBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.DISGUST), m_disgustBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.FEAR), m_fearBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.HAPPINESS), m_happinessBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.SADNESS), m_sadnessBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.SURPRISE), m_surpriseBlendShapeValues);
        }

        // Displaying a warning message if no blend shapes mapper is added to the VHP manager.
        else
        {
            Debug.LogWarning("No blend shapes preset. Procedural emotions won't be initialized");
            return;
        }
    }

    // Function to copy the values from the blend shapes mapper added to the VHP manager.
    private void CopyBlendshapesMappersValues(List<float> blendShapesMapperValues, List<float> emotionBlendShapeValues)
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

    // Function that set the current emotions blenshapes intensity of the character if any change is asked in the emotions values. 
    private void SetCurrentEmotionBlendShapesIntensity()
    {
        // Public and private emotions blenshapes intensity values are compared to detect any modification asked thanks to the public variables.
        float[] currentEmotionsIntensityValues = { m_anger, m_disgust, m_fear, m_happiness, m_sadness, m_surprise };
        float[] neededEmotionsIntensityValues = { anger, disgust, fear, happiness, sadness, surprise };

        for (int i = 0; i < currentEmotionsIntensityValues.Length; i++)
        {
            // If a new emotion intensity value is detected and is significantly different from the previous one, the current emotion state is updated.
            if (neededEmotionsIntensityValues[i] > currentEmotionsIntensityValues[i] + 1 || neededEmotionsIntensityValues[i] < currentEmotionsIntensityValues[i] - 1)
            {
                currentEmotionsIntensityValues[i] = neededEmotionsIntensityValues[i];

                m_currentEmotion = (Emotions)i;

                // All other intensity values are set to 0 as they are mutually exclusive.
                for (int j = 0; j < currentEmotionsIntensityValues.Length; j++)
                    if (j != (int)m_currentEmotion)
                        currentEmotionsIntensityValues[j] = 0;

                m_anger = currentEmotionsIntensityValues[0];
                m_disgust = currentEmotionsIntensityValues[1];
                m_fear = currentEmotionsIntensityValues[2];
                m_happiness = currentEmotionsIntensityValues[3];
                m_sadness = currentEmotionsIntensityValues[4];
                m_surprise = currentEmotionsIntensityValues[5];

                // Then, public and private intensity values are synchronized and the function to update the emotions' blend shape values is called.
                SyncEmotionsIntensityValues();
                UpdateEmotionBlendShapeValues();

                break;
            }
        }
    }

    // Function to synchronize the public intensity variables with the private ones. 
    private void SyncEmotionsIntensityValues()
    {
        anger = m_anger;
        disgust = m_disgust;
        fear = m_fear;
        happiness = m_happiness;
        sadness = m_sadness;
        surprise = m_surprise;
    }

    #endregion

    #region Updating emotions blenshapes values

    // Function to update the current emotion blend shape values and to trigger the event to allow the VHP manager to update the character's blend shapes with these new emotion's values.
    private void UpdateEmotionBlendShapeValues()
    {
        float[] currentEmotionBlendShapeValues = new float[m_VHPmanager.TotalCharacterBlendShapes];

        // A function to scale the emotion blend shape values based on the current emotion intensity is called.
        // The final values are added to the current emotion values list.
        switch (m_currentEmotion)
        {
            case Emotions.ANGER:
                EmotionsValuesScalling(m_anger, m_angerBlendShapeValues, currentEmotionBlendShapeValues);
                break;
            case Emotions.DISGUST:
                EmotionsValuesScalling(m_disgust, m_disgustBlendShapeValues, currentEmotionBlendShapeValues);
                break;
            case Emotions.FEAR:
                EmotionsValuesScalling(m_fear, m_fearBlendShapeValues, currentEmotionBlendShapeValues);
                break;
            case Emotions.HAPPINESS:
                EmotionsValuesScalling(m_happiness, m_happinessBlendShapeValues, currentEmotionBlendShapeValues);
                break;
            case Emotions.SADNESS:
                EmotionsValuesScalling(m_sadness, m_sadnessBlendShapeValues, currentEmotionBlendShapeValues);
                break;
            case Emotions.SURPRISE:
                EmotionsValuesScalling(m_surprise, m_surpriseBlendShapeValues, currentEmotionBlendShapeValues);
                break;
            case Emotions.NONE:
                break;
        }

        // If any function subscribed to the emotions change event, the associated delegate is invoked with the list of the current emotion blend shape values as parameter.
        OnEmotionsChange?.Invoke(currentEmotionBlendShapeValues);
    }

    // Function to scale the emotion blend shape values depending on the emotion blenshapes intensity and to add them to the current emotion blend shape values list.
    private void EmotionsValuesScalling(float currentEmotionIntensityValue, List<float> emotionMaxBlendShapeValues, float[] currentEmotionBlendShapeValues)
    {
        for (int i = 0; i < emotionMaxBlendShapeValues.Count; i++)
            currentEmotionBlendShapeValues[i] = (Mathf.Clamp((currentEmotionIntensityValue * emotionMaxBlendShapeValues[i]) / 100, 0f, 100f));
    }

    #endregion
}
