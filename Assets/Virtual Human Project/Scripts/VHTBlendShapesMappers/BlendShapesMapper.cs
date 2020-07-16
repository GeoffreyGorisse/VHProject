/********************************************************************
Filename    :   BlendShapesMapper.cs
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

[CreateAssetMenu(menuName = "Virtual Human Project/Blend Shapes Mapper")]
public class BlendShapesMapper : ScriptableObject
{
    // Enumeration of all facial expressions to set to enable virtual humans procedural animations.
    public enum FacialExpression
    {
        ANGER,
        DISGUST,
        FEAR,
        HAPPINESS,
        SADNESS,
        SURPRISE,
        BLINK,
        GAZEUP,
        GAZEDOWN,
        VISEME_sil,
        VISEME_PP,
        VISEME_FF,
        VISEME_TH,
        VISEME_DD,
        VISEME_kk,
        VISEME_CH,
        VISEME_SS,
        VISEME_nn,
        VISEME_RR,
        VISEME_aa,
        VISEME_E,
        VISEME_I,
        VISEME_O,
        VISEME_U,
        DEFAULT
    }

    // Lists to contain the saved blend shape values for each facial expression.
    // Lists are serialazable to display the float values of the saved blend shapes mappers in the inspector. 
    [Header("Emotions:")]
    [SerializeField] private List<float> m_angerBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_disgustBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_fearBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_happinessBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_sadnessBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_surpriseBlendShapeValues = new List<float>();

    [Header("Gaze:")]
    [SerializeField] private List<float> m_blinkBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_gazeUpBlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_gazeDownBlendShapeValues = new List<float>();

    [Header("Visems:")]
    [SerializeField] private List<float> m_viseme_sil_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_PP_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_FF_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_TH_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_DD_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_kk_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_CH_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_SS_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_nn_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_RR_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_aa_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_E_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_I_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_O_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> m_viseme_U_BlendShapeValues = new List<float>();

    // Function returning true if the list corresponding to the facial pose to be edited contains blend shape values to display a warning message in the blenshapes mapper editor when saving data.
    public List<float> GetBlenShapeValues(FacialExpression facialPoseToEdit)
    {
        switch (facialPoseToEdit)
        {
            case FacialExpression.ANGER:
                return m_angerBlendShapeValues;
            case FacialExpression.DISGUST:
                return m_disgustBlendShapeValues;
            case FacialExpression.FEAR:
                return m_fearBlendShapeValues;
            case FacialExpression.HAPPINESS:
                return m_happinessBlendShapeValues;
            case FacialExpression.SADNESS:
                return m_sadnessBlendShapeValues;
            case FacialExpression.SURPRISE:
                return m_surpriseBlendShapeValues;
            case FacialExpression.BLINK:
                return m_blinkBlendShapeValues;
            case FacialExpression.GAZEUP:
                return m_gazeUpBlendShapeValues;
            case FacialExpression.GAZEDOWN:
                return m_gazeDownBlendShapeValues;
            case FacialExpression.VISEME_sil:
                return m_viseme_sil_BlendShapeValues;
            case FacialExpression.VISEME_PP:
                return m_viseme_PP_BlendShapeValues;
            case FacialExpression.VISEME_FF:
                return m_viseme_FF_BlendShapeValues;
            case FacialExpression.VISEME_TH:
                return m_viseme_TH_BlendShapeValues;
            case FacialExpression.VISEME_DD:
                return m_viseme_DD_BlendShapeValues;
            case FacialExpression.VISEME_kk:
                return m_viseme_kk_BlendShapeValues;
            case FacialExpression.VISEME_CH:
                return m_viseme_CH_BlendShapeValues;
            case FacialExpression.VISEME_SS:
                return m_viseme_SS_BlendShapeValues;
            case FacialExpression.VISEME_nn:
                return m_viseme_nn_BlendShapeValues;
            case FacialExpression.VISEME_RR:
                return m_viseme_RR_BlendShapeValues;
            case FacialExpression.VISEME_aa:
                return m_viseme_aa_BlendShapeValues;
            case FacialExpression.VISEME_E:
                return m_viseme_E_BlendShapeValues;
            case FacialExpression.VISEME_I:
                return m_viseme_I_BlendShapeValues;
            case FacialExpression.VISEME_O:
                return m_viseme_O_BlendShapeValues;
            case FacialExpression.VISEME_U:
                return m_viseme_U_BlendShapeValues;
            default:
                return null;
        }
    }

    // Function allowing to save the blend shape values for each facial expression from the blend shapes mapper editor.
    public void SetBlendShapeValues(FacialExpression facialPoseToEdit, List<float> skinnedMeshBlendShapeValues)
    {
        switch (facialPoseToEdit)
        {
            case FacialExpression.ANGER:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_angerBlendShapeValues);
                break;
            case FacialExpression.DISGUST:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_disgustBlendShapeValues);
                break;
            case FacialExpression.FEAR:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_fearBlendShapeValues);
                break;
            case FacialExpression.HAPPINESS:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_happinessBlendShapeValues);
                break;
            case FacialExpression.SADNESS:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_sadnessBlendShapeValues);
                break;
            case FacialExpression.SURPRISE:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_surpriseBlendShapeValues);
                break;
            case FacialExpression.BLINK:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_blinkBlendShapeValues);
                break;
            case FacialExpression.GAZEUP:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_gazeUpBlendShapeValues);
                break;
            case FacialExpression.GAZEDOWN:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_gazeDownBlendShapeValues);
                break;
            case FacialExpression.VISEME_sil:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_sil_BlendShapeValues);
                break;
            case FacialExpression.VISEME_PP:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_PP_BlendShapeValues);
                break;
            case FacialExpression.VISEME_FF:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_FF_BlendShapeValues);
                break;
            case FacialExpression.VISEME_TH:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_TH_BlendShapeValues);
                break;
            case FacialExpression.VISEME_DD:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_DD_BlendShapeValues);
                break;
            case FacialExpression.VISEME_kk:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_kk_BlendShapeValues);
                break;
            case FacialExpression.VISEME_CH:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_CH_BlendShapeValues);
                break;
            case FacialExpression.VISEME_SS:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_SS_BlendShapeValues);
                break;
            case FacialExpression.VISEME_nn:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_nn_BlendShapeValues);
                break;
            case FacialExpression.VISEME_RR:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_RR_BlendShapeValues);
                break;
            case FacialExpression.VISEME_aa:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_aa_BlendShapeValues);
                break;
            case FacialExpression.VISEME_E:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_E_BlendShapeValues);
                break;
            case FacialExpression.VISEME_I:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_I_BlendShapeValues);
                break;
            case FacialExpression.VISEME_O:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_O_BlendShapeValues);
                break;
            case FacialExpression.VISEME_U:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, m_viseme_U_BlendShapeValues);
                break;
            default:
                break;
        }
    }

    // Function adding the current blenshapes values of the character's skinned mesh renderer in the scene to a list.
    private void AddBlendShapeValues(List<float> skinnedMeshBlendShapeValues, List<float> blendShapeValues)
    {
        // Current values of the list are deleted to add another set of data.
        if (blendShapeValues.Any())
            blendShapeValues.Clear();

        // New values are added to the list.
        foreach (float value in skinnedMeshBlendShapeValues)
            blendShapeValues.Add(value);
    }
}


