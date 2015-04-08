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

public abstract class IKSolver 
{
	public float epsilon = 0.001f;
    public int maxIterations = 100; // (If applicable)

    public abstract void Solve(IKJoint[] joints, Vector3 target);
    public abstract void Solve(
        IKJoint[] transforms,
        Transform endEffector,
        Vector3 target);

    public void Solve(
        Transform[] transforms,
        Transform endEffector,
        Vector3 target)
    {
        IKJoint[] joints = new IKJoint[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
            joints[i] = new IKJoint(transforms[i]);
        this.Solve(joints, endEffector, target);
    }

    public void Solve(Transform[] transforms, Vector3 target)
    {
        IKJoint[] joints = new IKJoint[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
            joints[i] = new IKJoint(transforms[i]);
        this.Solve(joints, target);
    }


}
