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

public class IK1JointAnalytic : IKSolver {

    public override void Solve(IKJoint[] joints, Vector3 target) 
    {
        Transform hip = joints[0].transform;
        Transform knee = joints[1].transform;
        Transform ankle = joints[2].transform;
		
		// Calculate the direction in which the knee should be pointing
		Vector3 vKneeDir = Vector3.Cross(
			ankle.position - hip.position,
			Vector3.Cross(
				ankle.position-hip.position,
				ankle.position-knee.position
			)
		);
		
		// Get lengths of leg bones
		float fThighLength = (knee.position-hip.position).magnitude;
		float fShinLength = (ankle.position-knee.position).magnitude;
		
		// Calculate the desired new joint positions
		Vector3 pHip = hip.position;
		Vector3 pAnkle = target;
		Vector3 pKnee = findKnee(pHip,pAnkle,fThighLength,fShinLength,vKneeDir);
		
		// Rotate the bone transformations to align correctly
		Quaternion hipRot = Quaternion.FromToRotation(knee.position-hip.position, pKnee-pHip) * hip.rotation;
		if (System.Single.IsNaN(hipRot.x)) {
			Debug.LogWarning("hipRot="+hipRot+" pHip="+pHip+" pAnkle="+pAnkle+" fThighLength="+fThighLength+" fShinLength="+fShinLength+" vKneeDir="+vKneeDir);
		}
		else {
			hip.rotation = hipRot;
			knee.rotation = Quaternion.FromToRotation(ankle.position-knee.position, pAnkle-pKnee) * knee.rotation;
		}
		
	}
	
	public Vector3 findKnee(Vector3 pHip, Vector3 pAnkle, float fThigh, float fShin, Vector3 vKneeDir) {
		Vector3 vB = pAnkle-pHip;
		float LB = vB.magnitude;
		
		float maxDist = (fThigh+fShin)*0.999f;
		if (LB>maxDist) {
			// ankle is too far away from hip - adjust ankle position
			pAnkle = pHip+(vB.normalized*maxDist);
			vB = pAnkle-pHip;
			LB = maxDist;
		}
		
		float minDist = Mathf.Abs(fThigh-fShin)*1.001f;
		if (LB<minDist) {
			// ankle is too close to hip - adjust ankle position
			pAnkle = pHip+(vB.normalized*minDist);
			vB = pAnkle-pHip;
			LB = minDist;
		}
		
		float aa = (LB*LB+fThigh*fThigh-fShin*fShin)/2/LB;
		float bb = Mathf.Sqrt(fThigh*fThigh-aa*aa);
		Vector3 vF = Vector3.Cross(vB,Vector3.Cross(vKneeDir,vB));
		return pHip+(aa*vB.normalized)+(bb*vF.normalized);
	}

    public override void Solve(
        IKJoint[] transforms, 
        Transform endEffector, 
        Vector3 target)
    {
        throw new System.NotImplementedException();
    }
}
