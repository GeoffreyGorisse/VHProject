/********************************************************************
Filename    :   VHPGazeInterestField.cs
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

public class VHPGazeInterestField : MonoBehaviour
{
    public List<Transform> GazeTargets  = new List<Transform>();

    private void OnEnable()
    {
        StartCoroutine(CheckForDisabledTargets());
    }

    private void OnDisable()
    {
        GazeTargets.Clear();
        StopCoroutine(CheckForDisabledTargets());
    }

    private void OnTriggerEnter(Collider target)
    {
        GazeTargets.Add(target.transform);      // No need to check the target's tag, as the probabilistic gaze configurator editor sets the collision matrix to enable self-collisions for VHPGazeComponents.
    }

    private void OnTriggerExit(Collider target)
    {
        GazeTargets.Remove(target.transform);
    }

    // Checks for disabled GameObjects in the interest field, as they do not call the trigger exit function.
    private IEnumerator CheckForDisabledTargets()
    {
        while (gameObject.activeSelf)
        {
            if (GazeTargets.Any())
            {
                // Inverted loop avoiding null reference exceptions when removing a disabled element from the list.
                for (int i = GazeTargets.Count - 1; i >= 0; i--)
                {
                    if (!GazeTargets[i].gameObject.activeInHierarchy)
                        GazeTargets.Remove(GazeTargets[i]);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
}