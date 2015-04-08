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

public class CursorChild : MonoBehaviour
{
    public enum Type
    {
        Reach,
        HeadLook
    }

    public Type type;
    public CursorParent parent;
    public string activateKey = "5";
    public bool locked = false;
    public Vector3 offset = Vector3.zero;
    public GameObject character;

    private Body bodyInterface = null;
    private ShadowReachController reach = null;
    private ShadowHeadLookController headLook = null;

    void Start()
    {
        this.bodyInterface = character.GetComponent<Body>();
        // If we're working with a reduced character, like in the HeadLook and
        // Reach demo, we might not have a BodyInterface, so we'll need to manually
        // fetch the right shadow controllers. In practice, you don't want to do this.
        if (this.bodyInterface == null)
        {
            this.reach = character.GetComponent<ShadowReachController>();
            this.headLook = character.GetComponent<ShadowHeadLookController>();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (this.activateKey.Length > 0 && Input.GetKeyDown(this.activateKey))
            this.locked = !this.locked;

        if (this.locked == false)
            this.transform.position = 
                this.parent.transform.position + this.offset;

        // If we have a full BodyInterface character, set the HeadLook
        // the "proper" way
        if (bodyInterface != null)
        {
            if (type == Type.HeadLook)
                bodyInterface.HeadLookSetTarget(transform.position);
            else
                bodyInterface.ReachSetTarget(transform.position);
        }
        // Otherwise, we have to do it the ugly, manual way
        else
        {
            if (type == Type.HeadLook)
                this.headLook.target = transform.position;
            else
                this.reach.target = transform.position;
        }
    }
}
