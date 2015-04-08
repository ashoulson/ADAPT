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
/// A coordinator for blending locomotion and a ragdoll
/// </summary>
public class DummyRagdollCoordinator : ShadowCoordinator
{
    public Slider dWeight = null;

    private ShadowRagdollController ragdoll = null;
    private ShadowController locomotion = null;

    private ShadowTransform[] ragdollPose = null;
    private ShadowTransform[] locomotionPose = null;
	
    void Awake()
    {
        CharacterController cc = GetComponent<CharacterController>();
        cc.collider.isTrigger = true;
        this.dWeight = new Slider(2.0f);
        this.ragdollPose = this.NewTransformArray();
        this.locomotionPose = this.NewTransformArray();
    }

	void Update()
    {
        if (this.ragdoll == null)
            this.ragdoll =
                this.shadowControllers["ShadowRagdollController"]
                as ShadowRagdollController;
        if (this.locomotion == null)
            this.locomotion = 
                this.shadowControllers["ShadowLocomotionController"];

        this.dWeight.Tick(Time.deltaTime);

        // Set all of the shadows' root transform positions and orientations
        // to the real root position and orientation
        UpdateCoordinates();

        this.locomotion.ControlledUpdate();
        this.locomotion.Encode(this.locomotionPose);

        // Special management of the ragdoll controller for telling it
        // that it's fully faded out and done falling
        if (this.dWeight.IsMin == true)
            this.ragdoll.IsFalling = false;

        // Reuse the locomotion pose buffer
        this.locomotionPose = this.BlendRagdoll(this.locomotionPose);
        Shadow.ReadShadowData(
            this.locomotionPose,
            hips, 
            this);
    }

    private ShadowTransform[] BlendRagdoll(ShadowTransform[] input)
    {
        if (this.dWeight.IsMin == true)
            this.ragdoll.Decode(
                input,
                new Blacklist<string>("LeftUpLeg", "RightUpLeg"));
        this.ragdoll.ControlledUpdate();
        ShadowTransform[] result
            = this.ragdoll.Encode(this.ragdollPose);

        return BlendSystem.Blend(
            this.NewTransformArray(),
            new BlendPair(input, this.dWeight.Inverse),
            new BlendPair(result, this.dWeight.Value));
    }

    #region Controller Events
    void EvtBeginFalling() { this.dWeight.ForceMax(); }
    void EvtDoneFalling() { this.dWeight.ToMin(); }
    #endregion
}
