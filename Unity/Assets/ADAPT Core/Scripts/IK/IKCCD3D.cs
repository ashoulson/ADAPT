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

///
///  Original source code : http://www.darwin3d.com/gamedev/CCD3D.cpp
///  This class is ported on the original source code for Unity
/// 

public class IKCCD3D : IKSolver
{
    public bool damping = true;
    public float dampingMax = 0.001f;

    public override void Solve(
        IKJoint[] joints, 
        Transform endEffector, 
        Vector3 tarPos)
    {
        float damp = this.dampingMax * Time.deltaTime;
        int link = joints.Length - 1;
        Vector3 endPos = endEffector.position;

        // Cap out the number of iterations
        for (int tries = 0; tries < this.maxIterations; tries++)
        {
            // Are we there yet?
            if ((endPos - tarPos).sqrMagnitude <= epsilon)
                break;
            if (link < 0)
                link = joints.Length - 1;

            endPos = endEffector.position;

            Vector3 rootPos = joints[link].position;
            Vector3 currentDirection = (endPos - rootPos).normalized;
            Vector3 targetDirection = (tarPos - rootPos).normalized;
            float dot = Vector3.Dot(currentDirection, targetDirection);

            if (dot < (1.0f - epsilon))
            {
                float turnRad = Mathf.Acos(dot);
                if (damping == true && turnRad > damp)
                    turnRad = damp;
                float turnDeg = turnRad * Mathf.Rad2Deg;

                // Use the cross product to determine which way to rotate
                Vector3 cross =
                    Vector3.Cross(currentDirection, targetDirection);
                joints[link].rotation =
                    Quaternion.AngleAxis(turnDeg, cross)
                    * joints[link].rotation;
                joints[link].Constrain();
                joints[link].Relax(Time.deltaTime);
            }

            //// Move back in the array
            link--;
        }
    }

    public override void Solve(
        IKJoint[] joints,
        Vector3 tarPos)
    {
        this.Solve(joints, joints[joints.Length - 1].transform, tarPos);
    }
}