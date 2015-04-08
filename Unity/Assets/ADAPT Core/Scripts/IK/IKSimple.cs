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

/*
Copyright (c) 2008, Rune Skovbo Johansen & Unity Technologies ApS

See the document "TERMS OF USE" included in the project folder for licencing details.
*/
#endregion

using UnityEngine;
using System.Collections;

public class IKSimple : IKSolver 
{
    public override void Solve(
        IKJoint[] joints, 
        Transform endEffector, 
        Vector3 target) 
    {	
		// Get the axis of rotation for each joint
		Vector3[] rotateAxes = new Vector3[joints.Length-2];
		float[] rotateAngles = new float[joints.Length-2];
		Quaternion[] rotations = new Quaternion[joints.Length-2];
		for (int i=0; i<joints.Length-2; i++) 
        {
			rotateAxes[i] = Vector3.Cross(
				joints[i+1].position - joints[i].position,
                joints[i+2].position - joints[i+1].position);
            rotateAxes[i] = 
                Quaternion.Inverse(joints[i].rotation) * rotateAxes[i];
			rotateAxes[i] = rotateAxes[i].normalized;
			rotateAngles[i] = Vector3.Angle(
                joints[i+1].position - joints[i].position,
                joints[i+1].position - joints[i+2].position);
			
			rotations[i] = joints[i+1].localRotation;
		}
		
		// Get the length of each bone
		float[] boneLengths = new float[joints.Length-1];
		float legLength = 0;
		for (int i=0; i<joints.Length-1; i++) 
        {
			boneLengths[i] = 
                (joints[i+1].position-joints[i].position).magnitude;
			legLength += boneLengths[i];
		}
		epsilon = legLength*0.001f;
		
		float currentDistance = 
            (endEffector.position-joints[0].position).magnitude;
		float targetDistance = (target-joints[0].position).magnitude;
		
		// Search for right joint bendings to get target distance between 
        // the hip and the foot
		float bendingLow, bendingHigh;
		bool minIsFound = false;
		bool bendMore = false;
		if (targetDistance > currentDistance) 
        {
			minIsFound = true;
			bendingHigh = 1;
			bendingLow = 0;
		}
		else 
        {
			bendMore = true;
			bendingHigh = 1;
			bendingLow = 0;
		}

		int tries = 0;
        float testDistance = Mathf.Abs(currentDistance - targetDistance);
        while (testDistance > epsilon && tries < maxIterations)
        {
			tries++;
			float bendingNew;
			if (minIsFound == true) 
                bendingNew = (bendingLow + bendingHigh) / 2;
			else
                bendingNew = bendingHigh;

			for (int i=0; i < joints.Length - 2; i++) 
            {
				float newAngle;
				if (bendMore == false) 
                    newAngle = Mathf.Lerp(180, rotateAngles[i], bendingNew);
				else 
                    newAngle = 
                        rotateAngles[i] * (1 - bendingNew) 
                        + (rotateAngles[i] - 30) * bendingNew;

				float angleDiff = (rotateAngles[i]-newAngle);
				Quaternion addedRotation = 
                    Quaternion.AngleAxis(angleDiff, rotateAxes[i]);
				Quaternion newRotation = addedRotation * rotations[i];
				joints[i + 1].localRotation = newRotation;
			}

            Vector3 difference = endEffector.position - joints[0].position;
            if (targetDistance > difference.magnitude) 
                minIsFound = true;

            if (minIsFound == true) 
            {
				if (targetDistance > currentDistance) 
                    bendingHigh = bendingNew;
				else 
                    bendingLow = bendingNew;
				if (bendingHigh < 0.01f) 
                    break;
			}
			else 
            {
				bendingLow = bendingHigh;
				bendingHigh++;
			}

            testDistance = Mathf.Abs(currentDistance - targetDistance);
		}
		
		// Rotate hip bone such that foot is at desired position
        float angle = 
            Vector3.Angle(
                endEffector.position - joints[0].position, 
                target-joints[0].position);
        Vector3 axis =
            Vector3.Cross(
                endEffector.position - joints[0].position,
                target-joints[0].position);
        joints[0].rotation = 
            Quaternion.AngleAxis(angle, axis) * joints[0].rotation;
	}

    public override void Solve(IKJoint[] joints, Vector3 target)
    {
        this.Solve(joints, joints[joints.Length - 1].transform, target);
    }
}
