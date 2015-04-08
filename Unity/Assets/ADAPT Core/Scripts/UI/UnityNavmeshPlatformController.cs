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

[RequireComponent(typeof(UnitySteeringController))]
public class UnityNavmeshPlatformController : MonoBehaviour
{
    private UnitySteeringController steering;

    public float maxForwardSpeed = 1.5f;
    public float maxBackwardsSpeed = 1.5f;
    public float maxSidewaysSpeed = 1.5f;
    public float maxVelocityChange = 0.2f;

    new public Camera camera = null;

    public float walkMultiplier = 0.5f;
    public bool defaultIsWalk = false;

    public float maxDistance = 2.0f;

    private Vector3 lastPosition;

    void Start()
    {
        this.lastPosition = transform.position;
        this.steering = GetComponent<UnitySteeringController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get input vector from kayboard or analog stick and make it length 1 at most
        Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        if (directionVector.magnitude > 1) directionVector = directionVector.normalized;
        directionVector = directionVector.normalized * Mathf.Pow(directionVector.magnitude, 2);

        // Rotate input vector into camera space so is up camera's up and right is camera's right
        directionVector = this.camera.transform.rotation * directionVector;

        // Rotate input vector to be perpendicular to character's up vector
        Quaternion camToCharacterSpace = Quaternion.FromToRotation(this.camera.transform.forward * -1, transform.up);
        directionVector = (camToCharacterSpace * directionVector);

        // Make input vector relative to Character's own orientation
        directionVector = Quaternion.Inverse(transform.rotation) * directionVector;

        if (walkMultiplier != 1)
            if ((Input.GetKey("left shift") || Input.GetKey("right shift")) != defaultIsWalk)
                directionVector *= walkMultiplier;

        float difference = 0.0f;
        if (Input.GetKey(KeyCode.Q) == true)
            difference -= 1.0f;
        if (Input.GetKey(KeyCode.E) == true)
            difference += 1.0f;

        this.UpdateOrientation(difference, 200.0f);
        this.UpdateVelocity(directionVector);
    }

	private void UpdateVelocity(Vector3 directionVector) 
    {
		Vector3 velocity = 
            (transform.position - this.lastPosition) / Time.deltaTime;
        this.lastPosition = transform.position;
		
		// Calculate how fast we should be moving
		Vector3 movement = velocity;

		// Apply a force that attempts to reach our target velocity
		Vector3 velocityChange = this.DesiredVelocity(directionVector) - velocity;
		if (velocityChange.magnitude > maxVelocityChange) 
			velocityChange = velocityChange.normalized * maxVelocityChange;
		movement += velocityChange;
		
		// Apply movement
        this.steering.Move(movement * Time.deltaTime);
    }

    private void UpdateOrientation(float difference, float speed)
    {
        if (difference != 0.0f)
        {
            Vector3 euler = this.transform.rotation.eulerAngles;
            Vector3 eulerPlus = 
                new Vector3(
                    euler.x,
                    euler.y + (difference * speed) * Time.deltaTime, 
                    euler.z);
            this.steering.orientationBehavior = OrientationBehavior.None;
            this.steering.desiredOrientation = Quaternion.Euler(eulerPlus);
        }
        else
        {
            this.steering.orientationBehavior = OrientationBehavior.LookForward;
        }
    }

    public Vector3 DesiredVelocity(Vector3 directionVector)
    {
        if (directionVector == Vector3.zero)
        {
            return Vector3.zero;
        }
        else
        {
            float zAxisEllipseMultiplier = (directionVector.z > 0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
            Vector3 temp = new Vector3(directionVector.x, 0, directionVector.z / zAxisEllipseMultiplier).normalized;
            float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
            Vector3 velocity = directionVector * length;
            return transform.rotation * velocity;
        }
    }

}
