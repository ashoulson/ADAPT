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
using System.Collections.Generic;

/// <summary>
/// Version of the Unity HeadLookController 
/// (http://unity3d.com/support/resources/unity-extensions/head-look-controller) that is used wtih shadows.
/// Should be the same as the original HeadLookController, but with pointers to the
/// root transform swapped with pointers to the root of the skeleton.
/// 
/// \warning The ShadowHeadlookController does not have an Update() function. Rather, it has a 
/// ControlledUpdate() function that must be called manually by ShadowAnimSystem.cs. See the ControlledUpdate()
/// documentation for more details. 
/// </summary>
[RequireComponent(typeof(ShadowCoordinator))]
public class ShadowHeadLookController : ShadowController
{
    //public bool IsStandAlone = false;
    public bool Restricted = false;

    public BendingSegment[] segments;
    public NonAffectedJoint[] nonAffectedJoints;
    public Vector3 headLookVector = Vector3.forward;
    public Vector3 headUpVector = Vector3.up;
    public Vector3 target = Vector3.zero; //target that we are supposed to look at

    public float effect = 1;

    #region Shadow Functions
    // Updates the Bending Segments, copying over the references from the
    // original transforms to the shadow transforms
    private void ShadowBendingSegments()
    {
        foreach (BendingSegment segment in this.segments)
        {
            segment.firstTransform = 
                this.shadow.FindInShadow(segment.firstTransform);
            segment.lastTransform =
                this.shadow.FindInShadow(segment.lastTransform);
        }
    }

    // Updates the Non-Affected Joints, copying over the references from
    // the original transforms to the shadow transforms
    private void ShadowNonAffectedJoints()
    {
        foreach (NonAffectedJoint nonAffect in this.nonAffectedJoints)
            nonAffect.joint = this.shadow.FindInShadow(nonAffect.joint);
    }
    #endregion

    #region MonoBehavior Functions
    public override void ControlledAwake()
    {
        // We shadowed the model on which the bending segments were set, so
        // we need to update the references to the transforms in the shadow
        this.ShadowBendingSegments();
        this.ShadowNonAffectedJoints();

        // Setup segments
        foreach (BendingSegment segment in segments)
        {
            Quaternion parentRot = segment.firstTransform.parent.rotation;
            Quaternion parentRotInv = Quaternion.Inverse(parentRot);
            segment.referenceLookDir =
                parentRotInv * transform.rotation * headLookVector.normalized;
            segment.referenceUpDir =
                parentRotInv * transform.rotation * headUpVector.normalized;
            segment.angleH = 0;
            segment.angleV = 0;
            segment.dirUp = segment.referenceUpDir;

            // Calculates chainLength, i.e. the tree depth of this segment
            segment.chainLength = 1;
            Transform t = segment.lastTransform;
            while (t != segment.firstTransform && t != t.root)
            {
                segment.chainLength++;
                t = t.parent;
            }

            // Old code for standalone mode, now removed
            //if (this.IsStandAlone == true)
            //{
            //    // Store the original rotations in the current segment's chain 
            //    // inside segment.origRotations
            //    segment.origRotations = new Quaternion[segment.chainLength];
            //    t = segment.lastTransform;
            //    for (int i = segment.chainLength - 1; i >= 0; i--)
            //    {
            //        segment.origRotations[i] = t.localRotation;
            //        t = t.parent;
            //    }
            //}
        }
    }

