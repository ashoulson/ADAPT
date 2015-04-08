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
using System.Runtime.InteropServices;

public class RecastSteeringManager : MonoBehaviour
{
    private static RecastSteeringManager instance = null;
    public static RecastSteeringManager Instance
    {
        get
        {
            if (instance == null)
            {
                UnityEngine.Object[] objs =
                    GameObject.FindObjectsOfType(typeof(RecastSteeringManager));
                if (objs.Length == 1)
                    instance = (RecastSteeringManager)objs[0];
                else if (objs.Length == 0)
                    Debug.LogError("No SteeringManager found");
                else
                    Debug.LogError("Multiple SteeringManagers found");
            }
            return instance;
        }
    }

	public Navmesh navmesh = null;
	public int maxAgents = 10000;
	public float maxAgentRadius = 0.5f;

    bool initialized = false;
    private int lastUpdateFrame = -1;
    protected IntPtr steeringManager;

    public List<RecastSteeringController> agents;
	
    public enum Pushiness 
    { 
        PUSHINESS_LOW, 
        PUSHINESS_MEDIUM, 
        PUSHINESS_HIGH 
    };

    public enum NavigationQuality 
    { 
        NAVIGATIONQUALITY_LOW, 
        NAVIGATIONQUALITY_MED, 
        NAVIGATIONQUALITY_HIGH 
    };
	
	void Update()
    {
        this.EnsureUpdated();
    }

    public void EnsureUpdated()
    {
        if (this.lastUpdateFrame != Time.frameCount)
        {
            this.DoUpdate();
            this.lastUpdateFrame = Time.frameCount;
        }
    }

    private void DoUpdate()
    {
        if (initialized)
            NativeUpdate(this.steeringManager, Time.deltaTime);
    }

    void OnEnable()
    {
        if (this.navmesh != null)
        {
            if (this.navmesh.Data != null)
            {
                this.steeringManager = NativeCreateSteeringManager();
                NativeInit(
                    this.steeringManager,
                    this.navmesh.Data,
                    this.navmesh.Data.Length,
                    this.maxAgents,
                    this.maxAgentRadius);
                this.initialized = true;
            }
            else
            {
                Debug.LogError("Null Navmesh");
            }
        }
        else
        {
            Debug.LogError("No Navmesh");
        }
    }
	
	void OnDisable()
	{
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeDestroySteeringManager(this.steeringManager);
	}
	
	public int AddAgent(Vector3 vPos, float radius, float height, float accel, float maxSpeed)
	{
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeAddAgent(this.steeringManager, vPos, radius, height, accel, maxSpeed);
	}

