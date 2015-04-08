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

public class ShadowTransform
{
    public static bool IsValid(ShadowTransform t)
    {
        return (t != null && t.valid == true);
    }

    public bool valid = false;
    public Vector3 Position;
    public Quaternion Rotation;

    /// <summary>
    /// Shallow-clones a transform
    /// </summary>
    /// <param name="t">The transform to shallow copy</param>
    public ShadowTransform()
    {
        this.valid = false;
        this.Position = default(Vector3);
        this.Rotation = default(Quaternion);
    }

    public void ReadFrom(Transform t, bool valid = true)
    {
        this.Position = t.localPosition;
        this.Rotation = t.localRotation;
        this.valid = valid;
    }

    public void ReadFrom(Vector3 pos, Quaternion rot, bool valid = true)
    {
        this.Position = pos;
        this.Rotation = rot;
        this.valid = valid;
    }

    public void WriteTo(Transform t)
    {
        t.localPosition = this.Position;
        t.localRotation = this.Rotation;
    }
}
