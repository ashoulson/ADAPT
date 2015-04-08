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

[RequireComponent(typeof(CharacterController))]
public class NormalCharacterMotor : CharacterMotor {
	
	public float maxRotationSpeed = 270;
	
	private bool firstframe = true;
	
	private void OnEnable () {
		firstframe = true;
	}
	
	private void UpdateFacingDirection() {
		// Calculate which way character should be facing
		float facingWeight = desiredFacingDirection.magnitude;
		Vector3 combinedFacingDirection = (
			transform.rotation * desiredMovementDirection * (1-facingWeight)
			+ desiredFacingDirection * facingWeight
		);
		combinedFacingDirection = LocomotionUtil.ProjectOntoPlane(combinedFacingDirection, transform.up);
		combinedFacingDirection = alignCorrection * combinedFacingDirection;
		
		if (combinedFacingDirection.sqrMagnitude > 0.01f) {
			Vector3 newForward = LocomotionUtil.ConstantSlerp(
				transform.forward,
				combinedFacingDirection,
				maxRotationSpeed*Time.deltaTime
			);
			newForward = LocomotionUtil.ProjectOntoPlane(newForward, transform.up);
			//Debug.DrawLine(transform.position, transform.position+newForward, Color.yellow);
			Quaternion q = new Quaternion();
			q.SetLookRotation(newForward, transform.up);
			transform.rotation = q;
		}
	}
	
	private void UpdateVelocity() {
		CharacterController controller = GetComponent(typeof(CharacterController)) as CharacterController;
		Vector3 velocity = controller.velocity;
		if (firstframe) {
			velocity = Vector3.zero;
			firstframe = false;
		}
		if (grounded) velocity = LocomotionUtil.ProjectOntoPlane(velocity, transform.up);
		
		// Calculate how fast we should be moving
		Vector3 movement = velocity;
		//bool hasJumped = false;
		jumping = false;
		if (grounded) {
			// Apply a force that attempts to reach our target velocity
			Vector3 velocityChange = (desiredVelocity - velocity);
			if (velocityChange.magnitude > maxVelocityChange) {
				velocityChange = velocityChange.normalized * maxVelocityChange;
			}
			movement += velocityChange;
		
			// Jump
			if (canJump && Input.GetButton("Jump")) {
				movement += transform.up * Mathf.Sqrt(2 * jumpHeight * gravity);
				//hasJumped = true;
				jumping = true;
			}
		}
		
		// Apply downwards gravity
		movement += transform.up * -gravity * Time.deltaTime;
		
		if (jumping) {
			movement -= transform.up * -gravity * Time.deltaTime / 2;
			
		}
		
		// Apply movement
		CollisionFlags flags = controller.Move(movement * Time.deltaTime);
		grounded = (flags & CollisionFlags.CollidedBelow) != 0;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateFacingDirection();
		
		UpdateVelocity();
	}
}
