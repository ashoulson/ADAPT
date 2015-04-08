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

// Parameters for "Chuck"
// LeftArm
//    XRange
//       A 65
//       B 320
//       D 51
//       S 2
//    YRange
//       A 50
//       B 300
//       D 327
//       S 2
//    ZRange
//       A 50
//       B 230
//       D 327
//       S 2
//
// LeftForeArm
//    XRange
//       A 5
//       B 355
//       D 359
//       S 2
//    YRange
//       A 60
//       B 290
//       D 9
//       S 2
//    ZRange
//       A 350
//       B 200
//       D 354
//       S 2
//
// LeftHand
//    XRange
//       A 50
//       B 345
//       D 8
//       S 3
//    YRange
//       A 65
//       B 335
//       D 28
//       S 3
//    ZRange
//       A 20
//       B 340
//       D 5
//       S 3
//

public class ShadowReachController : ShadowController 
{
    /// <summary>
    /// Used for computing HasReached as a success evaluation
    /// </summary>
    public float maxReachDistance = 0.05f;

    public bool damping = true;
    public float dampingMax = 0.0014f;
    public IKJoint[] bones;
    public Vector3 target;
    public Transform endEffector = null;

    public bool HasReached
    {
        get
        {
            float d = (this.endEffector.position - this.target).sqrMagnitude;
            return (d < this.maxReachDistance);
        }
    }

    private IKCCD3D solver = null;

    public override void ControlledAwake()
    {
        this.solver = new IKCCD3D();
        this.solver.damping = this.damping;
        this.solver.dampingMax = this.dampingMax;

        // We have to make sure to convert the input transforms to the
        // corresponding transforms on the shadow, or else you'll see
        // strange artifacts
        for (int i = 0; i < bones.Length; i++)
            bones[i].transform = this.shadow.FindInShadow(bones[i].transform);

        // If the end effector isn't given, we assume it's the last bone
        if (this.endEffector != null)
            this.endEffector = this.shadow.FindInShadow(this.endEffector);
        else
            this.endEffector = this.bones[this.bones.Length - 1].transform;
    }

    // Update is called once per frame
    public override void ControlledUpdate()
    {
        solver.Solve(this.bones, this.endEffector, this.target);

        #if UNITY_EDITOR
        // Lets us change the damping settings during runtime in the editor
        this.solver.damping = this.damping;
        this.solver.dampingMax = this.dampingMax;
        #endif
    }

    #region Messages
    /// <summary>
    /// Sets the reach target position in global space
    /// </summary>
    void CmdSetReachTarget(Vector3 target)
    {
        this.target = target;
    }
    #endregion
}
