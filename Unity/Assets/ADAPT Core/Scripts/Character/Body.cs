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

/// <summary>
/// A very basic graphical interface for what should be treated as basic motor
/// skills performed by the character. These actions include no preconditions
/// and can fail if executed in impossible/nonsensical situations. In this
/// case, the functions will usually try their best.
/// 
/// Used with a BodyCoordinator and/or a SteeringController. Needs at least
/// one on the same GameObject to be able to do anything.
/// </summary>
public class Body : MonoBehaviour
{
    private Coordinator _coordinator = null;
    private SteeringController _steering = null;

    public Coordinator Coordinator
    {
        get
        {
            if (this._coordinator == null)
                throw new ApplicationException(
                    this.gameObject.name + ": No BodyCoordinator found!");
            return this._coordinator;
        }
    }

    public SteeringController Steering
    {
        get
        {
            if (this._steering == null)
                throw new ApplicationException(
                    this.gameObject.name + ": No SteeringController found!");
            return this._steering;
        }
    }

	// Use this for initialization
	void Awake() 
    {
        this._coordinator = this.gameObject.GetComponent<Coordinator>();
        this._steering = this.gameObject.GetComponent<SteeringController>();
	}

    #region Reach Commands
    /// <summary>
    /// (ADVANCED) Sets the active state of the reaching choreographer
    /// </summary>
    /// <param name="status"></param>
    public void ReachSetActive(bool status)
    {
        if (status == true)
            this.Coordinator.rWeight.ToMax();
        else
            this.Coordinator.rWeight.ToMin();
    }

    /// <summary>
    /// (ADVANCED) Sets the target coordinates for reaching
    /// </summary>
    public void ReachSetTarget(Vector3 target)
    {
        this.Coordinator.RelayMessage("CmdSetReachTarget", target);
    }

    /// <summary>
    /// (STATUS) Returns the current reach target.
    /// </summary>
    public Vector3 ReachTarget()
    {
        return this.Coordinator.reach.target;
    }

    /// <summary>
    /// (SIMPLE) Command to reach for a target position, the easiest way
    /// to activate reach functionality. Combines SetTarget and SetActive.
    /// </summary>
    public void ReachFor(Vector3 target)
    {
        this.ReachSetActive(true);
        this.ReachSetTarget(target);
    }

    /// <summary>
    /// (SIMPLE) Command to stop reaching. Based on SetActive.
    /// </summary>
    public void ReachStop()
    {
        this.ReachSetActive(false);
    }

    /// <summary>
    /// (STATUS) Returns true if and only if the end effector is close enough 
    /// to the goal.
    /// </summary>
    public bool ReachHasReached()
    {
        // TODO: Kind of a clumsy way to do it. Maybe we should check the 
        //       hand position of the display model instead? - AS
        return this.Coordinator.reach.HasReached 
            && this.Coordinator.rWeight.IsMax;
    }
    #endregion

    #region HeadLook Commands
    /// <summary>
    /// (ADVANCED) Sets the active state of the HeadLook choreographer.
    /// </summary>
    public void HeadLookSetActive(bool status)
    {
        if (status == true)
            this.Coordinator.hWeight.ToMax();
        else
            this.Coordinator.hWeight.ToMin();
    }

    /// <summary>
    /// (ADVANCED) Sets the goal position of the HeadLook choreographer.
    /// </summary>
    public void HeadLookSetTarget(Vector3 target)
    {
        this.Coordinator.RelayMessage("CmdSetHeadLookTarget", target);
    }

    /// <summary>
    /// (SIMPLE) Command to look at a target point. Combines SetTarget and
    /// SetActive.
    /// </summary>
    public void HeadLookAt(Vector3 target)
    {
        this.HeadLookSetActive(true);
        this.HeadLookSetTarget(target);
    }

    /// <summary>
    /// (SIMPLE) Stops gaze tracking. Based on SetActive.
    /// </summary>
    public void HeadLookStop()
    {
        this.HeadLookSetActive(false);
    }
    #endregion

    #region Animation Commands
    /// <summary>
    /// (SIMPLE) Begins playing the named animation on the upper body.
    /// </summary>
    public void AnimPlay(string name)
    {
        this.Coordinator.RelayMessage("CmdStartAnimation", name);
        this.Coordinator.aWeight.ToMax();
    }

    /// <summary>
    /// (SIMPLE) Fades out the current animation while playing.
    /// </summary>
    public void AnimStop()
    {
        this.Coordinator.aWeight.ToMin();
    }

    /// <summary>
    /// (SIMPLE) Immediately stops the current animation
    /// </summary>
    public void AnimStopImmediate()
    {
        this.Coordinator.RelayMessage("CmdStopAnimation"); 
        this.Coordinator.aWeight.ForceMin();
    }

    /// <summary>
    /// (STATUS) Returns true if and only if a gesture animation is playing.
    /// </summary>
    public bool AnimIsPlaying()
    {
        return this.Coordinator.anim.IsPlaying();
    }

