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

public class ShadowLeanControllerCompleted : ShadowController
{
    public Transform spine;

    public override void ControlledStart()
    {
        // Find the cloned version of the bone we were given in the inspector
        // so that we're editing our own shadow, not the display model
        this.spine = this.shadow.GetBone(this.spine);
    }

    public override void ControlledUpdate()
    {
        // Get the current euler angle rotation
        Vector3 rot = spine.rotation.eulerAngles;

        // Detect key input and add or subtract from the x rotation (scaling
        // by deltaTime to make this speed independent from the frame rate)
        if (Input.GetKey(KeyCode.R))
            rot.x -= Time.deltaTime * 50.0f;
        if (Input.GetKey(KeyCode.F))
            rot.x += Time.deltaTime * 50.0f;

        // Apply the new rotation
        spine.rotation = Quaternion.Euler(rot);
    }
}
