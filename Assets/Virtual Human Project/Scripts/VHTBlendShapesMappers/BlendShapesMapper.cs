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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Virtual Human Project/Blend Shapes Mapper")]
public class BlendShapesMapper : ScriptableObject
{
    // Enumeration of facial expressions to enable procedural animations.
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

    // Lists to store the saved blend shape values for each facial expression.
    // Lists are serialized to display the float values of the blend shapes mappers in the inspector.
    [Header("Emotions:")]
    [SerializeField] private List<float> _angerBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _disgustBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _fearBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _happinessBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _sadnessBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _surpriseBlendShapeValues = new List<float>();

    [Header("Gaze:")]
    [SerializeField] private List<float> _blinkBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _gazeUpBlendShapeValues = new List<float>();
    [SerializeField] private List<float> _gazeDownBlendShapeValues = new List<float>();

    [Header("Visems:")]
    [SerializeField] private List<float> _viseme_sil_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_PP_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_FF_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_TH_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_DD_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_kk_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_CH_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_SS_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_nn_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_RR_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_aa_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_E_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_I_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_O_BlendShapeValues = new List<float>();
    [SerializeField] private List<float> _viseme_U_BlendShapeValues = new List<float>();

    // Returns true if the list corresponding to the facial pose being edited contains blend shape values.
    // Triggers a warning message in the blend shapes mapper editor when saving data.
    public List<float> GetBlenShapeValues(FacialExpression facialPoseToEdit)
    {
        switch (facialPoseToEdit)
        {
            case FacialExpression.ANGER:
                return _angerBlendShapeValues;
            case FacialExpression.DISGUST:
                return _disgustBlendShapeValues;
            case FacialExpression.FEAR:
                return _fearBlendShapeValues;
            case FacialExpression.HAPPINESS:
                return _happinessBlendShapeValues;
            case FacialExpression.SADNESS:
                return _sadnessBlendShapeValues;
            case FacialExpression.SURPRISE:
                return _surpriseBlendShapeValues;
            case FacialExpression.BLINK:
                return _blinkBlendShapeValues;
            case FacialExpression.GAZEUP:
                return _gazeUpBlendShapeValues;
            case FacialExpression.GAZEDOWN:
                return _gazeDownBlendShapeValues;
            case FacialExpression.VISEME_sil:
                return _viseme_sil_BlendShapeValues;
            case FacialExpression.VISEME_PP:
                return _viseme_PP_BlendShapeValues;
            case FacialExpression.VISEME_FF:
                return _viseme_FF_BlendShapeValues;
            case FacialExpression.VISEME_TH:
                return _viseme_TH_BlendShapeValues;
            case FacialExpression.VISEME_DD:
                return _viseme_DD_BlendShapeValues;
            case FacialExpression.VISEME_kk:
                return _viseme_kk_BlendShapeValues;
            case FacialExpression.VISEME_CH:
                return _viseme_CH_BlendShapeValues;
            case FacialExpression.VISEME_SS:
                return _viseme_SS_BlendShapeValues;
            case FacialExpression.VISEME_nn:
                return _viseme_nn_BlendShapeValues;
            case FacialExpression.VISEME_RR:
                return _viseme_RR_BlendShapeValues;
            case FacialExpression.VISEME_aa:
                return _viseme_aa_BlendShapeValues;
            case FacialExpression.VISEME_E:
                return _viseme_E_BlendShapeValues;
            case FacialExpression.VISEME_I:
                return _viseme_I_BlendShapeValues;
            case FacialExpression.VISEME_O:
                return _viseme_O_BlendShapeValues;
            case FacialExpression.VISEME_U:
                return _viseme_U_BlendShapeValues;
            default:
                return null;
        }
    }

    // Saves the blend shape values for each facial expression from the blend shapes mapper editor.
    public void SetBlendShapeValues(FacialExpression facialPoseToEdit, List<float> skinnedMeshBlendShapeValues)
    {
        switch (facialPoseToEdit)
        {
            case FacialExpression.ANGER:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _angerBlendShapeValues);
                break;
            case FacialExpression.DISGUST:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _disgustBlendShapeValues);
                break;
            case FacialExpression.FEAR:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _fearBlendShapeValues);
                break;
            case FacialExpression.HAPPINESS:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _happinessBlendShapeValues);
                break;
            case FacialExpression.SADNESS:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _sadnessBlendShapeValues);
                break;
            case FacialExpression.SURPRISE:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _surpriseBlendShapeValues);
                break;
            case FacialExpression.BLINK:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _blinkBlendShapeValues);
                break;
            case FacialExpression.GAZEUP:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _gazeUpBlendShapeValues);
                break;
            case FacialExpression.GAZEDOWN:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _gazeDownBlendShapeValues);
                break;
            case FacialExpression.VISEME_sil:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_sil_BlendShapeValues);
                break;
            case FacialExpression.VISEME_PP:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_PP_BlendShapeValues);
                break;
            case FacialExpression.VISEME_FF:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_FF_BlendShapeValues);
                break;
            case FacialExpression.VISEME_TH:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_TH_BlendShapeValues);
                break;
            case FacialExpression.VISEME_DD:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_DD_BlendShapeValues);
                break;
            case FacialExpression.VISEME_kk:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_kk_BlendShapeValues);
                break;
            case FacialExpression.VISEME_CH:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_CH_BlendShapeValues);
                break;
            case FacialExpression.VISEME_SS:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_SS_BlendShapeValues);
                break;
            case FacialExpression.VISEME_nn:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_nn_BlendShapeValues);
                break;
            case FacialExpression.VISEME_RR:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_RR_BlendShapeValues);
                break;
            case FacialExpression.VISEME_aa:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_aa_BlendShapeValues);
                break;
            case FacialExpression.VISEME_E:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_E_BlendShapeValues);
                break;
            case FacialExpression.VISEME_I:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_I_BlendShapeValues);
                break;
            case FacialExpression.VISEME_O:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_O_BlendShapeValues);
                break;
            case FacialExpression.VISEME_U:
                AddBlendShapeValues(skinnedMeshBlendShapeValues, _viseme_U_BlendShapeValues);
                break;
            default:
                break;
        }
    }

    // Adds the current blend shape values of the character's skinned mesh renderers (from the scene) to a list.
    private void AddBlendShapeValues(List<float> skinnedMeshBlendShapeValues, List<float> blendShapeValues)
    {
        if (blendShapeValues.Any())
            blendShapeValues.Clear();

        foreach (float value in skinnedMeshBlendShapeValues)
            blendShapeValues.Add(value);
    }
}