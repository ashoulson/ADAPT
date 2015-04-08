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

public class HeadLookController : MonoBehaviour {
	
	public Transform rootNode;
	public BendingSegment[] segments;
	public NonAffectedJoint[] nonAffectedJoints;
	public Vector3 headLookVector = Vector3.forward;
	public Vector3 headUpVector = Vector3.up;
    public Vector3 target = Vector3.zero;
	public float effect = 1;
	public bool overrideAnimation = false;
	
	void Start () {
		if (rootNode == null) {
			rootNode = transform;
		}
		
		// Setup segments
		foreach (BendingSegment segment in segments) {
			Quaternion parentRot = segment.firstTransform.parent.rotation;
			Quaternion parentRotInv = Quaternion.Inverse(parentRot);
			segment.referenceLookDir =
				parentRotInv * rootNode.rotation * headLookVector.normalized;
			segment.referenceUpDir =
				parentRotInv * rootNode.rotation * headUpVector.normalized;
			segment.angleH = 0;
			segment.angleV = 0;
			segment.dirUp = segment.referenceUpDir;
			
			segment.chainLength = 1;
			Transform t = segment.lastTransform;
			while (t != segment.firstTransform && t != t.root) {
				segment.chainLength++;
				t = t.parent;
			}
			
			segment.origRotations = new Quaternion[segment.chainLength];
			t = segment.lastTransform;
			for (int i=segment.chainLength-1; i>=0; i--) {
				segment.origRotations[i] = t.localRotation;
				t = t.parent;
			}
		}
	}
	
	void LateUpdate () {
		if (Time.deltaTime == 0)
			return;
		
		// Remember initial directions of joints that should not be affected
		Vector3[] jointDirections = new Vector3[nonAffectedJoints.Length];
		for (int i=0; i<nonAffectedJoints.Length; i++) {
			foreach (Transform child in nonAffectedJoints[i].joint) {
				jointDirections[i] = child.position - nonAffectedJoints[i].joint.position;
				break;
			}
		}
		
		// Handle each segment
		foreach (BendingSegment segment in segments) {
			Transform t = segment.lastTransform;
			if (overrideAnimation) {
				for (int i=segment.chainLength-1; i>=0; i--) {
					t.localRotation = segment.origRotations[i];
					t = t.parent;
				}
			}
			
			Quaternion parentRot = segment.firstTransform.parent.rotation;
			Quaternion parentRotInv = Quaternion.Inverse(parentRot);
			
			// Desired look direction in world space
			Vector3 lookDirWorld = (target - segment.lastTransform.position).normalized;
			
			// Desired look directions in neck parent space
			Vector3 lookDirGoal = (parentRotInv * lookDirWorld);
			
			// Get the horizontal and vertical rotation angle to look at the target
			float hAngle = AngleAroundAxis(
				segment.referenceLookDir, lookDirGoal, segment.referenceUpDir
			);
			
			Vector3 rightOfTarget = Vector3.Cross(segment.referenceUpDir, lookDirGoal);
			
			Vector3 lookDirGoalinHPlane =
				lookDirGoal - Vector3.Project(lookDirGoal, segment.referenceUpDir);
			
			float vAngle = AngleAroundAxis(
				lookDirGoalinHPlane, lookDirGoal, rightOfTarget
			);
			
			// Handle threshold angle difference, bending multiplier,
			// and max angle difference here
			float hAngleThr = Mathf.Max(
				0, Mathf.Abs(hAngle) - segment.thresholdAngleDifference
			) * Mathf.Sign(hAngle);
			
			float vAngleThr = Mathf.Max(
				0, Mathf.Abs(vAngle) - segment.thresholdAngleDifference
			) * Mathf.Sign(vAngle);
			
			hAngle = Mathf.Max(
				Mathf.Abs(hAngleThr) * Mathf.Abs(segment.bendingMultiplier),
				Mathf.Abs(hAngle) - segment.maxAngleDifference
			) * Mathf.Sign(hAngle) * Mathf.Sign(segment.bendingMultiplier);
			
			vAngle = Mathf.Max(
				Mathf.Abs(vAngleThr) * Mathf.Abs(segment.bendingMultiplier),
				Mathf.Abs(vAngle) - segment.maxAngleDifference
			) * Mathf.Sign(vAngle) * Mathf.Sign(segment.bendingMultiplier);
			
			// Handle max bending angle here
			hAngle = Mathf.Clamp(hAngle, -segment.maxBendingAngle, segment.maxBendingAngle);
			vAngle = Mathf.Clamp(vAngle, -segment.maxBendingAngle, segment.maxBendingAngle);
			
			Vector3 referenceRightDir =
				Vector3.Cross(segment.referenceUpDir, segment.referenceLookDir);
			
			// Lerp angles
			segment.angleH = Mathf.Lerp(
				segment.angleH, hAngle, Time.deltaTime * segment.responsiveness
			);
			segment.angleV = Mathf.Lerp(
				segment.angleV, vAngle, Time.deltaTime * segment.responsiveness
			);
			
			// Get direction
			lookDirGoal = Quaternion.AngleAxis(segment.angleH, segment.referenceUpDir)
				* Quaternion.AngleAxis(segment.angleV, referenceRightDir)
				* segment.referenceLookDir;
			
			// Make look and up perpendicular
			Vector3 upDirGoal = segment.referenceUpDir;
			Vector3.OrthoNormalize(ref lookDirGoal, ref upDirGoal);
			
			// Interpolated look and up directions in neck parent space
			Vector3 lookDir = lookDirGoal;
			segment.dirUp = Vector3.Slerp(segment.dirUp, upDirGoal, Time.deltaTime*5);
			Vector3.OrthoNormalize(ref lookDir, ref segment.dirUp);
			
			// Look rotation in world space
			Quaternion lookRot = (
				(parentRot * Quaternion.LookRotation(lookDir, segment.dirUp))
				* Quaternion.Inverse(
					parentRot * Quaternion.LookRotation(
						segment.referenceLookDir, segment.referenceUpDir
					)
				)
			);
			
			// Distribute rotation over all joints in segment
			Quaternion dividedRotation =
				Quaternion.Slerp(Quaternion.identity, lookRot, effect / segment.chainLength);
			t = segment.lastTransform;
			for (int i=0; i<segment.chainLength; i++) {
				t.rotation = dividedRotation * t.rotation;
				t = t.parent;
			}
		}
		
		// Handle non affected joints
		for (int i=0; i<nonAffectedJoints.Length; i++) {
			Vector3 newJointDirection = Vector3.zero;
			
			foreach (Transform child in nonAffectedJoints[i].joint) {
				newJointDirection = child.position - nonAffectedJoints[i].joint.position;
				break;
			}
			
			Vector3 combinedJointDirection = Vector3.Slerp(
				jointDirections[i], newJointDirection, nonAffectedJoints[i].effect
			);
			
			nonAffectedJoints[i].joint.rotation = Quaternion.FromToRotation(
				newJointDirection, combinedJointDirection
			) * nonAffectedJoints[i].joint.rotation;
		}
	}
	
	// The angle between dirA and dirB around axis
	public static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
		// Project A and B onto the plane orthogonal target axis
		dirA = dirA - Vector3.Project(dirA, axis);
		dirB = dirB - Vector3.Project(dirB, axis);
		
		// Find (positive) angle between A and B
		float angle = Vector3.Angle(dirA, dirB);
		
		// Return angle multiplied with 1 or -1
		return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
	}
}