    /// <summary>
    /// Like the update function, but must be called manually.
    /// This is in order to avoid a race condition - the headlook controller must take input
    /// from the blend of all the other controllers to begin with, then it has to copy the 
    /// transforms to its own shadow, then it must twist its shadow. In order to enforce 
    /// this order, the update function must be called manually in ShadowAnimSystem.cs.
    /// </summary>
    public override void ControlledUpdate()
    {
        // Remember initial directions of joints that should not be affected

        Vector3[] jointDirections = new Vector3[nonAffectedJoints.Length];
        for (int i = 0; i < nonAffectedJoints.Length; i++)
        {
            if (nonAffectedJoints[i].joint.GetChildCount() > 0)
            {
                Transform child = nonAffectedJoints[i].joint.GetChild(0);
                jointDirections[i] = child.position - nonAffectedJoints[i].joint.position;
            }
        }

        // Handle each segment
        foreach (BendingSegment seg in segments)
        {
            Transform t = seg.lastTransform;

            // Old code for standalone mode, now removed
            //if (this.IsStandAlone == true)
            //{
            //    // Reset all of the joints to the default stored positions
            //    for (int i = seg.chainLength - 1; i >= 0; i--)
            //    {
            //        t.localRotation = seg.origRotations[i];
            //        t = t.parent;
            //    }
            //}

            Quaternion parentRotOrig = seg.firstTransform.parent.rotation;
            Quaternion parentRot = parentRotOrig;
            Quaternion parentRotInv = Quaternion.Inverse(parentRot);

            // Desired look direction in world space.
            Vector3 lookDirWorld = 
                (target - seg.lastTransform.position).normalized;

            // Desired look directions in neck parent space
            // If headlook is "on", then we look towards the target,
            // otherwise we simply look in the direction of the forward vector.
            Vector3 lookDirGoal;
            lookDirGoal = (parentRotInv * lookDirWorld);

            // Get the horizontal and vertical rotation angle to look at the
            // target
            Vector3 rightOfTarget = 
                Vector3.Cross(seg.referenceUpDir, lookDirGoal);

            Vector3 lookDirGoalinHPlane =
                lookDirGoal 
                - Vector3.Project(lookDirGoal, seg.referenceUpDir);

            float hAngle = 
                CalculateAngle(
                    seg, 
                    AngleAroundAxis(
                        seg.referenceLookDir, 
                        lookDirGoal, 
                        seg.referenceUpDir));

            float vAngle = 
                CalculateAngle(
                    seg,
                    AngleAroundAxis(
                        lookDirGoalinHPlane, 
                        lookDirGoal, 
                        rightOfTarget));

            // Lerp angles
            seg.angleH = Mathf.Lerp(
                seg.angleH, 
                hAngle, 
                Time.deltaTime * seg.responsiveness);

            seg.angleV = 
                Mathf.Lerp(
                    seg.angleV, 
                    vAngle, 
                    Time.deltaTime * seg.responsiveness);

            Vector3 referenceRightDir =
                Vector3.Cross(
                    seg.referenceUpDir,
                    seg.referenceLookDir);

            // Get direction
            lookDirGoal = 
                Quaternion.AngleAxis(seg.angleH, seg.referenceUpDir)
                * Quaternion.AngleAxis(seg.angleV, referenceRightDir)
                * seg.referenceLookDir;

            // Make look and up perpendicular
            Vector3 upDirGoal = seg.referenceUpDir;
            Vector3.OrthoNormalize(ref lookDirGoal, ref upDirGoal);

            // Interpolated look and up directions in neck parent space
            Vector3 lookDir = lookDirGoal;
            // Question: why are we slerping here instead of lerping? - NM
            seg.dirUp = 
                Vector3.Slerp(seg.dirUp, upDirGoal, Time.deltaTime * 5);
            Vector3.OrthoNormalize(ref lookDir, ref seg.dirUp);

            // Look rotation in world space
            Quaternion lookRot = 
                (parentRot * Quaternion.LookRotation(lookDir, seg.dirUp))
                * Quaternion.Inverse(
                    parentRot * Quaternion.LookRotation(
                        seg.referenceLookDir, 
                        seg.referenceUpDir));

            // Distribute rotation over all joints in segment
            Quaternion dividedRotation =
                Quaternion.Slerp(
                    Quaternion.identity, 
                    lookRot, 
                    effect / seg.chainLength);

            // Apply the calculated values to the actual transforms of the model
            t = seg.lastTransform;
            for (int i = 0; i < seg.chainLength; i++)
            {
                // Calculate the SLERP-ed rotation value, and then set the appropriate 
                // animation curve. Use the wrapper's tempRotation instead.
                Quaternion slerpedRot;
                slerpedRot = dividedRotation * t.rotation;
                t.rotation = slerpedRot;
                t = t.parent;
            }
        }

        // Handle non affected joints
        for (int i = 0; i < nonAffectedJoints.Length; i++)
        {
            Vector3 newJointDirection = Vector3.zero;

            if (nonAffectedJoints[i].joint.GetChildCount() > 0)
            {
                Transform child = nonAffectedJoints[i].joint.GetChild(0);
                newJointDirection = child.position - nonAffectedJoints[i].joint.position;
            }

            Vector3 combinedJointDirection = 
                Vector3.Slerp(
                    jointDirections[i], 
                    newJointDirection, 
                    nonAffectedJoints[i].effect);

            nonAffectedJoints[i].joint.rotation = 
                Quaternion.FromToRotation(
                    newJointDirection, 
                    combinedJointDirection) 
                * nonAffectedJoints[i].joint.rotation;
        }
    }

    float CalculateAngle(BendingSegment seg, float angle)
    {
        // Handle threshold angle difference, bending multiplier,
        // and max angle difference here
        float hAngleThr =
            Mathf.Max(
                0,
                Mathf.Abs(angle) - seg.thresholdAngleDifference)
            * Mathf.Sign(angle);

        angle =
            Mathf.Max(
                Mathf.Abs(hAngleThr) * Mathf.Abs(seg.bendingMultiplier),
                Mathf.Abs(angle) - seg.maxAngleDifference)
            * Mathf.Sign(angle)
            * Mathf.Sign(seg.bendingMultiplier);

        // If we're in restricted mode
        float maxBendingAngle = seg.maxBendingAngle;
        if (this.Restricted == true)
            maxBendingAngle = seg.restrictedBendingAngle;

        // Handle max bending angle here
        angle =
            Mathf.Clamp(
                angle,
                -maxBendingAngle,
                maxBendingAngle);

        return angle;
    }

    /// <summary>
    /// Finds the angle between dirA and dirB around axis. This function was in the original
    /// HeadlookController script.
    /// </summary>
    /// <param name="dirA">Direction vector A</param>
    /// <param name="dirB">Direction vector B</param>
    /// <param name="axis">Common axis of the direction vectors used to find the angles between the vectors</param>
    /// <returns>Angle between dirA and dirB around axis</returns>
    public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);

        // Return angle multiplied with 1 or -1
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }
    #endregion

    #region Messages
    /// <summary>
    /// Message for setting restricted mode to true or false
    /// </summary>
    void CmdSetRestricted(bool value)
    {
        this.Restricted = value;
    }

    /// <summary>
    /// Message for setting the headlook target
    /// </summary>
    void CmdSetHeadLookTarget(Vector3 target)
    {
        this.target = target;
    }
    #endregion
}