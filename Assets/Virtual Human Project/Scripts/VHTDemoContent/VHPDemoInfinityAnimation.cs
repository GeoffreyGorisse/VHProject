/********************************************************************
Filename    :   VHPDemoInfinityAnimation.cs
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

public class VHPDemoInfinityAnimation : MonoBehaviour
{
    void Update()
    {
        // Lemniscate of Bernoulli position animation.
        float scale = 4 / (3 - Mathf.Cos(2 * Time.time));
        transform.localPosition = new Vector3(scale * Mathf.Cos(Time.time), scale * Mathf.Sin(2 * Time.time) / 2, 0);
    }
}
