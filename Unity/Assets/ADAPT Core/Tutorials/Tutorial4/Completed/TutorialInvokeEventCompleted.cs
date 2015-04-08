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
using TreeSharpPlus;
using System.Collections;

public class TutorialInvokeEventCompleted : MonoBehaviour 
{
    public Behavior Wanderer;
    public Behavior Friend;

    /// <summary>
    /// This subtree will cause the two characters to look at each other's
    /// heads. It will run indefinitely until terminated, at which point
    /// the characters will stop gaze tracking
    /// </summary>
    protected Node EyeContact(
        Val<Vector3> WandererPos, Val<Vector3> FriendPos)
    {
        // Estimate the head position based on height
        Vector3 height = new Vector3(0.0f, 1.85f, 0.0f);

        Val<Vector3> WandererHead = Val.Val(() => WandererPos.Value + height);
        Val<Vector3> FriendHead = Val.Val(() => FriendPos.Value + height);

        return new SequenceParallel(
            Friend.Node_HeadLook(WandererHead),
            Wanderer.Node_HeadLook(FriendHead));
    }

    protected Node Converse()
    {
        return new Sequence(
            Wanderer.Node_Gesture("acknowledging"),
            Friend.Node_Gesture("dismissing_gesture"),
            Wanderer.Node_Gesture("being_cocky"),
            Friend.Node_Gesture("lenghty_head_nod"));
    }

    protected Node EyeContactAndConverse(
        Val<Vector3> WandererPos, Val<Vector3> FriendPos)
    {
        return new Race(
            this.EyeContact(WandererPos, FriendPos),
            this.Converse());
    }

    protected Node ApproachAndOrient(
        Val<Vector3> WandererPos, Val<Vector3> FriendPos)
    {
        return new Sequence(
            // Approach at distance 1.0f
            Friend.Node_GoTo(WandererPos, 1.0f),
            new SequenceParallel(
                Friend.Node_OrientTowards(WandererPos),
                Wanderer.Node_OrientTowards(FriendPos)));
    }

    public Node ConversationTree()
    {
        Val<Vector3> WandererPos = Val.Val(() => Wanderer.transform.position);
        Val<Vector3> FriendPos = Val.Val(() => Friend.transform.position);

        return new Sequence(
            this.ApproachAndOrient(WandererPos, FriendPos),
            this.EyeContactAndConverse(WandererPos, FriendPos));
    }

	void Update() 
    {
        if (Input.GetKeyDown(KeyCode.R) == true)
            BehaviorEvent.Run(
                this.ConversationTree(), Wanderer, Friend);
	}
}
