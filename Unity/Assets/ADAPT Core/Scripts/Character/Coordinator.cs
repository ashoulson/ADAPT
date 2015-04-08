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

public class Coordinator : ShadowCoordinator
{
    public Transform midSpine = null;
    public Transform reachArm = null;
    public Transform leftHip = null;
    public Transform rightHip = null;

    // Interpolation parameters
    public Slider sWeight = null;
    public Slider aWeight = null;
    public Slider hWeight = null;
    public Slider rWeight = null;
    public Slider dWeight = null;

    private Vector3 oldPosition = Vector3.zero;

    [HideInInspector]
    public ShadowSittingController sitting = null;
    [HideInInspector]
    public ShadowLocomotionController locomotion = null;
    [HideInInspector]
    public ShadowAnimationController anim = null;
    [HideInInspector]
    public ShadowHeadLookController headLook = null;
    [HideInInspector]
    public ShadowReachController reach = null;
    [HideInInspector]
    public ShadowRagdollController ragdoll = null;  

    void Awake()
    {
        this.sWeight = new Slider(2.0f);
        this.aWeight = new Slider(2.0f);
        this.hWeight = new Slider(2.0f);
        this.rWeight = new Slider(2.0f);
        this.dWeight = new Slider(2.0f);

        this.sitting = this.GetComponent<ShadowSittingController>();
        this.locomotion = this.GetComponent<ShadowLocomotionController>();
        this.anim = this.GetComponent<ShadowAnimationController>();
        this.headLook = this.GetComponent<ShadowHeadLookController>();
        this.reach = this.GetComponent<ShadowReachController>();
        this.ragdoll = this.GetComponent<ShadowRagdollController>();
    }

    /// <summary>
    /// A rather ugly, complicated update and blend pipeline for four controllers
    /// </summary>
    void Update()
    {
        this.sWeight.Tick(Time.deltaTime);
        this.aWeight.Tick(Time.deltaTime);
        this.hWeight.Tick(Time.deltaTime);
        this.rWeight.Tick(Time.deltaTime);
        this.dWeight.Tick(Time.deltaTime);

        // Set all of the shadows' root transform positions and orientations
        // to the real root position and orientation
        this.UpdateCoordinates();

        // This tells the headlook controller to go into "restricted" mode
        float speed = (this.oldPosition - transform.position).sqrMagnitude;
        if (speed > 0.0001f)
            this.RelayMessage("CmdSetRestricted", true);
        else
            this.RelayMessage("CmdSetRestricted", false);
        this.oldPosition = transform.position;

        ShadowTransform[] lg = this.BlendLegsAndSitting();
        ShadowTransform[] anim = this.BlendAnimations(lg);
        ShadowTransform[] head = this.BlendHeadLook(anim);
        ShadowTransform[] reach = this.BlendReach(head);
        ShadowTransform[] rag = this.BlendRagdoll(reach);

        // Special management of the ragdoll controller for telling it
        // that it's fully faded out and done falling
        if (this.dWeight.IsMin == true)
            this.ragdoll.IsFalling = false;

        Shadow.ReadShadowData(rag, this.hips, this);
    }

    private ShadowTransform[] BlendLegsAndSitting()
    {
        // Update the leg controller
        this.locomotion.ControlledUpdate();
        ShadowTransform[] legs =
            this.locomotion.Encode(this.NewTransformArray());

        // If we don't need to blend the gesture controller, don't bother
        if (sWeight.IsMin == true)
            return legs;

        this.sitting.ControlledUpdate();
        ShadowTransform[] sitBody = 
            this.sitting.Encode(this.NewTransformArray());

        return BlendSystem.Blend(
            this.NewTransformArray(),
            new BlendPair(legs, sWeight.Inverse),
            new BlendPair(sitBody, sWeight.Value));
    }

    private ShadowTransform[] BlendController(
        ShadowController controller,
        ShadowTransform[] input,
        Slider weight,
        FilterList<string> filter = null)
    {
        if (weight.IsMin == true)
            return input;

        // Update the target controller from that blend
        if (filter == null)
            controller.Decode(input);
        else
            controller.Decode(input, filter);
        controller.ControlledUpdate();
        ShadowTransform[] result 
            = controller.Encode(this.NewTransformArray());

        return BlendSystem.Blend(
            this.NewTransformArray(),
            new BlendPair(input, weight.Inverse),
            new BlendPair(result, weight.Value));
    }

    private ShadowTransform[] BlendAnimations(ShadowTransform[] input)
    {
        return BlendController(
            this.anim,
            input,
            this.aWeight,
            // We want to filter out the upper body from the sitting
            // and locomotion blend when we're doing the animation on top
            new Blacklist<string>(this.midSpine.name));
    }

    private ShadowTransform[] BlendHeadLook(ShadowTransform[] input)
    {
        return BlendController(
            this.headLook,
            input,
            this.hWeight);
    }

    private ShadowTransform[] BlendReach(ShadowTransform[] input)
    {
        return BlendController(
            this.reach,
            input,
            this.rWeight,
            new Blacklist<string>(this.reachArm.name));
    }

    private ShadowTransform[] BlendRagdoll(ShadowTransform[] input)
    {
        if (this.dWeight.IsMin == true)
            this.ragdoll.Decode(
                input, 
                new Blacklist<string>(this.leftHip.name, this.rightHip.name));
        this.ragdoll.ControlledUpdate();
        ShadowTransform[] result
            = this.ragdoll.Encode(this.NewTransformArray());

        return BlendSystem.Blend(
            this.NewTransformArray(),
            new BlendPair(input, this.dWeight.Inverse),
            new BlendPair(result, this.dWeight.Value));
    }

    #region Controller Events
    void EvtDoneStanding() { sWeight.ToMin(); }
    void EvtDoneAnimation() { aWeight.ToMin(); }
    void EvtBeginFalling() { this.dWeight.ForceMax(); }
    void EvtDoneFalling() { this.dWeight.ToMin(); }
    #endregion
}
