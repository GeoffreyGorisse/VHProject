/********************************************************************
Filename    :   VHPGaze.cs
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

[RequireComponent(typeof(VHPManager), typeof(Animator))]
public class VHPGaze : MonoBehaviour
{
    public enum GazeBehavior
    {
        PROBABILISTIC,
        RANDOM,
        STATIC,
        SCRIPTED,
        NONE
    }

    public enum GazeBlendShapesBehavior
    {
        BLINK,
        GAZEUP,
        GAZEDOWN,
        NONE
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Header("Debug")]

    [Tooltip("Enable to display the gaze direction in the scene view.")]
    public bool drawGazeDirection = false;

    [Header("Character properties")]

    [Tooltip("Head bone.")]
    public Transform headBone;
    [Tooltip("Left eye bone.")]
    public Transform leftEyeBone;
    [Tooltip("Right eye bone.")]
    public Transform rightEyeBone;
    [Tooltip("Select the bones' forward axis of the eyes.")]
    public Axis eyesForwardAxis;
    [Tooltip("Enable to invert the bones forward axes.")]
    public bool invertEyesAxisSense = false;

    [Header("Gaze settings")]

    [Tooltip("Select the behavior model of the eyes.")]
    public GazeBehavior gazeBehavior;
    [HideInInspector]
    public GameObject interestFieldPrefab;
    [HideInInspector]
    public bool agentMode = false;
    [Tooltip("Enable to add random micro variations to the gaze direction.")]
    public bool enableGazeMicroVariations = true;
    [Tooltip("Enable to add blinking.")]
    public bool enableBlinking = true;

    // Property to access the central point between the character's eyes.
    public Vector3 EyesAveragePosition { get { return m_eyesAveragePosition; } }
    // Property to access the initial target position.
    public Vector3 NeutralTargetPosition { get { return m_neutralTarget.transform.position; } }

    // Lists to copy the max values for the each gaze direction from the blend shapes preset added to the VHP manager.
    private List<float> m_blinkBlendShapeValues = new List<float>();
    private List<float> m_gazeUpBlendShapeValues = new List<float>();
    private List<float> m_gazeDownBlendShapeValues = new List<float>();

    // Private values to set the gaze direction intensity.
    [Range(0, 100)] private float m_blink;
    [Range(0, 100)] private float m_gazeUp;
    [Range(0, 100)] private float m_gazeDown;

    // Public gaze values to allow other script to access them and to expose the sliders in the inspector if needed.
    [HideInInspector, Range(0, 100)] public float blink = 0f;
    [HideInInspector, Range(0, 100)] public float gazeUp = 0f;
    [HideInInspector, Range(0, 100)] public float gazeDown = 0f;

    // Delegate and event allowing the VHP manager to subscribe with a function that updates the character's blend shapes with the new gaze values as soon as they get updated.
    public delegate void OnGazeChangeDelegate(float[] currentGazeBlendShapeValues);
    public event OnGazeChangeDelegate OnGazeChange;

    private VHPManager m_VHPmanager;
    private bool m_enableEyesProceduralAnimation = false;

    private Vector3 m_eyesAveragePosition;
    private GameObject m_gazeSubObjectsParent;
    private GameObject m_neutralTarget;
    private GameObject m_target;
    private Vector3 m_targetPosition;
    private GameObject m_interestFieldInstance;
    private bool m_interestFieldLoaded = false;
    private bool m_characterModeSet = false;
    private Animator m_animator;

    // Current gaze blenshapes behavior of the character.
    private GazeBlendShapesBehavior m_currentGazeBlendShapesBehavior = GazeBlendShapesBehavior.NONE;
    // Rotation to correct the eyes forward axis direction depending on their bones' foward axis.
    private Quaternion m_eyeForwardAxisRotationCorrection;

    private float m_currentMicroVariationWaitingTime = 0f;
    private bool m_lookingAtRandomPoint = false;

    private float m_currentBlinkingWaitingTime = 0f;
    private bool m_isBlinking = false;

    private void Awake()
    {
        m_VHPmanager = gameObject.GetComponent<VHPManager>();

        if (headBone && leftEyeBone && rightEyeBone)
            m_enableEyesProceduralAnimation = true;

        else
            Debug.LogWarning("Missing bone(s)' transform(s). Please assign the character's bones in the public fields to enable the procedural gaze system.");

        LoadBlendshapesValues();
        InstantiateGazeComponents();
    }

    private void OnEnable()
    {
        if (m_enableEyesProceduralAnimation)
        {
            SyncGazeIntensityValues();
            InstantiateGazeTarget();
            
            // Assignation of the correct forward axis direction of the character's eyes.
            int eyesForwardAxisSense = 1;

            if (invertEyesAxisSense)
                eyesForwardAxisSense = -1;

            switch (eyesForwardAxis)
            {
                case Axis.X:
                    m_eyeForwardAxisRotationCorrection = Quaternion.Euler(new Vector3(0, 90 * eyesForwardAxisSense, 0));
                    break;
                case Axis.Y:
                    m_eyeForwardAxisRotationCorrection = Quaternion.Euler(new Vector3(90 * eyesForwardAxisSense, 0, 0));
                    break;
                case Axis.Z:
                    if (invertEyesAxisSense)
                        m_eyeForwardAxisRotationCorrection = Quaternion.Euler(new Vector3(0, 0, eyesForwardAxisSense));
                    else
                        m_eyeForwardAxisRotationCorrection = Quaternion.Euler(Vector3.zero);
                    break;
                default:
                    break;
            }
        }
    }

    // Late update loop is used to override the bones animations to affect the eyes orientation procedurally.
    private void LateUpdate()
    {
        m_eyesAveragePosition = Vector3.Lerp(leftEyeBone.position, rightEyeBone.position, 0.5f);

        m_gazeSubObjectsParent.transform.position = m_eyesAveragePosition;
        m_gazeSubObjectsParent.transform.rotation = headBone.rotation;

        if (gazeBehavior == GazeBehavior.PROBABILISTIC)
        {
            if(m_interestFieldLoaded && agentMode)
                m_interestFieldInstance.transform.rotation = transform.rotation;

            else if(!m_interestFieldLoaded)                
                SetProbabilisticGazeInterestField();
        }

        if (m_interestFieldLoaded && (gazeBehavior != GazeBehavior.PROBABILISTIC || agentMode != m_characterModeSet))
        {
            Destroy(m_interestFieldInstance);
            m_interestFieldLoaded = false;
        }

        CalculateGazeDirection();
        CalculateGazeBlendShapesIntensity();
        if (enableBlinking)
            CalculateBlinkingBlendShapesIntensity();

        SetCurrentGazeBlendShapesIntensity();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        m_blink = 0f;
        m_gazeUp = 0f;
        m_gazeDown = 0f;

        SyncGazeIntensityValues();

        Destroy(m_target);

        if (m_interestFieldInstance)
        {
            Destroy(m_interestFieldInstance);
            m_interestFieldLoaded = false;
        }
    }

    #region Loading blend shape values

    // Function to load the gaze max values from the blend shapes mapper added to the VHP manager.
    private void LoadBlendshapesValues()
    {
        // Loading the blendshapes mapper values to be used for procedural gaze if a mapper preset is added to the VHP manager.
        if (m_VHPmanager.blendShapesMapperPreset)
        {
            BlendShapesMapper blendShapesMapper = m_VHPmanager.blendShapesMapperPreset;

            // Calling the function to copy the values from the blendshapes mapper added to the VHP manager.
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.BLINK), m_blinkBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.GAZEUP), m_gazeUpBlendShapeValues);
            CopyBlendshapesMappersValues(blendShapesMapper.GetBlenShapeValues(BlendShapesMapper.FacialExpression.GAZEDOWN), m_gazeDownBlendShapeValues);
        }

        // Displaying a warning message if no blendshapes mapper is added to the VHP manager.
        else
        {
            Debug.LogWarning("No blend shapes preset. Procedural gaze won't be initialized");
            return;
        }
    }

    // Function to copy the values from the blend shapes mapper added to the VHP manager.
    private void CopyBlendshapesMappersValues(List<float> blendShapesMapperValues, List<float> gazeBlendShapeValues)
    {
        for (int i = 0; i < blendShapesMapperValues.Count; i++)
        {
            if (blendShapesMapperValues[i] >= 0 && blendShapesMapperValues[i] <= 100)
                gazeBlendShapeValues.Add(blendShapesMapperValues[i]);

            else
                gazeBlendShapeValues.Add(0);
        }
    }

    #endregion

    #region Initializing gaze components

    // Function to instiantiate the game objects required to compute the gaze direction and its blend shape values.
    private void InstantiateGazeComponents()
    {
        // Eyes average position calculation to set the target position.
        m_eyesAveragePosition = Vector3.Lerp(leftEyeBone.position, rightEyeBone.position, 0.5f);

        // Instantiation of a parent object to contains the targets.
        m_gazeSubObjectsParent = new GameObject("Gaze_Sub_Objects");
        m_gazeSubObjectsParent.transform.position = m_eyesAveragePosition;
        m_gazeSubObjectsParent.transform.rotation = headBone.rotation;
        m_gazeSubObjectsParent.transform.parent = transform;

        // Instantiation of a target corresponding to the neutral gaze direction of the character depending on the eyes' forward axis.
        // This target is used to compute the relative angle between the current gaze direction and the neutral one.
        m_neutralTarget = new GameObject("Neutral_Target");
        //m_neutralTarget.hideFlags = HideFlags.HideInHierarchy;
        m_neutralTarget.transform.parent = m_gazeSubObjectsParent.transform;
        m_neutralTarget.transform.position = m_gazeSubObjectsParent.transform.position;
        m_neutralTarget.transform.rotation = leftEyeBone.transform.rotation;

        int eyesForwardAxisSense = 1;

        if (invertEyesAxisSense)
            eyesForwardAxisSense = -1;

        switch (eyesForwardAxis)
        {
            case Axis.X:
                m_neutralTarget.transform.Translate(eyesForwardAxisSense, 0, 0);
                break;
            case Axis.Y:
                m_neutralTarget.transform.Translate(0, eyesForwardAxisSense, 0);
                break;
            case Axis.Z:
                m_neutralTarget.transform.Translate(0, 0, eyesForwardAxisSense);
                break;
            default:
                break;
        }
    }

    // Function to instiantiate the eyes' target.
    private void InstantiateGazeTarget()
    {
        // Instanciation of the eyes' target with its script to set and control the gaze model.
        m_target = new GameObject("Eyes_Target");
        m_target.AddComponent<VHPGazeTarget>();
        m_target.GetComponent<VHPGazeTarget>().VHPGaze = this;
        m_target.transform.parent = m_gazeSubObjectsParent.transform;
        m_target.transform.position = m_neutralTarget.transform.position;

        m_targetPosition = m_target.transform.position;
    }

    // Function to instiantiate the interest field prefab required to detect the potential targets for the probabilistic gaze model in the scene.
    private void SetProbabilisticGazeInterestField()
    {
        if (interestFieldPrefab)
        {
            m_interestFieldInstance = Instantiate(interestFieldPrefab);
            m_interestFieldInstance.name = "Interest_Field";
            m_interestFieldInstance.transform.parent = m_gazeSubObjectsParent.transform;
            m_interestFieldInstance.transform.position = m_gazeSubObjectsParent.transform.position;

            if (!agentMode)
                m_interestFieldInstance.transform.rotation = transform.parent.rotation;

            else
                m_animator = transform.GetComponent<Animator>();

            // Interest field configuration based on the selected mode (avatar/agent).
            m_interestFieldInstance.transform.GetComponent<MeshCollider>().enabled = !agentMode;
            m_interestFieldInstance.transform.GetComponent<SphereCollider>().enabled = agentMode;

            m_target.GetComponent<VHPGazeTarget>().VHPGazeInterestField = m_interestFieldInstance.GetComponent<VHPGazeInterestField>();

            m_characterModeSet = agentMode;
            m_interestFieldLoaded = true;
        }

        else
            Debug.LogError("No interest field prefab assigned in the public field. The interest field prefab must be located in the following folder: Assets/Virtual Human Project/Prefabs/.");
    }

    #endregion

    #region Updating gaze direction and micro variations

    // Function to set the gaze direction.
    private void CalculateGazeDirection()
    {
        // When micro variations are enabled, a function to set a random target near the actuel eyes' target is called as soon as the delay is over.
        if (enableGazeMicroVariations)
        {
            if (m_currentMicroVariationWaitingTime >= Random.Range(1f, 3f) && !m_lookingAtRandomPoint)
            {
                m_targetPosition = GazeDirectionMicroVariations(m_target.transform.position);

                // A coroutine is called to control the micro variation duration.
                StartCoroutine(GazeDirectionMicroVariationDuration());
            }

            else if (!m_lookingAtRandomPoint)
                m_targetPosition = m_target.transform.position;

            m_currentMicroVariationWaitingTime += Time.deltaTime;
        }

        else
            m_targetPosition = m_target.transform.position;

        // Making the eyes looking at the target with a rotation correction based on the forward axes of the bones.
        leftEyeBone.rotation = Quaternion.LookRotation(m_targetPosition - leftEyeBone.position) * m_eyeForwardAxisRotationCorrection;
        rightEyeBone.rotation = Quaternion.LookRotation(m_targetPosition - rightEyeBone.position) * m_eyeForwardAxisRotationCorrection;

        // Draw the gaze direction in the scene view.
        if (drawGazeDirection)
        {
            Debug.DrawLine(leftEyeBone.transform.position, m_targetPosition, Color.cyan);
            Debug.DrawLine(rightEyeBone.transform.position, m_targetPosition, Color.cyan);
        }
    }

    // Function to set a random target near the actuel eyes' target for to create gaze micro variations.
    private Vector3 GazeDirectionMicroVariations(Vector3 currentEyesTargetPosition)
    {
        // Depending on the target distance, a maximum value for random positioning is determined.
        float targetDistance = Vector3.Distance(m_eyesAveragePosition, currentEyesTargetPosition);
        float maxRandomVariation = 0.1f * targetDistance;
        float variation = Random.Range(-maxRandomVariation, maxRandomVariation);

        // The random target position is determined based on the current target position and the calculated random variations.
        Vector3 randomTargetPosition = new Vector3(currentEyesTargetPosition.x + variation, currentEyesTargetPosition.y + variation, currentEyesTargetPosition.z + variation);

        return randomTargetPosition;
    }

    // Coroutine to control the micro gaze variation duration.
    private IEnumerator GazeDirectionMicroVariationDuration()
    {
        m_lookingAtRandomPoint = true;

        yield return new WaitForSeconds(Random.Range(0.2f, 0.3f));

        m_lookingAtRandomPoint = false;
        m_currentMicroVariationWaitingTime = 0;
    }

    #endregion

    #region Updating gaze's blend shapes intensity

    // Function to calculate the gaze blenshapes' intensity.
    private void CalculateGazeBlendShapesIntensity()
    {
        // Determining the current gaze direction vector and the initial forward direction vector.
        Vector3 gazeDirection = m_target.transform.position - m_eyesAveragePosition;
        Vector3 gazeInitialDirection = m_neutralTarget.transform.position - m_eyesAveragePosition;

        // Vertical plane normal calculation based on the vector between the character's eyes.
        Vector3 planeNormal = leftEyeBone.transform.position - rightEyeBone.transform.position;

        // Projection of the gaze direction and the initial forward direction on the vertical plane.
        Vector3 projectedGazeDirection = Vector3.ProjectOnPlane(gazeDirection, planeNormal);
        Vector3 projectedInitialGazeDirection = Vector3.ProjectOnPlane(gazeInitialDirection, planeNormal);

        // Angle calculation between the two directions to set the gaze blenshapes intensity value.
        float eyesVerticalAngle = Vector3.SignedAngle(projectedGazeDirection, projectedInitialGazeDirection, planeNormal);

        float blendShapeIntensityMultiplier = 15f;

        // Setting of the gaze blenshapes intensity value depending on the gaze direction.
        if (eyesVerticalAngle >= 0 && eyesVerticalAngle < 180)
            gazeDown = Mathf.Clamp(eyesVerticalAngle * blendShapeIntensityMultiplier, 0f, 100f);

        else if (eyesVerticalAngle < 0 && eyesVerticalAngle >= -180)
            gazeUp = Mathf.Clamp((eyesVerticalAngle * -1) * blendShapeIntensityMultiplier, 0f, 100f);
    }

    // Function to set the blinking blenshapes intensity.
    private void CalculateBlinkingBlendShapesIntensity()
    {
        if (m_currentBlinkingWaitingTime >= Random.Range(3f, 8f) && !m_isBlinking)
        {
            m_isBlinking = true;
            blink = 100f;

            StartCoroutine(BlinkingDuration(0.1f));

            m_currentBlinkingWaitingTime = 0;
        }

        m_currentBlinkingWaitingTime += Time.deltaTime;
    }

    // Coroutine to control the blinking duration.
    private IEnumerator BlinkingDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        blink = 0f;
        m_isBlinking = false;
    }

    // Function that set the current gaze blenshapes intensity values of the character if any change is asked in the gaze values. 
    private void SetCurrentGazeBlendShapesIntensity()
    {
        // Public and private gaze intensity values are compared to detect any modification asked thanks to the public variables.
        float[] currentGazeIntensityValues = { m_blink, m_gazeUp, m_gazeDown };
        float[] neededGazeIntensityValues = { blink, gazeUp, gazeDown };

        for (int i = 0; i < currentGazeIntensityValues.Length; i++)
        {
            // If a new gaze intensity value is detected and is significantly different from the previous one, the current gaze state is updated.
            if (neededGazeIntensityValues[i] > currentGazeIntensityValues[i] + 10 || neededGazeIntensityValues[i] < currentGazeIntensityValues[i] - 10)
            {
                currentGazeIntensityValues[i] = neededGazeIntensityValues[i];

                m_currentGazeBlendShapesBehavior = (GazeBlendShapesBehavior)i;

                // Except the blinking intensity and the current gaze intensity, all other intensity values are set to 0 as they are mutually exclusive.
                for (int j = 0; j < currentGazeIntensityValues.Length; j++)
                    if (j != 0 && j != (int)m_currentGazeBlendShapesBehavior)
                        currentGazeIntensityValues[j] = 0;

                m_blink = currentGazeIntensityValues[0];
                m_gazeUp = currentGazeIntensityValues[1];
                m_gazeDown = currentGazeIntensityValues[2];

                // Public and private intensity values are synchronized and the function to update the gaze blendshapes values is called.     
                SyncGazeIntensityValues();
                UpdateGazeBlendShapeValues();

                break;
            }
        }
    }

    // Function to synchronize the public intensity variables with the private ones. 
    private void SyncGazeIntensityValues()
    {
        blink = m_blink;
        gazeUp = m_gazeUp;
        gazeDown = m_gazeDown;
    }

    #endregion

    #region Updating gaze's blend shape values

    // Function to update the current gaze's blend shape values and to trigger the event to allow the VHP manager to update the character's blend shapes with these new gaze's values.
    private void UpdateGazeBlendShapeValues()
    {
        float[] currentGazeBlendShapeValues = new float[m_VHPmanager.TotalCharacterBlendShapes];

        // Function to scale the gaze blendshapes values based on the current gaze intensity is called.
        // The final values are added to the current gaze values list.
        switch (m_currentGazeBlendShapesBehavior)
        {
            case GazeBlendShapesBehavior.GAZEUP:
                GazeValuesScalling(m_gazeUp, m_gazeUpBlendShapeValues, currentGazeBlendShapeValues);
                break;
            case GazeBlendShapesBehavior.GAZEDOWN:
                GazeValuesScalling(m_gazeDown, m_gazeDownBlendShapeValues, currentGazeBlendShapeValues);
                break;
        }

        // Blinking blenshapes values are added to the current gaze values list to blend the gaze direction blend shapes and the blinking blend shape values.
        for (int i = 0; i < currentGazeBlendShapeValues.Length; i++)
            currentGazeBlendShapeValues[i] = Mathf.Clamp(currentGazeBlendShapeValues[i] + (m_blink * m_blinkBlendShapeValues[i] / 100f), 0f, 100f);

        // If any function subscribed to the gaze change event, the associated delegate is invoked with the list of the current gaze blendshapes values as parameter.
        OnGazeChange?.Invoke(currentGazeBlendShapeValues);
    }

    // Function to scale the gaze's blend shapes values depending on the gaze blenshapes intensity and to add them to the current gaze blenshapes values list.
    private void GazeValuesScalling(float currentGazeIntensityValue, List<float> gazeMaxBlendShapeValues, float[] currentGazeBlendShapeValues)
    {
        for (int i = 0; i < gazeMaxBlendShapeValues.Count; i++)
            currentGazeBlendShapeValues[i] = Mathf.Clamp(currentGazeIntensityValue * gazeMaxBlendShapeValues[i] / 100, 0f, 100f);
    }

    #endregion

    #region Agent mode IK settings

    private void OnAnimatorIK()
    {
        if (m_animator && gazeBehavior == GazeBehavior.PROBABILISTIC && agentMode)
        {
            // Setting the weights (head and body) to rotate the character based on the target's location.
            // Note that we do not override the eyes' weight as it is already controlled by other functions to enable micro variations without rotating the character's body.
            // Moreover, eyes have to be controlled without the animator functions to allow for non rigged characters to be used (e.g. partial avatars in VR).
            m_animator.SetLookAtWeight(1f, 0.05f, 0.5f);
            m_animator.SetLookAtPosition(m_target.transform.position);
        }
    }

    #endregion
}