    /// <summary>
    /// (STATUS) Returns true if and only if a gesture animation is playing.
    /// </summary>
    public bool AnimIsPlaying(string name)
    {
        return this.Coordinator.anim.IsPlaying(name);
    }
    #endregion

    #region Sitting Commands
    /// <summary>
    /// (SIMPLE) Sits the character down. Note that this will not interrupt
    /// the character's navigation if the character is still walking
    /// somewhere.
    /// </summary>
    public void SitDown()
    {
        this.Coordinator.RelayMessage("CmdSitDown");
        this.Coordinator.sWeight.ToMax();
    }

    /// <summary>
    /// (SIMPLE) Stands the character up. Note that this will not interrupt
    /// the character's navigation if the character is still walking
    /// somewhere.
    /// </summary>
    public void StandUp()
    {
        this.Coordinator.RelayMessage("CmdStandUp");
    }

    /// <summary>
    /// (STATUS) Returns true if and only if the character is definitely
    /// sitting.
    /// </summary>
    public bool IsSitting()
    {
        return this.Coordinator.sitting.IsSitting 
            && this.Coordinator.sWeight.IsMax;
    }

    /// <summary>
    /// (STATUS) Returns true if and only if the character is definitely 
    /// standing.
    /// </summary>
    public bool IsStanding()
    {
        return this.Coordinator.sitting.IsStanding
            && this.Coordinator.sWeight.IsMax;
    }
    #endregion

    #region Navigation Commands
    public float NavStopRadius
    {
        get { return this.Steering.stoppingRadius; }
        set { this.Steering.stoppingRadius = value; }
    }

    public float NavArriveRadius
    {
        get { return this.Steering.arrivingRadius; }
        set { this.Steering.arrivingRadius = value; }
    }

    /// <summary>
    /// (SIMPLE) Sets the navigation target for a character. Note that this 
    /// will move the character even if the character is sitting.
    /// </summary>
    public void NavGoTo(Vector3 target)
    {
        this.Steering.Target = target;
    }

    /// <summary>
    /// (SIMPLE) Stops the character while navigating.
    /// </summary>
    /// <param name="sticky">Use this if you want a more abrupt stop.</param>
    public void NavStop()
    {
        this.Steering.Stop();
    }

    /// <summary>
    /// (STATUS) Returns true if and only if the character is below a very 
    /// small velocity.
    /// </summary>
    public bool NavIsStopped()
    {
        return this.Steering.IsStopped();
    }

    /// <summary>
    /// (STATUS) Returns true if and only if the character is very close to the goal.
    /// </summary>
    public bool NavIsAtTarget()
    {
        return this.Steering.IsAtTarget();
    }

    /// <summary>
    /// (STATUS) Combines IsStopped and IsAtTarget.
    /// </summary>
    public bool NavHasArrived()
    {
        return this.Steering.HasArrived();
    }

    /// <summary>
    /// (STATUS) Queries a path to a given navigation target.
    /// </summary>
    public bool NavCanReach(Vector3 target)
    {
        return this.Steering.CanReach(target);
    }

    /// <summary>
    /// (STATUS) Returns the current navigation target.
    /// </summary>
    public Vector3 NavTarget()
    {
        return this.Steering.Target;
    }

    /// <summary>
    /// (STATUS) Returns true if and only if we are facing our desired
    /// orientation.
    /// </summary>
    public bool NavIsFacingDesired()
    {
        return this.Steering.IsFacing();
    }

    /// <summary>
    /// (SIMPLE) Sets a goal orientation to face while walking. Note that this
    /// will only take effect if the orientation behavior is set to
    /// OrientationBehavior.None
    /// </summary>
    public void NavSetDesiredOrientation(Vector3 target)
    {
        this.Steering.SetDesiredOrientation(target);
    }

    /// <summary>
    /// (SIMPLE) Sets a goal orientation to face while walking. Note that this
    /// will only take effect if the orientation behavior is set to
    /// OrientationBehavior.None
    /// </summary>
    public void NavSetDesiredOrientation(Quaternion desired)
    {
        this.Steering.desiredOrientation = desired;
    }

    /// <summary>
    /// (SIMPLE) Allows you to configure the automatic orientation behavior, 
    /// if any
    /// </summary>
    public void NavSetOrientationBehavior(OrientationBehavior behavior)
    {
        this.Steering.orientationBehavior = behavior;
    }

    /// <summary>
    /// (ADVANCED) Attaches or detaches the character from the navmesh. Only 
    /// use this if you know what you're doing.
    /// </summary>
    public void NavSetAttached(bool value)
    {
        this.Steering.Attached = value;
    }

    ///// <summary>
    ///// (SIMPLE) Moves a character instantly. Note that the character will
    ///// remain within the bounds of the navigation mesh.
    ///// </summary>
    //public void NavTranslate(Vector3 translation)
    //{
    //    this.Steering.Move(translation);
    //}
    #endregion
}   
