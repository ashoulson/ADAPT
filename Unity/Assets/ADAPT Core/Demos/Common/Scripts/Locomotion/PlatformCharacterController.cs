#region License
/*
* Agent Development and Prototyping Testbed
* https://github.com/ashoulson/ADAPT
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
*
* This file is part of ADAPT.
* 
* ADAPT is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* ADAPT is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with ADAPT.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using UnityEngine;
using System.Collections;

public class PlatformCharacterController : MonoBehaviour {
	
	private CharacterMotor motor;
	
	public float walkMultiplier = 0.5f;
	public bool defaultIsWalk = false;
	
	// Use this for initialization
	void Start () {
		motor = GetComponent(typeof(CharacterMotor)) as CharacterMotor;
		if (motor==null) Debug.Log("Motor is null!!");
	}
	
	// Update is called once per frame
	void Update () {
		// Get input vector from kayboard or analog stick and make it length 1 at most
		Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		if (directionVector.magnitude>1) directionVector = directionVector.normalized;
		directionVector = directionVector.normalized * Mathf.Pow(directionVector.magnitude, 2);
		
		// Rotate input vector into camera space so up is camera's up and right is camera's right
		directionVector = Camera.main.transform.rotation * directionVector;
		
		// Rotate input vector to be perpendicular to character's up vector
		Quaternion camToCharacterSpace = Quaternion.FromToRotation(Camera.main.transform.forward*-1, transform.up);
		directionVector = (camToCharacterSpace * directionVector);
		
		// Make input vector relative to Character's own orientation
		directionVector = Quaternion.Inverse(transform.rotation) * directionVector;
		
		if (walkMultiplier!=1) {
			if ( (Input.GetKey("left shift") || Input.GetKey("right shift")) != defaultIsWalk ) {
				directionVector *= walkMultiplier;
			}
		}
		
		// Apply direction
		motor.desiredMovementDirection = directionVector;
	}
}
