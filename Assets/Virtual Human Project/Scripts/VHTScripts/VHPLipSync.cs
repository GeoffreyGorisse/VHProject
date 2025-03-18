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

    [Header("Lip sync settings")]

    [Tooltip("Enable audio processing. Must be disabled when lip synchronization is not active.")]
    public bool audioProcessing = true;
    [Tooltip("Lip synchronisation smoothing value.")]
    [Range(1, 100)] public int smoothing = 50;
    [Tooltip("Lip synchronisation mode (realtime: based on mic input; pre-recorded: based on audio source clip).")]
    public LipSyncMode lipSyncMode;

    // Delegate and event allowing the VHP manager to subscribe with a function that updates the character's blend shapes with the new emotions values as soon as they get updated.
    public delegate void OnLipChangeDelegate(float[] currentLipBlendShapeValues);
    public event OnLipChangeDelegate OnLipChange;

    private List<float> _visemesIntensityValues = new List<float>();

    // Lists to copy the max values for each viseme from the blend shapes preset added to the VHP manager.
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

    private VHPManager m_VHPmanager;
    private OVRLipSyncContext m_OVRLipSyncContext;
    private OVRLipSync.Frame m_OVRLipSyncFrame;
    private OVRLipSyncMicInput m_OVRLipSyncMicInput;

    private void Awake()
    {
        m_VHPmanager = gameObject.GetComponent<VHPManager>();

        m_OVRLipSyncContext = transform.GetComponent<OVRLipSyncContext>();

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
        m_OVRLipSyncFrame = m_OVRLipSyncContext.GetCurrentPhonemeFrame();

        if (_visemesIntensityValues.Any())
            _visemesIntensityValues.Clear();

        for (int i = 0; i < m_OVRLipSyncFrame.Visemes.Length; i++)
            _visemesIntensityValues.Add(m_OVRLipSyncFrame.Visemes[i]);
    }

    void Update()
    {
        if (audioProcessing)
        {
            if (!m_OVRLipSyncContext.enabled)
                m_OVRLipSyncContext.enabled = true;

            if (m_OVRLipSyncContext.Smoothing != smoothing)
                m_OVRLipSyncContext.Smoothing = smoothing;

            // Adding the required components depending on the lip synchronization mode (realtime/pre-computed).
            if (lipSyncMode == LipSyncMode.REALTIME && (!m_OVRLipSyncMicInput || !m_OVRLipSyncMicInput.enabled))
            {
                if (!m_OVRLipSyncMicInput)
                    m_OVRLipSyncMicInput = gameObject.AddComponent<OVRLipSyncMicInput>();

                m_OVRLipSyncMicInput.enabled = true;
            }

            else if (lipSyncMode == LipSyncMode.PRERECORDED && m_OVRLipSyncMicInput && m_OVRLipSyncMicInput.enabled)
                m_OVRLipSyncMicInput.enabled = false;

            // Calling the function to detect the visemes' variations.
            DetectVisemesVariations();
        }

        else
        {
            if (m_OVRLipSyncContext.enabled)
                m_OVRLipSyncContext.enabled = false;

            if (m_OVRLipSyncMicInput && m_OVRLipSyncMicInput.enabled)
                m_OVRLipSyncMicInput.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (_visemesIntensityValues.Any())
            _visemesIntensityValues.Clear();
    }

    #region Loading blend shapes max values

    // Function to load the visemes max values from the blend shapes mapper added to the VHP manager.
    private void LoadBlendShapeValues()
    {
        // Loading the blend shapes mapper values to be used for lip synchronization if a mapper preset is added to the VHP manager.
        if (m_VHPmanager.blendShapesMapperPreset)
        {
            BlendShapesMapper blendShapesMapper = m_VHPmanager.blendShapesMapperPreset;

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

    #region Updating lip blenshapes values

    // Function to detect the visemes' variations.
    private void DetectVisemesVariations()
    {
        m_OVRLipSyncFrame = m_OVRLipSyncContext.GetCurrentPhonemeFrame();

        for (int i = 0; i < m_OVRLipSyncFrame.Visemes.Length; i++)
        {
            // If a new viseme intensity value is detected and is significantly different from the previous one, the function to update the lip blend shape values is called.
            if (m_OVRLipSyncFrame.Visemes[i] > _visemesIntensityValues[i] + 0.05f || m_OVRLipSyncFrame.Visemes[i] < _visemesIntensityValues[i] - 0.05f)
            {
                UpdateLipBlendShapeValues();
                break;
            }
        }
    }

    // Function to update the current lip blendshapes values and to trigger the event to allow the VHP manager to update the character's blendshapes.
    private void UpdateLipBlendShapeValues()
    {
        float[] currentLipBlendShapeValues = new float[m_VHPmanager.TotalCharacterBlendShapes];

        // For each viseme, if it's value is superior to 0, each blend shape value from the preset is added to the current lip values list.
        // As several visemes can be active at the same time, blend shape values must be cumulated.
        for (int i = 0; i < m_OVRLipSyncFrame.Visemes.Length; i++)
        {
            if(m_OVRLipSyncFrame.Visemes[i] > 0)
            {
                for (int j = 0; j < m_VHPmanager.TotalCharacterBlendShapes; j++)
                {
                    if (i == 0)
                        currentLipBlendShapeValues[j] = 0;

                    currentLipBlendShapeValues[j] += (m_OVRLipSyncFrame.Visemes[i] * _visemesBlendShapeValues[i][j]);
                    currentLipBlendShapeValues[j] = Mathf.Clamp(currentLipBlendShapeValues[j], 0f, 100f);
                }
            }

            _visemesIntensityValues[i] = m_OVRLipSyncFrame.Visemes[i];
        }

        // If any function subscribed to the lip change event, the associated delegate is invoked with the list of the current lip blend shape values as parameter.
        OnLipChange?.Invoke(currentLipBlendShapeValues);
    }

    #endregion
}
