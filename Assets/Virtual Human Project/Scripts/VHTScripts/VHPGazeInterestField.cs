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
    //public List<Transform> GazeTargets { get; private set; } = new List<Transform>();
    public List<Transform> GazeTargets  = new List<Transform>();

    private void OnEnable()
    {
        // Starting a coroutine to check for disabled game objects in the interest field every second as they do not call the trigger exit function.
        StartCoroutine(CheckForDisabledTargets());
    }

    private void OnDisable()
    {
        GazeTargets.Clear();
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider target)
    {
        // No need to check the target tag, as the probabilistic gaze configurator editor tool set the collision matrix to only enable VHPGazeComponents self collisions.
        GazeTargets.Add(target.transform);
    }

    private void OnTriggerExit(Collider target)
    {
        GazeTargets.Remove(target.transform);
    }

    // Coroutine to check for disabled game objects in the interest field every second as they do not call the trigger exit function.
    private IEnumerator CheckForDisabledTargets()
    {
        if (GazeTargets.Any())
        {
            // Inverted for loop avoiding null reference exceptions when removing a disabled element from the list.
            for (int i = GazeTargets.Count - 1; i > 0; i--)
            {
                if (!GazeTargets[i].gameObject.activeInHierarchy)
                    GazeTargets.Remove(GazeTargets[i]);
            }
        }

        yield return new WaitForSeconds(1f);

        StartCoroutine(CheckForDisabledTargets());
    }
}
