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

/// <summary>
/// A very simple shadow coordinator that expects only one
/// ShadowController, and gives it full control of the body
/// </summary>
public class DummyHeadlookCoordinator : ShadowCoordinator
{
    public float interp = 0.999f;

    [HideInInspector]
    public ShadowController dummyController;

    [HideInInspector]
    public ShadowController headlookController;


    void Start()
    {
        this.dummyController = this.shadowControllers["ShadowDummyController"];
        this.headlookController = this.shadowControllers["ShadowHeadLookController"];
    }

    void Update()
    {
        UpdateCoordinates();
        UpdateInterpolation();

        // Project the idle controller onto the headLook controller
        // each frame, since the headLook controller calculates a full
        // offset each frame from the base pose to the gazing pose
        headlookController.Decode(
            this.dummyController.Encode(
                this.NewTransformArray()));
        this.headlookController.ControlledUpdate();

        ShadowTransform[] dummy = 
            dummyController.Encode(this.NewTransformArray());
        ShadowTransform[] headLook =
            this.headlookController.Encode(this.NewTransformArray());

        ShadowTransform[] blend = BlendSystem.Blend(
            this.NewTransformArray(),
            new BlendPair(headLook, interp),
            new BlendPair(dummy, 1.0f - interp));

        Shadow.ReadShadowData(blend, transform.GetChild(0), this);
    }

    private void UpdateInterpolation()
    {
        if (Input.GetKey(KeyCode.Y))
            interp += Time.deltaTime * 2.0f;
        if (Input.GetKey(KeyCode.H))
            interp -= Time.deltaTime * 2.0f;
        interp = Mathf.Clamp(interp, 0.001f, 0.999f);
    }
}
