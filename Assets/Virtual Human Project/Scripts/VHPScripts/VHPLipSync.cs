/********************************************************************
Filename    :   VHPLipSync.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(VHPManager), typeof(OVRLipSyncContext))]
public class VHPLipSync : MonoBehaviour
{
    public enum LipSyncMode
    {
        REALTIME,
        PRERECORDED
    }

    // Delegate and event allowing the VHP manager to subscribe a function that updates the character's blend shapes with new lip values.
    public delegate void OnLipChangeDelegate(float[] currentLipBlendShapeValues);
    public event OnLipChangeDelegate OnLipChange;

    [Header("Lip sync settings")]

    [SerializeField, Tooltip("Enables audio processing. Should be disabled when lip synchronization is not active.")]
    private bool _audioProcessing = true;
    [SerializeField, Range(1, 100), Tooltip("Lip synchronization smoothing value.")]
    private int _smoothing = 50;
    [SerializeField, Tooltip("Lip synchronization mode: 'realtime' (based on microphone input) or 'pre-recorded' (based on audio source clip).")]
    private LipSyncMode _lipSyncMode;

    private List<float> _visemesIntensityValues = new List<float>();
    private List<float> _viseme_sil_BlendShapeValues = new List<float>();
    private List<float> _viseme_PP_BlendShapeValues = new List<float>();
    private List<float> _viseme_FF_BlendShapeValues = new List<float>();
    private List<float> _viseme_TH_BlendShapeValues = new List<float>();
    private List<float> _viseme_DD_BlendShapeValues = new List<float>();
    private List<float> _viseme_kk_BlendShapeValues = new List<float>();
    private List<float> _viseme_CH_BlendShapeValues = new List<float>();
    private List<float> _viseme_SS_BlendShapeValues = new List<float>();
    private List<float> _viseme_nn_BlendShapeValues = new List<float>();
    private List<float> _viseme_RR_BlendShapeValues = new List<float>();
    private List<float> _viseme_aa_BlendShapeValues = new List<float>();
    private List<float> _viseme_E_BlendShapeValues = new List<float>();
    private List<float> _viseme_I_BlendShapeValues = new List<float>();
    private List<float> _viseme_O_BlendShapeValues = new List<float>();
    private List<float> _viseme_U_BlendShapeValues = new List<float>();

    private List<List<float>> _visemesBlendShapeValues = new List<List<float>>();

    private VHPManager _VHPmanager;
    private OVRLipSyncContext _OVRLipSyncContext;
    private OVRLipSync.Frame _OVRLipSyncFrame;
    private OVRLipSyncMicInput _OVRLipSyncMicInput;

    private void Awake()
    {
        _VHPmanager = gameObject.GetComponent<VHPManager>();
        _OVRLipSyncContext = transform.GetComponent<OVRLipSyncContext>();

        LoadBlendShapeValues();
 
        _visemesBlendShapeValues.Add(_viseme_sil_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_PP_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_FF_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_TH_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_DD_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_kk_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_CH_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_SS_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_nn_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_RR_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_aa_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_E_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_I_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_O_BlendShapeValues);
        _visemesBlendShapeValues.Add(_viseme_U_BlendShapeValues);
    }

    private void OnEnable()
    {
        _OVRLipSyncFrame = _OVRLipSyncContext.GetCurrentPhonemeFrame();

        if (_visemesIntensityValues.Any())
            _visemesIntensityValues.Clear();

        for (int i = 0; i < _OVRLipSyncFrame.Visemes.Length; i++)
            _visemesIntensityValues.Add(_OVRLipSyncFrame.Visemes[i]);
    }
    private void OnDisable()
    {
        if (_visemesIntensityValues.Any())
            _visemesIntensityValues.Clear();
    }

    void Update()
    {
        if (_audioProcessing)
        {
            if (!_OVRLipSyncContext.enabled)
                _OVRLipSyncContext.enabled = true;

            if (_OVRLipSyncContext.Smoothing != _smoothing)
                _OVRLipSyncContext.Smoothing = _smoothing;

            // Adds the necessary components based on the lip synchronization mode ('realtime' or 'pre-recorded').
            if (_lipSyncMode == LipSyncMode.REALTIME && (!_OVRLipSyncMicInput || !_OVRLipSyncMicInput.enabled))
            {
                if (!_OVRLipSyncMicInput)
                    _OVRLipSyncMicInput = gameObject.AddComponent<OVRLipSyncMicInput>();

                _OVRLipSyncMicInput.enabled = true;
            }

            else if (_lipSyncMode == LipSyncMode.PRERECORDED && _OVRLipSyncMicInput && _OVRLipSyncMicInput.enabled)
                _OVRLipSyncMicInput.enabled = false;

            DetectDifferencesInVisemesIntensityValues();
        }

        else
        {
            if (_OVRLipSyncContext.enabled)
                _OVRLipSyncContext.enabled = false;

            if (_OVRLipSyncMicInput && _OVRLipSyncMicInput.enabled)
                _OVRLipSyncMicInput.enabled = false;
        }
    }

    #region Blend shape values initilization

    // Function to load the visemes max values from the blend shapes mapper added to the VHP manager.
    private void LoadBlendShapeValues()
    {
        // Loading the blend shapes mapper values to be used for lip synchronization if a mapper preset is added to the VHP manager.
        if (_VHPmanager.blendShapesMapperPreset)
        {
            BlendShapesMapper blendShapesMapper = _VHPmanager.blendShapesMapperPreset;

            // Calling the function to copy the values from the blend shapes mapper added to the VHP manager.
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_sil), _viseme_sil_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_PP), _viseme_PP_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_FF), _viseme_FF_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_TH), _viseme_TH_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_DD), _viseme_DD_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_kk), _viseme_kk_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_CH), _viseme_CH_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_SS), _viseme_SS_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_nn), _viseme_nn_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_RR), _viseme_RR_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_aa), _viseme_aa_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_E), _viseme_E_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_I), _viseme_I_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_O), _viseme_O_BlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.VISEME_U), _viseme_U_BlendShapeValues);
        }

        // Displaying a warning message if no blend shapes mapper is added to the VHP manager.
        else
        {
            Debug.LogWarning("No blend shapes mapper preset. Lip synchronization won't be initialized");
            return;
        }
    }

    // Function to copy the values from the blend shapes mapper added to the VHP manager.
    private void CopyBlendshapesMappersValues(List<float> blendShapesMapperValues, List<float> visemesBlendShapeValues)
    {
        for (int i = 0; i < blendShapesMapperValues.Count; i++)
        {
            if (blendShapesMapperValues[i] >= 0 && blendShapesMapperValues[i] <= 100)
                visemesBlendShapeValues.Add(blendShapesMapperValues[i]);

            else
                visemesBlendShapeValues.Add(0);
        }
    }

    #endregion

    #region Setting lip blenshapes intensity

    // Detects differences in the current visemes intensity values.
    private void DetectDifferencesInVisemesIntensityValues()
    {
        _OVRLipSyncFrame = _OVRLipSyncContext.GetCurrentPhonemeFrame();

        for (int i = 0; i < _OVRLipSyncFrame.Visemes.Length; i++)
        {
            // Detects a significant difference in the requested visemes intensities to update the blend shape values.
            if (_OVRLipSyncFrame.Visemes[i] > _visemesIntensityValues[i] + 0.05f || _OVRLipSyncFrame.Visemes[i] < _visemesIntensityValues[i] - 0.05f)
            {
                UpdateLipBlendShapeValues();
                break;
            }
        }
    }

    // Updates the current lip blend shape values and triggers the event to allow the VHP manager to update the character's blend shapes with the new lip values.
    private void UpdateLipBlendShapeValues()
    {
        float[] currentLipBlendShapeValues = new float[_VHPmanager.TotalCharacterBlendShapes];

        // For each viseme, if its value is greater than 0, the corresponding blend shape values from the preset are added to the current lip sync values list.
        // Since multiple visemes can be active simultaneously, blend shape values are accumulated (empirically tested, but could be improved with more advanced filtering methods).
        for (int i = 0; i < _OVRLipSyncFrame.Visemes.Length; i++)
        {
            if(_OVRLipSyncFrame.Visemes[i] > 0)
            {
                for (int j = 0; j < _VHPmanager.TotalCharacterBlendShapes; j++)
                {
                    if (i == 0)
                        currentLipBlendShapeValues[j] = 0;

                    currentLipBlendShapeValues[j] += (_OVRLipSyncFrame.Visemes[i] * _visemesBlendShapeValues[i][j]);
                    currentLipBlendShapeValues[j] = Mathf.Clamp(currentLipBlendShapeValues[j], 0f, 100f);
                }
            }

            _visemesIntensityValues[i] = _OVRLipSyncFrame.Visemes[i];
        }

        // If any function is subscribed to the lip change event, the associated delegate is invoked with the current lip blend shape values as parameter.
        OnLipChange?.Invoke(currentLipBlendShapeValues);
    }

    #endregion
}