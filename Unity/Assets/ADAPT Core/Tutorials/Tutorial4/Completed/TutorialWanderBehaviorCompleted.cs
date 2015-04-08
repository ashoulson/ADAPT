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

using System;
using UnityEngine;
using TreeSharpPlus;
using System.Collections;

public class TutorialWanderBehaviorCompleted : Behavior 
{
    public Transform wander1;
    public Transform wander2;
    public Transform wander3;

    protected Node ST_ApproachAndWait(Transform target)
    {
        Val<Vector3> position = Val.Val(() => target.position);

        return new Sequence(
            //new LeafTrace("Going to: " + position.Value),
            this.Node_GoTo(position),
            new LeafWait(1000));
    }

    protected Node BuildTreeRoot()
    {
        return
            new DecoratorLoop(
                new DecoratorForceStatus(RunStatus.Success,
                    new SequenceShuffle(
                        ST_ApproachAndWait(this.wander1),
                        ST_ApproachAndWait(this.wander2),
                        ST_ApproachAndWait(this.wander3))));
    }

	// Use this for initialization
	void Start() 
    {
        base.StartTree(this.BuildTreeRoot());
	}
}
