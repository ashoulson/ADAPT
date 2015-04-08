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

/// <summary>
/// A very simple shadow coordinator that expects only one
/// ShadowController, and gives it full control of the body
/// </summary>
public class DummyCoordinator : ShadowCoordinator 
{
    private ShadowTransform[] buffer = null;
    private ShadowController controller = null;

	void Update() 
    {
        if (this.buffer == null)
            this.buffer = this.NewTransformArray();

        if (this.controller == null)
            foreach (ShadowController sc in this.shadowControllers.Values)
                this.controller = sc;

        UpdateCoordinates();
        this.controller.ControlledUpdate();
        this.controller.Encode(this.buffer);

        Shadow.ReadShadowData(
            this.buffer, 
            transform.GetChild(0), 
            this);
	}
}
