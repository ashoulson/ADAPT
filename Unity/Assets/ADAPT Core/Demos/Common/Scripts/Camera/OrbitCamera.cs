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

public class OrbitCamera : MonoBehaviour {
	
	public Transform target;
	public Transform cam;
	public Vector3 offset = Vector3.zero;
	private float cameraRotSide;
	private float cameraRotUp;
	private float cameraRotSideCur;
	private float cameraRotUpCur;
	private float distance;
	
	void Start () {
		cameraRotSide = transform.eulerAngles.y;
		cameraRotSideCur = transform.eulerAngles.y;
		cameraRotUp = transform.eulerAngles.x;
		cameraRotUpCur = transform.eulerAngles.x;
		distance = -cam.localPosition.z;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(1)) 
        {
			cameraRotSide += Input.GetAxis("Mouse X")*5;
			cameraRotUp -= Input.GetAxis("Mouse Y")*5;
		}
		cameraRotSideCur = Mathf.LerpAngle(cameraRotSideCur, cameraRotSide, Time.deltaTime*5);
		cameraRotUpCur = Mathf.Lerp(cameraRotUpCur, cameraRotUp, Time.deltaTime*5);
		
		distance *= (1-1*Input.GetAxis("Mouse ScrollWheel"));
		
		Vector3 targetPoint = target.position;
		transform.position = Vector3.Lerp(transform.position, targetPoint + offset, Time.deltaTime);
		transform.rotation = Quaternion.Euler(cameraRotUpCur, cameraRotSideCur, 0);
		
		float dist = Mathf.Lerp(-cam.transform.localPosition.z, distance, Time.deltaTime*5);
		cam.localPosition = -Vector3.forward * dist;
	}
}
