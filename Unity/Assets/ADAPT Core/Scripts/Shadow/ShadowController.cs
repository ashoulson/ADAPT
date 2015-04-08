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
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ShadowCoordinator))]
public abstract class ShadowController : MonoBehaviour 
{
    public virtual void ControlledAwake() { }
    public virtual void ControlledStart() { }
    public virtual void ControlledUpdate() { }
    public virtual void ControlledFixedUpdate() { }
    public virtual void ControlledLateUpdate() { }

    public Shadow shadow = null;
    public bool showGizmo = true;

    private ShadowCoordinator _coordinator = null;
    public ShadowCoordinator Coordinator { get { return this._coordinator; } }

    // Ignore these objects in the base character model when
    // cloning a shadow for this controller
    public string[] ignoreBones = { };

    new public Transform transform { get { return this.shadow.transform; } }
    new public Animation animation { get { return this.shadow.animation; } }

    void Awake()
    {
        if (this.enabled == true)
        {
            this._coordinator =
                this.gameObject.GetComponent<ShadowCoordinator>();
            this._coordinator.RegisterController(this);
        }
    }

    public ShadowTransform[] Encode(ShadowTransform[] buffer)
    {
        return this.shadow.Encode(buffer);
    }

    public ShadowTransform[] Encode(
        ShadowTransform[] buffer, 
        FilterList<string> nameFilter)
    {
        return this.shadow.Encode(buffer, nameFilter);
    }

    public void Decode(ShadowTransform[] data)
    {
        this.shadow.Decode(data);
    }

    public void Decode(
        ShadowTransform[] data,
        FilterList<string> nameFilter)
    {
        this.shadow.Decode(data, nameFilter);
    }

    /// <summary>
    /// Gets a bone by name
    /// </summary>
    public Transform GetBone(string name)
    {
        return this.shadow.GetBone(name);
    }

    /// <summary>
    /// Gets a bone by matching transform
    /// </summary>
    public Transform GetBone(Transform t)
    {
        return this.shadow.GetBone(t);
    }

    // This is just here so we can enable or 
    // disable the script from the inspector
    void Update() { }
}
