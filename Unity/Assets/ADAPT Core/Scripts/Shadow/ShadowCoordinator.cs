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

public abstract class ShadowCoordinator : MonoBehaviour
{
    public Transform hips = null;
    public string[] NonBoneTransforms = { "MixamoMesh", "Ragdoll" };

    [HideInInspector]
    private HashSet<string> _nonBoneTransformsSet = null;
    public HashSet<string> NonBoneTransformsSet
    {
        get
        {
            if (this._nonBoneTransformsSet == null)
                this._nonBoneTransformsSet =
                    new HashSet<string>(this.NonBoneTransforms);
            return this._nonBoneTransformsSet;
        }
    }

    protected Dictionary<string, ShadowController> shadowControllers = null;
    protected Dictionary<string, int> boneKeys = null;

    #region BoneKey Functions
    protected void ReadBone(Transform t)
    {
        int curBone = this.boneKeys.Count;
        this.boneKeys[t.name] = curBone;
        foreach (Transform child in t)
            if (this.NonBoneTransformsSet.Contains(t.name) == false)
                this.ReadBone(child);
    }

    protected void InitBoneKeys()
    {
        this.boneKeys = new Dictionary<string, int>();
        this.ReadBone(this.transform);
    }

    public void ClearTransformArray(ShadowTransform[] array)
    {
        for (int i = 0; i < array.Length; i++)
            array[i].valid = false;
    }

    public ShadowTransform[] NewTransformArray()
    {
        ShadowTransform[] buffer = new ShadowTransform[this.boneKeys.Count];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = new ShadowTransform();
        return buffer;
    }

    public int GetBoneKey(string name)
    {
        return this.boneKeys[name];
    }
    #endregion

    #region Controller Functions
    public ShadowController GetController(string name)
    {
        return this.shadowControllers[name];
    }

    public void RegisterController(ShadowController controller)
    {
        if (this.shadowControllers == null)
            this.shadowControllers = new Dictionary<string, ShadowController>();

        if (this.boneKeys == null)
            this.InitBoneKeys();

        string name = controller.GetType().ToString();
        this.shadowControllers[name] = controller;
        controller.shadow = new Shadow(this.transform, this.hips, controller);

        // The controller is now ready to wake up
        controller.ControlledAwake();
    }

    protected void UpdateCoordinates()
    {
        foreach (ShadowController controller in this.shadowControllers.Values)
        {
            controller.shadow.transform.position = transform.position;
            controller.shadow.transform.rotation = transform.rotation;
        }
    }
    #endregion

    #region MonoBehavior Functions
    /// <summary>
    /// Propagates the Start function to all registerred children in order
    /// </summary>
    protected void ControlledStartAll()
    {
        if (this.shadowControllers != null)
            foreach (ShadowController sc in this.shadowControllers.Values)
                sc.ControlledStart();
    }

    /// <summary>
    /// Propagates the Start function to all registerred children in order
    /// </summary>
    void Start()
    {
        this.ControlledStartAll();
    }

    /// <summary>
    /// Override this function to execute your controller update order pipeline
    /// </summary>
    void Update()
    {
        throw new NotImplementedException("No Update() function!");
    }

    /// <summary>
    /// Propagates the LateUpdate function to all registerred children in order
    /// </summary>
    void LateUpdate()
    {
        if (this.shadowControllers != null)
            foreach (ShadowController sc in this.shadowControllers.Values)
                sc.ControlledLateUpdate();
    }

    /// <summary>
    /// Propagates the FixedUpdate function to all registerred children in order
    /// </summary>
    void FixedUpdate()
    {
        if (this.shadowControllers != null)
            foreach (ShadowController sc in this.shadowControllers.Values)
                sc.ControlledFixedUpdate();
    }
    #endregion

    #region Messages
    public void RelayMessage(string methodName)
    {
        this.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        foreach (ShadowController controller in this.shadowControllers.Values)
            controller.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
    }

    public void RelayMessage(string methodName, object value)
    {
        this.SendMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
        foreach (ShadowController controller in this.shadowControllers.Values)
            controller.SendMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
    }
    #endregion
}