    public void RemoveAgent(int agent)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeRemoveAgent(this.steeringManager, agent);
    }

    public Vector3 GetClosestWalkablePosition(Vector3 pos)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeGetClosestWalkablePosition(this.steeringManager, pos);
    }

    public Vector3 GetAgentPosition(int agent)
	{
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeGetAgentPosition(this.steeringManager, agent);
	}

    public Vector3 GetAgentCurrentVelocity(int agent)
	{
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeGetAgentCurrentVelocity(this.steeringManager, agent);
	}

    public Vector3 GetAgentDesiredVelocity(int agent)
	{
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeGetAgentDesiredVelocity(this.steeringManager, agent);
	}

    public void UpdateAgentNavigationQuality(int agent, NavigationQuality nq)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeUpdateAgentNavigationQuality(this.steeringManager, agent, nq);
    }

    public void UpdateAgentPushiness(int agent, Pushiness p)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeUpdateAgentPushiness(this.steeringManager, agent, p);
    }

    public void UpdateAgentHoldingRadius(int agent, float radius)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeUpdateAgentHoldingRadius(this.steeringManager, agent, radius);
    }

    public void UpdateAgentMaxSpeed(int agent, float speed)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeUpdateAgentMaxSpeed(this.steeringManager, agent, speed);
    }

    public void UpdateAgentMaxAcceleration(int agent, float accel)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeUpdateAgentMaxAcceleration(this.steeringManager, agent, accel);
    }

    public bool GetAgentMobile(int agent)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeGetAgentMobile(this.steeringManager, agent);
    }

    public void SetAgentVelocity(int agent, Vector3 velocity)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeSetAgentVelocity(this.steeringManager, agent, velocity);
    }

    public void SetAgentMobile(int agent, bool mobile)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeSetAgentMobile(this.steeringManager, agent, mobile);
    }

    public void SetAgentTarget(int agent, Vector3 vGoal)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        NativeSetAgentTarget(this.steeringManager, agent, vGoal);
    }

    public bool ResetAgentTarget(int agent)
    {
        if (!initialized)
            throw new ApplicationException("Uninitialized Steering Manager");
        return NativeResetAgentTarget(this.steeringManager, agent);
    }

    [DllImport("Steering_RecastDetour", EntryPoint = "createSteeringManager")]
    public static extern IntPtr NativeCreateSteeringManager();
    [DllImport("Steering_RecastDetour", EntryPoint = "destroySteeringManager")]
    public static extern void NativeDestroySteeringManager(IntPtr steeringManager);

    [DllImport("Steering_RecastDetour", EntryPoint = "init")]
    public static extern bool NativeInit(
        IntPtr steeringManager,
        [MarshalAs(UnmanagedType.LPArray)] byte[] data,
        int dataSize,
        int maxAgents,
        float maxAgentRadius);

	[DllImport("Steering_RecastDetour", EntryPoint="update")]
    public static extern IntPtr NativeUpdate(IntPtr steeringManager, float dT);

	[DllImport("Steering_RecastDetour", EntryPoint="addAgent")]
    public static extern int NativeAddAgent(
        IntPtr steeringManager, 
        [MarshalAs(UnmanagedType.LPArray)] Vector3 vPos,
		float radius, 
		float height, 
		float accel, 
		float maxSpeed);
    [DllImport("Steering_RecastDetour", EntryPoint = "removeAgent")]
    public static extern void NativeRemoveAgent(
        IntPtr steeringManager,
        int agent);

    [DllImport("Steering_RecastDetour", EntryPoint = "updateAgentNavigationQuality")]
    public static extern void NativeUpdateAgentNavigationQuality(IntPtr steeringManager, int agent, NavigationQuality nq);
    [DllImport("Steering_RecastDetour", EntryPoint = "updateAgentPushiness")]
    public static extern void NativeUpdateAgentPushiness(IntPtr steeringManager, int agent, Pushiness p);
    [DllImport("Steering_RecastDetour", EntryPoint = "updateAgentHoldingRadius")]
    public static extern void NativeUpdateAgentHoldingRadius(IntPtr steeringManager, int agent, float radius);
    [DllImport("Steering_RecastDetour", EntryPoint = "updateAgentMaxSpeed")]
    public static extern void NativeUpdateAgentMaxSpeed(IntPtr steeringManager, int agent, float speed);
    [DllImport("Steering_RecastDetour", EntryPoint = "updateAgentMaxAcceleration")]
    public static extern void NativeUpdateAgentMaxAcceleration(IntPtr steeringManager, int agent, float accel);

    [DllImport("Steering_RecastDetour", EntryPoint = "setAgentTarget")]
    public static extern bool NativeSetAgentTarget(
        IntPtr steeringManager,
        int agent,
        [MarshalAs(UnmanagedType.LPArray)] Vector3 vPos);
    [DllImport("Steering_RecastDetour", EntryPoint = "setAgentVelocity")]
    public static extern bool NativeSetAgentVelocity(IntPtr steeringManager, int agent, Vector3 velocity);
    [DllImport("Steering_RecastDetour", EntryPoint = "setAgentMobile")]
    public static extern void NativeSetAgentMobile(IntPtr steeringManager, int agent, bool mobile);

    [DllImport("Steering_RecastDetour", EntryPoint = "resetAgentTarget")]
    public static extern bool NativeResetAgentTarget(IntPtr steeringManager, int agent);

    [DllImport("Steering_RecastDetour", EntryPoint = "getAgentMobile")]
    public static extern bool NativeGetAgentMobile(IntPtr steeringManager, int agent);
    [DllImport("Steering_RecastDetour", EntryPoint = "getAgentPosition")]
    public static extern Vector3 NativeGetAgentPosition(IntPtr steeringManager, int agent);
    [DllImport("Steering_RecastDetour", EntryPoint = "getAgentCurrentVelocity")]
    public static extern Vector3 NativeGetAgentCurrentVelocity(IntPtr steeringManager, int agent);
    [DllImport("Steering_RecastDetour", EntryPoint = "getAgentDesiredVelocity")]
    public static extern Vector3 NativeGetAgentDesiredVelocity(IntPtr steeringManager, int agent);

    [DllImport("Steering_RecastDetour", EntryPoint = "getClosestWalkablePosition")]
    public static extern Vector3 NativeGetClosestWalkablePosition(
        IntPtr steeringManager,
        [MarshalAs(UnmanagedType.LPArray)] Vector3 pos);
}

/*
        if (orientate && status==MoveStatus.EnRoute) {
			// If we're close enough to the end-point, turn to face it
            if (targetName != null && (targetPos - transform.position).magnitude < turnAtDistance + holdingRadius)
            {
                if(holdingRadius > 0)
                    dho.desiredOrientation = Quaternion.LookRotation(targetPos - transform.position);
                else
                    dho.desiredOrientation = endRotation;
			// Drivespeed is how fast you turn? (Less for walking, more for ending)
                dho.driveSpeed = arrivingOrientSpeed;
            }
            else
            {
			// If we're moving, significantly
                if(vel.sqrMagnitude > 0.01f)
                {
                    dho.desiredOrientation = Quaternion.LookRotation(vel);
                    dho.driveSpeed = walkingOrientSpeed;
                }
            }
        }
*/
