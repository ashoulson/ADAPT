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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;

public sealed class BehaviorAgent : IBehaviorUpdate
{
    public delegate void OnStatusChanged(Status newStatus);
    public event OnStatusChanged statusChanged;

    private Status agentStatus;
    /// <summary>
    /// The status of the agent. Use the OnStatusChanged event to be notified
    /// of changes
    /// </summary>
    public Status AgentStatus 
    { 
        get
        {
            return this.agentStatus;
        }
        
        private set
        {
            this.agentStatus = value;
            if (this.statusChanged != null)
                this.statusChanged.Invoke(value);
        }
    }

    /// <summary>
    /// The priority that an event must exceed in order to be run on this
    /// agent. Reports zero if the agent isn't running any events.
    /// </summary>
    public float CurrentPriority
    {
        get
        {
            if (this.PendingEvent != null)
                return this.PendingEvent.Priority;
            if (this.CurrentEvent != null)
                return this.CurrentEvent.Priority;
            return 0.0f;
        }
    }

    public enum Status
    {
        Idle,        //< The agent is doing nothing
        Running,     //< The agent is running its own tree
        Terminating, //< The agent is terminating its own tree
        InEvent      //< The agent is suspended and locked by an event
    }

    /// <summary>
    /// The tree is final and can't be changed
    /// </summary>
    private readonly Node treeRoot = null;

    /// <summary>
    /// The event the agent is currently involved in, if any
    /// </summary>
    public BehaviorEvent CurrentEvent { get; private set; }

    private BehaviorEvent pendingEvent = null;
    /// <summary>
    /// Keep track of which event, if any, is interested in us
    /// </summary>
    public BehaviorEvent PendingEvent 
    {
        get
        {
            return this.pendingEvent;
        }

        set
        {
            if (this.pendingEvent != null
                && value != null
                && BehaviorEvent.ComparePriority(value, this.pendingEvent) <= 0)
                Debug.LogWarning("Replacing current evt with bad priority");
            this.pendingEvent = value;
        }
    }

    /// <summary>
    /// Block off the empty constructor
    /// </summary>
    private BehaviorAgent()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Constructs a new BehaviorAgent responsible for taking care of a tree
    /// </summary>
    /// <param name="root">The root node of the tree</param>
    /// <param name="statusChanged">An OnStatusChanged delegate for receiving 
    /// status change events</param>
    public BehaviorAgent(Node root, OnStatusChanged statusChanged)
    {
        this.treeRoot = root;
        this.pendingEvent = null;
        this.statusChanged = statusChanged;
        this.AgentStatus = Status.Idle;
        if (BehaviorManager.Instance == null)
            Debug.LogError(this + ": No BehaviorManager!");
        else
            BehaviorManager.RegisterReceiver(this);
    }

    public BehaviorAgent(Node root)
        : this(root, null) { }

    /// <summary>
    /// Returns true if and only if the candidate event is higher
    /// priority than both the running and next pending event, if any
    /// </summary>
    public bool EventElegible(BehaviorEvent candidate)
    {
        if (this.CurrentEvent != null
            && BehaviorEvent.ComparePriority(candidate, this.CurrentEvent) <= 0)
            return false;

        if (this.pendingEvent != null
            && BehaviorEvent.ComparePriority(candidate, this.pendingEvent) <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Activates the personal behavior tree
    /// </summary>
    private void TreeStart()
    {
        // We can't start our tree when terminating or in an event
        if (this.AgentStatus == Status.InEvent
            || this.AgentStatus == Status.Terminating)
            throw new ApplicationException(
                this + ".Activate(): AgentStatus is " + this.AgentStatus);
        // If we're already running, ignore the request
        else if (this.AgentStatus != Status.Running)
            this.treeRoot.Start();
        this.AgentStatus = Status.Running;
    }

    /// <summary>
    /// Terminates the personal behavior tree
    /// </summary>
    private RunStatus TreeTerminate()
    {
        // TODO: This doesn't handle termination failure very well, since we'll
        // report failure once and then switch to Idle and then report success
        // - AS
        if (this.AgentStatus == Status.InEvent)
            throw new ApplicationException(
                this + ".Terminate(): Agent is in an event");
        if (this.AgentStatus == Status.Idle)
            return RunStatus.Success;

        // If we finish terminating, switch our state to Idle
        RunStatus result = this.treeRoot.Terminate();
        if (result == RunStatus.Running)
            this.AgentStatus = Status.Terminating;
        else
            this.AgentStatus = Status.Idle;
        return result;
    }

    /// <summary>
    /// Notification that our pending event has become our current event
    /// </summary>
    public void EventStarted()
    {
        if (this.CurrentEvent != null)
            throw new ApplicationException(
                this + ".EventStarted(): Clearing active event");
        else if (this.AgentStatus != Status.Idle)
            throw new ApplicationException(
                this + ".EventStarted(): Starting evt on busy agent");

        this.CurrentEvent = this.pendingEvent;
        this.pendingEvent = null;
        this.AgentStatus = Status.InEvent;
    }

    /// <summary>
    /// Notification that the event has finished. Note that this doesn't
    /// restart the agent automatically.
    /// </summary>
    public void EventFinished()
    {
        if (this.CurrentEvent == null)
            throw new ApplicationException(
                this + ".EventFinished(): No active event");
        this.CurrentEvent = null;
        this.AgentStatus = Status.Idle;
    }

    /// <summary>
    /// External command for resuming autonomy, will always succeed
    /// unless an error is thrown (i.e. resuming while in an event
    /// or terminating)
    /// </summary>
    public void BehaviorStart()
    {
        this.TreeStart();
    }

    /// <summary>
    /// Tells the agent to suspend itself, reporting success or failure
    /// </summary>
    /// <returns>true if the agent is idle, false otherwise</returns>
    public bool BehaviorStop()
    {
        if (this.AgentStatus == Status.Idle)
            return true;
        // We do the actual termination in the behavior update
        this.AgentStatus = Status.Terminating;
        return false;
    }

    /// <summary>
    /// By default, ticks the internal tree if it's running
    /// </summary>
    bool IBehaviorUpdate.BehaviorUpdate(float deltaTime)
    {
        switch (this.AgentStatus)
        {
            case Status.Running:
                this.treeRoot.Tick();
                break;
            case Status.Terminating:
                RunStatus result = this.TreeTerminate();
                // TODO: Handle failure to terminate - AS
                if (result != RunStatus.Running)
                    this.AgentStatus = Status.Idle;
                break;
            // TODO: If we're idle, and we aren't running or waiting for
            // any events, should we restart automatically?
        }

        // TODO: For now, always return true so the manager never forgets
        // about updating its agents. We could make this more efficient
        // by forgetting about agents when they're in events and remembering
        // them when they're idle.
        return true;
    }
}
