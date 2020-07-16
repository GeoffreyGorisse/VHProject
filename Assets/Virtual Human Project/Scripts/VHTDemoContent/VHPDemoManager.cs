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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VHPDemoManager : MonoBehaviour
{
    public static VHPDemoManager s_Instance { get; private set; }

    public GameObject femaleCharacter, maleCharacter;
    public bool enableCharacterWalking = false;

    private GameObject m_demoCharacter;
    private Animator m_characterAnimator;
    private bool m_characterWalking = false;

    public enum ActiveCharacter
    {
        NONE,
        FEMALE,
        MALE
    }

    public ActiveCharacter activeCharacter;
    private ActiveCharacter previousActiveCharacter;

    public enum DemoTargetState
    {
        NONE,
        STATIC,
        MOVEMENT,
        SOUND,
        ALL
    }

    public DemoTargetState targetState;
    private DemoTargetState previousTargetState;

    public GameObject[] staticTargets;
    public GameObject[] dynamicTargets;
    public GameObject[] soundTargets;

    private void Awake()
    {
        if (!s_Instance)
        {
            s_Instance = this;
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
        if(activeCharacter != previousActiveCharacter)
            SetActiveCharacter();

        if (targetState != previousTargetState)
            SetGazeTargetsState();

        if (m_demoCharacter)
        {
            if (enableCharacterWalking && !m_characterWalking)
                SetWlakingAnimationTransition(enableCharacterWalking);

            else if (!enableCharacterWalking && m_characterWalking)
                SetWlakingAnimationTransition(enableCharacterWalking);
        }
    }

    private void SetActiveCharacter()
    {
        switch (activeCharacter)
        {
            case ActiveCharacter.NONE:
                femaleCharacter.SetActive(false);
                maleCharacter.SetActive(false);
                m_demoCharacter = null;
                break;
            case ActiveCharacter.FEMALE:
                femaleCharacter.SetActive(true);
                maleCharacter.SetActive(false);
                m_demoCharacter = femaleCharacter;
                break;
            case ActiveCharacter.MALE:
                femaleCharacter.SetActive(false);
                maleCharacter.SetActive(true);
                m_demoCharacter = maleCharacter;
                break;
            default:
                break;
        }

        previousActiveCharacter = activeCharacter;

        if (m_demoCharacter)
        {
            if (m_demoCharacter.GetComponent<Animator>())
            {
                m_characterAnimator = m_demoCharacter.GetComponent<Animator>();
                SetWlakingAnimationTransition(enableCharacterWalking);
            }

            else
                Debug.LogWarning("No demo character animator.");
        }
    }

    private void SetWlakingAnimationTransition(bool playAnimation)
    {
        m_characterAnimator.SetBool("walk", playAnimation);
        m_characterWalking = playAnimation;
    }

    private void SetGazeTargetsState()
    {
        switch (targetState)
        {
            case DemoTargetState.NONE:
                ActiveGameObjects(staticTargets, false);
                ActiveGameObjects(dynamicTargets, false);
                ActiveGameObjects(soundTargets, false);
                break;
            case DemoTargetState.STATIC:
                ActiveGameObjects(staticTargets, true);
                ActiveGameObjects(dynamicTargets, false);
                ActiveGameObjects(soundTargets, false);
                break;
            case DemoTargetState.MOVEMENT:
                ActiveGameObjects(staticTargets, false);
                ActiveGameObjects(dynamicTargets, true);
                ActiveGameObjects(soundTargets, false);
                break;
            case DemoTargetState.SOUND:
                ActiveGameObjects(staticTargets, false);
                ActiveGameObjects(dynamicTargets, false);
                ActiveGameObjects(soundTargets, true);
                break;
            case DemoTargetState.ALL:
                ActiveGameObjects(staticTargets, true);
                ActiveGameObjects(dynamicTargets, true);
                ActiveGameObjects(soundTargets, true);
                break;
            default:
                break;
        }

        previousTargetState = targetState;
    }

    private void ActiveGameObjects(GameObject[] targets, bool activeState)
    {
        foreach (GameObject target in targets)
            target.SetActive(activeState);
    }
}
