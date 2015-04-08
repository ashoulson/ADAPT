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

public abstract class CharacterMotor : MonoBehaviour {
	
	public float maxForwardSpeed = 1.5f;
	public float maxBackwardsSpeed = 1.5f;
	public float maxSidewaysSpeed = 1.5f;
	public float maxVelocityChange = 0.2f;
	
	public float gravity = 10.0f;
	public bool canJump = true;
	public float jumpHeight = 1.0f;
	
	public Vector3 forwardVector = Vector3.forward;
	protected Quaternion alignCorrection;
	
	private bool m_Grounded = false;
	public bool grounded {
		get { return m_Grounded; }
		protected set { m_Grounded = value; }
	}
	
	private bool m_Jumping = false;
	public bool jumping	{
		get { return m_Jumping; }
		protected set { m_Jumping = value; }
	}
	
	private Vector3 m_desiredMovementDirection;
	private Vector3 m_desiredFacingDirection;
	
	void Start () {
		alignCorrection = new Quaternion();
		alignCorrection.SetLookRotation(forwardVector, Vector3.up);
		alignCorrection = Quaternion.Inverse(alignCorrection);
	}
	
	public Vector3 desiredMovementDirection {
		get { return m_desiredMovementDirection; }
		set {
			m_desiredMovementDirection = value;
			if (m_desiredMovementDirection.magnitude>1) m_desiredMovementDirection = m_desiredMovementDirection.normalized;
		}
	}
	public Vector3 desiredFacingDirection {
		get { return m_desiredFacingDirection; }
		set {
			m_desiredFacingDirection = value;
			if (m_desiredFacingDirection.magnitude>1) m_desiredFacingDirection = m_desiredFacingDirection.normalized;
		}
	}
	public Vector3 desiredVelocity {
		get {
			//return m_desiredVelocity;
			if (m_desiredMovementDirection==Vector3.zero) return Vector3.zero;
			else {
				float zAxisEllipseMultiplier = (m_desiredMovementDirection.z>0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
				Vector3 temp = new Vector3(m_desiredMovementDirection.x, 0, m_desiredMovementDirection.z/zAxisEllipseMultiplier).normalized;
				float length = new Vector3(temp.x, 0, temp.z*zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
				Vector3 velocity = m_desiredMovementDirection * length;
				return transform.rotation * velocity;
			}
		}
	}
}