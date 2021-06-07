/********************************************************************
Filename    :   VHPDemoRotateAnimation.cs
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

public class VHPDemoRotateAnimation : MonoBehaviour
{
    public float speed = 1f;
    public Vector3 rotationAxis = new Vector3 (0, 1, 0);

    // Simple object rotation animation.
    void Update()
    {
        transform.Rotate(rotationAxis, speed * Time.deltaTime);
    }
}
