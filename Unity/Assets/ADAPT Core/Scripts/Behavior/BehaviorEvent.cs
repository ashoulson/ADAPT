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
using System.Collections.Generic;
using TreeSharpPlus;

// Preparation phase:
// 1) If agents are running, suspend them - Wait until termination
//    If agents are involved in a lower-priority event, terminate that event - Wait until termination
//    -- Move to next phase when agents are not in an event and not running any trees
//    Issue 1: What if the agent moves from one event to another during this time?
//    
// 2) All agents are not running, and not involved in an event
//    -- Set each agent's current event to be us, begin running
//

public sealed class BehaviorEvent : IBehaviorUpdate
{
    private readonly float priority;
    public string Name = null;
    public float Priority { get { return this.priority; } }

    public static int ComparePriority(BehaviorEvent a, BehaviorEvent b)
    {
        return Comparer<float>.Default.Compare(a.Priority, b.Priority);
    }

    public delegate void OnStatusChanged(Status newStatus);
    public event OnStatusChanged statusChanged;

    private Status eventStatus;
    /// <summary>
    /// The status of the event. Use the OnStatusChanged event to be notified
    /// of changes
    /// </summary>
    public Status EventStatus
    {
        get
        {
            return this.eventStatus;
        }

        private set
        {
            this.eventStatus = value;
            if (this.statusChanged != null)
                this.statusChanged.Invoke(value);
        }
    }

    public enum Status
    {
        Initializing, //< The event is seeing if it is eligible for its agents
        Pending,      //< The event is waiting for its agents to be available
        Running,      //< The event is running and actively ticking
        Terminating,  //< The event is in the process of terminating
        Detaching,    //< The event is detaching from agents and cleaning up
        Finished,     //< The event has ended (success or failure)
        // TODO: Maybe a TerminateSuccess and TerminateFailure? - AS
    }

    /// <summary>
    /// The tree is final and can't be changed
    /// </summary>
    private readonly Node treeRoot = null;

    /// <summary>
    /// The agents we will have in this event
    /// </summary>
    private readonly BehaviorAgent[] involvedAgents;

    /// <summary>
    /// Block off the empty constructor
    /// </summary>
    private BehaviorEvent()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Constructs a BehaviorEvent responsible for maintaining a tree
    /// </summary>
    /// <param name="root">The root node of the tree</param>
    /// <param name="priority">The event's priority</param>
    /// <param name="statusChanged">An OnStatusChanged delegate for receiving 
    /// status change events</param>
    /// <param name="involvedAgents">The agents involved</param>
    public BehaviorEvent(
        Node root,
        float priority,
        OnStatusChanged statusChanged,
        string name = null,
        params IBehavior[] involved)
    {
        this.treeRoot = root;
        this.priority = priority;
        this.statusChanged = statusChanged;
        this.Name = name;
        this.EventStatus = Status.Initializing;
        if (BehaviorManager.Instance == null)
            Debug.LogError(this + ": No BehaviorManager!");
        else
            BehaviorManager.RegisterReceiver(this);

        this.involvedAgents = new BehaviorAgent[involved.Length];
        for (int i = 0; i < involvedAgents.Length; i++)
            involvedAgents[i] = involved[i].Agent;
    }

    public static BehaviorEvent Run(
        Node root,
        params IBehavior[] involvedAgents)
    {
        return new BehaviorEvent(root, 0.5f, null, null, involvedAgents);
    }

    public static BehaviorEvent Run(
        Node root,
        string name,
        params IBehavior[] involvedAgents)
    {
        return new BehaviorEvent(root, 0.5f, null, name, involvedAgents);
    }

    public static BehaviorEvent Run(
        Node root,
        float priority,
        string name = null,
        params IBehavior[] involvedAgents)
    {
        return new BehaviorEvent(root, priority, null, name, involvedAgents);
    }

    public static BehaviorEvent Run(
        Node root,
        OnStatusChanged statusChanged,
        string name = null,
        params IBehavior[] involvedAgents)
    {
        return new BehaviorEvent(root, 0.5f, statusChanged, name, involvedAgents);
    }

    /// <summary>
    /// Tells the event to stop itself, reporting success or failure
    /// </summary>
    /// <returns>true if the event is finished, false otherwise</returns>
    public bool EventKill()
    {
        if (this.EventStatus == Status.Finished)
            return true;

        // We do the actual termination in the behavior update
        if (this.EventStatus == Status.Initializing
            || this.EventStatus == Status.Pending
            || this.EventStatus == Status.Running)
            this.EventStatus = Status.Terminating;
        return false;
    }

    /// <summary>
    /// Injects this event as the next pending event for the
    /// given agent
    /// </summary>
    private void InjectEvent(BehaviorAgent agent)
    {
        // Either kill the active event or suspend the agent
        if (agent.CurrentEvent != null)
            agent.CurrentEvent.EventKill();
        else
            agent.BehaviorStop();

        // Kill any pending event and just replace it
        if (agent.PendingEvent != null)
            agent.PendingEvent.EventKill();
        agent.PendingEvent = this;
    }

