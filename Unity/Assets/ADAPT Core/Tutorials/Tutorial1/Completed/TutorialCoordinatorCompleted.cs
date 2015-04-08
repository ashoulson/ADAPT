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
public class TutorialCoordinatorCompleted : ShadowCoordinator
{
    protected ShadowTransform[] buffer1 = null;
    protected ShadowTransform[] buffer2 = null;
    protected ShadowLeanControllerCompleted lean = null;
    protected ShadowAnimationController anim = null;
    protected Slider weight;

    void Start()
    {
        // Allocate space for two buffers for storing and passing shadow poses
        this.buffer1 = this.NewTransformArray();
        this.buffer2 = this.NewTransformArray();

        // Get a reference to our lean ShadowController
        this.lean = this.GetComponent<ShadowLeanControllerCompleted>();

        // Get a reference to our animation ShadowController
        this.anim = this.GetComponent<ShadowAnimationController>();

        // Set the weight
        this.weight = new Slider(4.0f);

        // Call each ShadowController's ControlledStart() function
        this.ControlledStartAll();
    }

    void Update()
    {
        this.weight.Tick(Time.deltaTime);

        // Move the root position of each shadow to match the display model
        this.UpdateCoordinates();

        // Update the lean controller and write its shadow into the buffer
        this.lean.ControlledUpdate();
        this.lean.Encode(this.buffer1);

        // Update the anim controller and write its shadow into the buffer
        this.anim.ControlledUpdate();
        this.anim.Encode(this.buffer2, new Whitelist<string>("Spine1"));

        // Optionally, uncomment this to see the weight value
        // Debug.Log(weight);

        // Play an animation when we press T
        if (Input.GetKeyDown(KeyCode.T) == true)
        {
            this.anim.AnimPlay("dismissing_gesture");
            this.weight.ToMin();
        }

        // Fade out the animation controller if we're finished
        if (anim.IsPlaying() == false)
            this.weight.ToMax();

        // Blend the two controllers using the weight value
        BlendSystem.Blend(
            this.buffer1,
            new BlendPair(this.buffer1, this.weight.Value),
            new BlendPair(this.buffer2, this.weight.Inverse));

        // Write the shadow buffer to the display model, starting at the hips
        Shadow.ReadShadowData(
            this.buffer1,
            this.transform.GetChild(0),
            this);
    }
}
