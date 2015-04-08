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
using TreeSharpPlus;

public class Behavior<T> : MonoBehaviour, IBehavior
    where T : MonoBehaviour, ICharacter
{
    [HideInInspector]
    public T Character = null;

    [HideInInspector]
    public BehaviorAgent Agent { get; private set; }

    void Awake() { this.Initialize(); }

    protected void Initialize() 
    {
        this.Character = this.GetComponent<T>(); 
    }

    protected void StartTree(
        Node root, 
        BehaviorAgent.OnStatusChanged statusChanged = null)
    {
        this.Agent = new BehaviorAgent(root, statusChanged);
        this.Agent.BehaviorStart();
    }

    #region Helper Nodes
    public Node Node_Gesture(Val<string> name)
    {
        return new LeafInvoke(
            () => this.Character.Gesture(name),  // Play the gesture
            () => this.Character.GestureStop()); // Stop if we're terminated
    }

    public Node Node_GoTo(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.NavGoTo(targ), // Approach the target
            () => this.Character.NavStop());    // Stop if we're terminated
    }

    /// <summary>
    /// Approaches a target at a given radius
    /// </summary>
    public Node Node_GoTo(Val<Vector3> targ, Val<float> dist)
    {
        Func<Vector3> position =
            delegate()
            {
                Vector3 targPos = targ.Value;
                Vector3 direction =
                    (targPos - transform.position).normalized;
                return targPos - (direction * dist.Value);
            };

        return new LeafInvoke(
            () => this.Character.NavGoTo(position),
            () => this.Character.NavStop());
    }

    public Node Node_Reach(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.ReachFor(targ),
            () => this.Character.ReachStop());
    }

    public Node Node_HeadLook(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.HeadLook(targ),
            () => this.Character.HeadLookStop());
    }

    public Node Node_OrientTowards(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.NavTurn(targ),
            () => this.Character.NavOrientBehavior(
                OrientationBehavior.LookForward));
    }
    #endregion

    #region Reusable Subtrees
    public Node ST_TurnToFace(Val<Vector3> target)
    {
        Func<RunStatus> turn =
            () => this.Character.NavTurn(target);

        Func<RunStatus> stopTurning =
            () => this.Character.NavOrientBehavior(
                OrientationBehavior.LookForward);

        return
            new Sequence(
                new LeafInvoke(turn, stopTurning));
    }
    #endregion
}

public class Behavior : Behavior<Character> { }
