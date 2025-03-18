/********************************************************************
Filename    :   VHPDemoManager.cs
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
using UnityEngine;

public class VHPDemoManager : MonoBehaviour
{
    public enum ActiveCharacter
    {
        NONE,
        FEMALE,
        MALE
    }

    public enum DemoTargetState
    {
        NONE,
        STATIC,
        MOVEMENT,
        SOUND,
        ALL
    }

    public static VHPDemoManager Instance { get; private set; }

    public ActiveCharacter activeCharacter;
    public DemoTargetState targetState;

    [SerializeField] private GameObject _femaleCharacter;
    [SerializeField] private GameObject _maleCharacter;
    [SerializeField] private bool _enableCharacterWalking = true;

    [SerializeField] private GameObject[] _staticTargets;
    [SerializeField] private GameObject[] _dynamicTargets;
    [SerializeField] private GameObject[] _soundTargets;

    private GameObject _demoCharacter;
    private Animator _characterAnimator;
    private bool _characterWalking = false;
    private ActiveCharacter _previousActiveCharacter;
    private DemoTargetState _previousTargetState;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
            Destroy(this);
    }

    void Start()
    {
        SetActiveCharacter();
        SetGazeTargetsState();
    }

    void Update()
    {
        if(activeCharacter != _previousActiveCharacter)
            SetActiveCharacter();

        if (targetState != _previousTargetState)
            SetGazeTargetsState();

        if (_demoCharacter)
        {
            if (_enableCharacterWalking && !_characterWalking)
                SetWlakingAnimationTransition(_enableCharacterWalking);

            else if (!_enableCharacterWalking && _characterWalking)
                SetWlakingAnimationTransition(_enableCharacterWalking);
        }
    }

    private void SetActiveCharacter()
    {
        switch (activeCharacter)
        {
            case ActiveCharacter.NONE:
                _femaleCharacter.SetActive(false);
                _maleCharacter.SetActive(false);
                _demoCharacter = null;
                break;
            case ActiveCharacter.FEMALE:
                _femaleCharacter.SetActive(true);
                _maleCharacter.SetActive(false);
                _demoCharacter = _femaleCharacter;
                break;
            case ActiveCharacter.MALE:
                _femaleCharacter.SetActive(false);
                _maleCharacter.SetActive(true);
                _demoCharacter = _maleCharacter;
                break;
            default:
                break;
        }

        _previousActiveCharacter = activeCharacter;

        if (_demoCharacter)
        {
            if (_demoCharacter.GetComponent<Animator>())
            {
                _characterAnimator = _demoCharacter.GetComponent<Animator>();
                SetWlakingAnimationTransition(_enableCharacterWalking);
            }

            else
                Debug.LogWarning("No demo character animator.");
        }
    }

    private void SetWlakingAnimationTransition(bool playAnimation)
    {
        _characterAnimator.SetBool("walk", playAnimation);
        _characterWalking = playAnimation;
    }

    private void SetGazeTargetsState()
    {
        switch (targetState)
        {
            case DemoTargetState.NONE:
                ActiveGameObjects(_staticTargets, false);
                ActiveGameObjects(_dynamicTargets, false);
                ActiveGameObjects(_soundTargets, false);
                break;
            case DemoTargetState.STATIC:
                ActiveGameObjects(_staticTargets, true);
                ActiveGameObjects(_dynamicTargets, false);
                ActiveGameObjects(_soundTargets, false);
                break;
            case DemoTargetState.MOVEMENT:
                ActiveGameObjects(_staticTargets, false);
                ActiveGameObjects(_dynamicTargets, true);
                ActiveGameObjects(_soundTargets, false);
                break;
            case DemoTargetState.SOUND:
                ActiveGameObjects(_staticTargets, false);
                ActiveGameObjects(_dynamicTargets, false);
                ActiveGameObjects(_soundTargets, true);
                break;
            case DemoTargetState.ALL:
                ActiveGameObjects(_staticTargets, true);
                ActiveGameObjects(_dynamicTargets, true);
                ActiveGameObjects(_soundTargets, true);
                break;
            default:
                break;
        }

        _previousTargetState = targetState;
    }

    private void ActiveGameObjects(GameObject[] targets, bool activeState)
    {
        foreach (GameObject target in targets)
            target.SetActive(activeState);
    }
}