    /// <summary>
    /// Kills either the agent's personal tree or current event,
    /// returning true iff the agent is ready to run this event
    /// </summary>
    private bool PrepareAgent(BehaviorAgent agent)
    {
        if (agent.CurrentEvent == null)
            return agent.BehaviorStop();
        return agent.CurrentEvent.EventKill();
        // TODO: For debugging, might want to make sure the agent isn't
        // running both an event and its own tree. - AS
    }

    /// <summary>
    /// Starts the event, and sets the current/pending event values properly
    /// for each involved agent
    /// </summary>
    private void EventStart()
    {
        foreach (BehaviorAgent agent in this.involvedAgents)
            if (agent.PendingEvent == this)
                agent.EventStarted();
            else
                throw new ApplicationException(
                    this + ".EventStart(): Starting a non-pending event");

        this.treeRoot.Start();
    }

    /// <summary>
    /// Removes this event from the agent, restoring autonomy if appropriate
    /// </summary>
    /// <returns>true if the event is successfully detached</returns>
    private bool DetachFromAgent(BehaviorAgent agent)
    {
        // If we were the agent's current event, restore autonomy (even if the
        // agent has another pending event -- that event will just stop it)
        if (agent.CurrentEvent == this)
        {
            agent.EventFinished();
            agent.BehaviorStart();
        }

        // If we were a pending event, then the response depends
        if (agent.PendingEvent == this)
        {
            // Was the agent terminating because of us? If so, wait until it's
            // done terminating, and then restart it
            if (agent.AgentStatus == BehaviorAgent.Status.Terminating)
                return false;

            // If the agent wasn't terminating, restart it if it's idle and then
            // clear the pending event
            if (agent.AgentStatus == BehaviorAgent.Status.Idle)
                agent.BehaviorStart();
            agent.PendingEvent = null;
            // Don't worry if another pending event swoops in and replaces us,
            // it'll handle the agent whether its terminating, running, or idle
        }

        return true;
    }

    /// <summary>
    /// First checks eligibility of the event, and then registers as a
    /// pending event for each involved agent. If we aren't eligible,
    /// this kills the event.
    /// </summary>
    private void Initializing()
    {
        // Is any agent in an event or waiting for an event
        // with higher priority than us?
        foreach (BehaviorAgent agent in this.involvedAgents)
        {
            if (agent.EventElegible(this) == false)
            {
                this.EventKill();
                return;
            }
        }

        // Every agent can run us now, so register as a pending event
        foreach (BehaviorAgent agent in this.involvedAgents)
            this.InjectEvent(agent);

        // We're good to go, now just wait for agents to be ready
        this.EventStatus = Status.Pending;
    }

    /// <summary>
    /// Checks to see if all of the agents are ready to load us, and then
    /// starts the event if that's the case
    /// </summary>
    private void Pending()
    {
        bool ready = true;
        foreach (BehaviorAgent agent in this.involvedAgents)
            if (this.PrepareAgent(agent) == false)
                ready = false;

        // If everybody is ready, start the tree and transition
        if (ready == true)
        {
            this.EventStart();
            this.EventStatus = Status.Running;
        }
    }

    /// <summary>
    /// Handles ticking the tree, switching to detaching if the tree finishes
    /// </summary>
    private void Running()
    {
        // TODO: Handle/report failure - AS
        if (this.treeRoot.Tick() != RunStatus.Running)
            this.EventStatus = Status.Detaching;
    }

    /// <summary>
    /// Handles terminating the tree, switching to detaching if the tree finishes
    /// </summary>
    private void Terminating()
    {
        // TODO: Handle/report failure
        if (this.treeRoot.Terminate() != RunStatus.Running)
            this.EventStatus = Status.Detaching;
    }

    /// <summary>
    /// Handles detaching from agents. If an agent terminated because of us,
    /// and we're still pending on that agent, this event will stick around long
    /// enough for that termination to finish and for the agent to restart.
    /// </summary>
    private void Detaching()
    {
        bool ready = true;
        foreach (BehaviorAgent agent in this.involvedAgents)
            if (this.DetachFromAgent(agent) == false)
                ready = false;

        // We've detached from everyone and are truly finished
        if (ready == true)
            this.EventStatus = Status.Finished;
    }

    bool IBehaviorUpdate.BehaviorUpdate(float deltaTime)
    {
        switch (this.EventStatus)
        {
            case Status.Initializing:
                this.Initializing();
                break;
            case Status.Pending:
                this.Pending();
                break;
            case Status.Running:
                this.Running();
                break;
            case Status.Terminating:
                this.Terminating();
                break;
            case Status.Detaching:
                this.Detaching();
                break;
        }

        return (this.EventStatus != Status.Finished);
    }
}
