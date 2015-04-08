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

public class ShadowRender : MonoBehaviour 
{
    public string shadowName;
    public GameObject rootObject;

    private ShadowCoordinator coordinator;
    private ShadowController controller;

	// Use this for initialization
	void Start () 
    {
        this.coordinator = rootObject.GetComponent<ShadowCoordinator>();
        this.controller = coordinator.GetController(shadowName);
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.position = rootObject.transform.position;
        transform.rotation = rootObject.transform.rotation;

        ShadowTransform[] encoded = this.coordinator.NewTransformArray();
        this.controller.Encode(encoded);
        Shadow.ReadShadowData(
            encoded, 
            transform.GetChild(0), 
            this.coordinator);
	}
}
