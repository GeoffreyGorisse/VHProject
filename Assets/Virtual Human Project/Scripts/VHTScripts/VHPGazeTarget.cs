/********************************************************************
Filename    :   VHPGazeTarget.cs
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

public class VHPGazeTarget : MonoBehaviour
{
    public VHPGaze VHPGaze { get; set; }
    public VHPGazeInterestField VHPGazeInterestField { get; set; }

    public Vector3 ScriptedTargetPosition
    {
        get { return _scriptedTargetPosition; }

        set
        {
            _scriptedTargetPosition = value;

            if (_currentGazeBehavior == VHPGaze.GazeBehavior.SCRIPTED)
                ScriptedGazeBehavior(_scriptedTargetPosition);
        }
    }

    private VHPGaze.GazeBehavior _currentGazeBehavior = VHPGaze.GazeBehavior.NONE;
    private Vector3 _scriptedTargetPosition = Vector3.zero;
    private Transform _initialParentTransform;
    private List<Transform> _gazeTargets = new List<Transform>();
    private int _targetListSize = 0;
    private int _targetIndex = 0;
    private List<Vector4> _currentTargetsPonderedPositions = new List<Vector4>();
    private int _targetPositionsListSize = 0;
    private List<Vector4> _previousTargetsPonderedPositions = new List<Vector4>();
    private float _targetsPositionsUpdateDeltaTime = 0f;
    private Dictionary<int, AudioSource> _audioSources = new Dictionary<int, AudioSource>();
    private float _targetSwitchingWaitingDuration = 0f;
    private bool _transitionInitialized = false;
    private Vector3 _finalTargetPosition;
    private float _transitionDuration;
    private float _xVelocity = 0;
    private float _yVelocity = 0;
    private float _zVelocity = 0;

    private void Start()
    {
        _initialParentTransform = transform.parent;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        // Sets the gaze behavior and initializes the corresponding functions. Updates only when the behavior changes.
        if (_currentGazeBehavior != VHPGaze.GazeBehaviorMode)
        {
            StopAllCoroutines();

            switch (VHPGaze.GazeBehaviorMode)
            {
                case VHPGaze.GazeBehavior.PROBABILISTIC:
                    transform.parent = VHPGaze.transform;
                    StartCoroutine(ProbabilisticGazeBehavior());
                    break;
                case VHPGaze.GazeBehavior.RANDOM:
                    transform.parent = _initialParentTransform;
                    StartCoroutine(RandomGazeBehavior(3f));
                    break;
                case VHPGaze.GazeBehavior.STATIC:
                    transform.parent = _initialParentTransform;
                    StaticGazeBehavior();
                    break;
                case VHPGaze.GazeBehavior.SCRIPTED:
                    transform.parent = _initialParentTransform;
                    ScriptedGazeBehavior(VHPGaze.NeutralTargetPosition);
                    break;
                case VHPGaze.GazeBehavior.NONE:
                    Debug.LogWarning("No gaze behavior selected in the gaze settings!");
                    break;
                default:
                    break;
            }

            _currentGazeBehavior = VHPGaze.GazeBehaviorMode;
        }
    }

    #region Probabilistic gaze behavior

    // Recursively handles the probabilistic gaze behavior.
    private IEnumerator ProbabilisticGazeBehavior()
    {
        Vector3 aimedTargetPosition = Vector3.zero;

        // Calls the target selection and target weighting functions when the interest field contains any targets.
        if (VHPGazeInterestField && VHPGazeInterestField.GazeTargets.Any())
        {
            if (transform.parent != VHPGaze.transform)
                transform.parent = VHPGaze.transform;

            aimedTargetPosition = SelectTargetProbabilistically(PonderateTargets());
        }

        // Sets a default gaze target position when the interest field contains no targets.
        else
        {
            if (transform.parent != _initialParentTransform)
                transform.parent = _initialParentTransform;

            aimedTargetPosition = VHPGaze.NeutralTargetPosition;
        }

        // Updates the target location if the parent GameObject is moving (head rotation, walking, etc.) to maintain the required position.
        if (transform.position != aimedTargetPosition)
            SmoothTargetTransition(aimedTargetPosition);

        yield return null;

        StartCoroutine(ProbabilisticGazeBehavior());
    }

    // Ponders the list of targets in the interest field.
    private List<Vector4> PonderateTargets()
    {
        // Clears the list of positions to update the moving targets' positions each frame.
        _currentTargetsPonderedPositions.Clear();

        // Updates the target list when objects enter or exit the interest field.
        if (_targetListSize != VHPGazeInterestField.GazeTargets.Count)
        {
            _gazeTargets = new List<Transform>(VHPGazeInterestField.GazeTargets);
            _previousTargetsPonderedPositions.Clear();

            _audioSources.Clear();
            AudioSource targetAudioSource;

            // Stores audio sources in a dictionary to avoid retrieving them each frame. The dictionary key corresponds to the target's position in the list.
            for (int i = 0; i < _gazeTargets.Count; i++)
            {
                if (_gazeTargets[i].parent && _gazeTargets[i].parent.GetComponent<AudioSource>())
                {
                    targetAudioSource = _gazeTargets[i].parent.GetComponent<AudioSource>();
                    _audioSources.Add(i, targetAudioSource);
                }
            }

            _targetListSize = _gazeTargets.Count;
        }

        // Applies ponderations based on distance, movement, and sound for each target in the interest field.
        for (int i = 0; i < _gazeTargets.Count; i++)
        {
            Vector3 targetPosition = _gazeTargets[i].position;
            float targetPonderedValue = 0f;

            targetPonderedValue = DistancePonderation(targetPosition, targetPonderedValue);

            if (_audioSources.Any())
            {
                AudioSource targetAudioSource;

                if (_audioSources.TryGetValue(i, out targetAudioSource))
                    targetPonderedValue = SoundPonderation(targetPosition, targetAudioSource, targetPonderedValue);
            }

            if (_previousTargetsPonderedPositions.Any())
                targetPonderedValue = MovementPonderation(targetPosition, _previousTargetsPonderedPositions[i], targetPonderedValue);

            _currentTargetsPonderedPositions.Add(new Vector4(targetPosition.x, targetPosition.y, targetPosition.z, targetPonderedValue));
        }

        // Updating the targets' positions list copy to enable position comparisions to detect any movements.
        if(_targetsPositionsUpdateDeltaTime >= 0.25f)
        {
            _previousTargetsPonderedPositions = new List<Vector4>(_currentTargetsPonderedPositions);
            _targetsPositionsUpdateDeltaTime = 0;
        }

        _targetsPositionsUpdateDeltaTime += Time.deltaTime;

        return _currentTargetsPonderedPositions;
    }

    #region Target ponderators

    // Returns a distance based pondered value.
    private float DistancePonderation(Vector3 targetPosition, float targetPonderedValue)
    {
        float targetDistance = Vector3.Distance(VHPGaze.EyesAveragePosition, targetPosition);
        targetPonderedValue = Mathf.Clamp(Mathf.RoundToInt(targetPonderedValue + (1 / targetDistance * 100f)), 0f, 100f);

        return targetPonderedValue;
    }

    // Returns a sound based pondered value considering the audio source distance and volume.
    private float SoundPonderation(Vector3 targetPosition, AudioSource audioSource, float targetPonderedValue)
    {
        if (audioSource.isPlaying)
        {
            float distanceWithAudioSource = Vector3.Distance(targetPosition, VHPGaze.EyesAveragePosition);
            float ponderationFactor = Mathf.Clamp((1 / distanceWithAudioSource) * audioSource.volume * 10, 3f, 10f);
            targetPonderedValue = Mathf.RoundToInt(targetPonderedValue * ponderationFactor);
        }

        return targetPonderedValue;
    }

    // Returns a movement based pondered value considering the object velocity.
    private float MovementPonderation(Vector3 targetPosition, Vector3 targetPreviousPosition, float targetPonderedValue)
    {
        if (targetPosition != targetPreviousPosition)
        {
            float distanceWithPreviousPosition = Vector3.Distance(targetPosition, targetPreviousPosition);
            float ponderationFactor = Mathf.Clamp(distanceWithPreviousPosition * 100, 3f, 10f);
            targetPonderedValue = Mathf.RoundToInt(targetPonderedValue * ponderationFactor);
        }

        return targetPonderedValue;
    }

    #endregion

    // Returns a probabilistically selected target position based on the list of pondered potential targets.
    private Vector3 SelectTargetProbabilistically(List<Vector4> orderedTargetPositions)
    {
        // Changes the target at a random frequency or when a target is added/removed from the list.
        if (_targetSwitchingWaitingDuration >= Random.Range(2f, 4f) || _targetPositionsListSize != orderedTargetPositions.Count)
        {
            _targetPositionsListSize = orderedTargetPositions.Count;

            float totalTargetsWeight = 0;

            // Calculates the total ponderation weight.
            for (int i = 0; i < orderedTargetPositions.Count; i++)
            {
                float targetWeight = orderedTargetPositions[i].w;
                totalTargetsWeight += targetWeight;
            }

            float targetProb = 0f;
            float targetCumulatedProb = targetProb;
            List<float> targetsProbs = new List<float>();

            // Calculates the selection probability percentage for each target based on its pondered score.
            for (int i = 0; i < orderedTargetPositions.Count; i++)
            {
                targetProb = orderedTargetPositions[i].w * 100 / totalTargetsWeight;
                // Adds the target selection probability to a cumulative score, creating value ranges between 1 and 100.
                // Each target's selection probability occupies a range between 0 and 100, without overlapping the ranges of other targets.
                targetCumulatedProb += targetProb;
                targetsProbs.Add(targetCumulatedProb);
            }

            // Generates a random number between 1 and 100.
            int randomProbScore = Random.Range(1, 101);

            // Checks which target's range contains the random value and updates the index to change the gaze target location.
            for (int i = 0; i < targetsProbs.Count(); i++)
            {
                if (targetsProbs[i] >= randomProbScore)
                {
                    _targetIndex = i;
                    break;
                }
            }

            _targetSwitchingWaitingDuration = 0;
        }

        else
            _targetSwitchingWaitingDuration += Time.deltaTime;

        return orderedTargetPositions[_targetIndex];
    }

    // Smooths the transition between target positions to enable realistic IK based animation.
    private void SmoothTargetTransition(Vector3 aimedTargetPosition)
    {
        if (!_transitionInitialized)
        {
            _finalTargetPosition = aimedTargetPosition;
            // Clamps the transition duration based on the distance between the initial and final positions.
            _transitionDuration = Mathf.Clamp(Vector3.Distance(transform.position, _finalTargetPosition) * Random.Range(0.2f, 0.3f), 0.05f, 1f);
            _transitionInitialized = true;
        }

        // Smooth, non-linear transition towards the target position.
        float xPosition = Mathf.SmoothDamp(transform.position.x, _finalTargetPosition.x, ref _xVelocity, _transitionDuration);
        float yPosition = Mathf.SmoothDamp(transform.position.y, _finalTargetPosition.y, ref _yVelocity, _transitionDuration);
        float zPosition = Mathf.SmoothDamp(transform.position.z, _finalTargetPosition.z, ref _zVelocity, _transitionDuration);

        transform.position = new Vector3(xPosition, yPosition, zPosition);

        // Allows variable reinitialization if the transition is complete or if the target changes during the transition.
        if (transform.position == aimedTargetPosition || aimedTargetPosition != _finalTargetPosition)
            _transitionInitialized = false;
    }

    #endregion

    #region Randome gaze behavior

    // Recursively handles the random gaze behavior.
    private IEnumerator RandomGazeBehavior(float updateDelay)
    {
        float randomVariation = Random.Range(-0.2f, 0.2f);
        float randomRecursiveDelay = Random.Range(3f, 5f);

        transform.position = VHPGaze.NeutralTargetPosition + new Vector3(randomVariation, randomVariation, 0);

        yield return new WaitForSeconds(updateDelay);

        StartCoroutine(RandomGazeBehavior(randomRecursiveDelay));
    }

    #endregion

    #region Static gaze behavior

    // Sets the static gaze behavior target position.
    private void StaticGazeBehavior()
    {
        transform.position = VHPGaze.NeutralTargetPosition;
    }

    #endregion

    #region Scripted gaze behavior

    // Sets the scripted gaze behavior target position.
    private void ScriptedGazeBehavior(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    #endregion
}
