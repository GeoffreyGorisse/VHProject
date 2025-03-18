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
using UnityEngine;

public class VHPDemoRotateAnimation : MonoBehaviour
{
    [SerializeField, Range(0, 100)] private float _speed = 50f;
    [SerializeField] private Vector3 _rotationAxis = new Vector3 (0, 0, -1);

    void Update()
    {
        transform.Rotate(_rotationAxis, _speed * Time.deltaTime);
    }
}
