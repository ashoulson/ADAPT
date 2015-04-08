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

public class TutorialSteeringCoordinatorCompleted : ShadowCoordinator
{
    protected ShadowTransform[] buffer1 = null;
    protected ShadowLocomotionController loco = null;

    void Start()
    {
        // Allocate space for a buffer for storing and passing shadow poses
        this.buffer1 = this.NewTransformArray();

        // Get a reference to our lean ShadowController
        this.loco = this.GetComponent<ShadowLocomotionController>();

        // Call each ShadowController's ControlledStart() function
        this.ControlledStartAll();
    }

    void Update()
    {
        // Move the root position of each shadow to match the display model
        this.UpdateCoordinates();

        // Update the lean controller and write its shadow into the buffer
        this.loco.ControlledUpdate();
        this.loco.Encode(this.buffer1);

        // Write the shadow buffer to the display model, starting at the hips
        Shadow.ReadShadowData(
            this.buffer1,
            this.transform.GetChild(0),
            this);
    }
}
