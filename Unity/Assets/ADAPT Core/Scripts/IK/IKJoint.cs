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

[System.Serializable]
public class IKJoint
{
    [System.Serializable]
    public class Restriction
    {
        public bool constrain;
        public bool relax;
        public float AngleA;
        public float AngleB;
        public float Rest;
        public float Strength;

        /// <summary>
        /// Find the shortest distance around a circle between two angles
        /// from 0 to 360 degrees
        /// </summary>
        private float minCircleDistance(float fromAngle, float toAngle)
        {
            float dist1 = fromAngle - toAngle;
            float dist2 = (fromAngle - 360.0f) - toAngle;

            if (Math.Abs(dist1) < Math.Abs(dist2))
                return dist1;
            return dist2;
        }

        /// <summary>
        /// Find the shortest distance around a circle between two angles
        /// from 0 to 360 degrees
        /// </summary>
        private float minAbsCircleDistance(float fromAngle, float toAngle)
        {
            return Mathf.Min(
                Mathf.Abs(fromAngle - toAngle),
                Mathf.Abs((fromAngle - 360.0f) - toAngle));
        }

        private float Clamp(float value)
        {
            float aDist = minAbsCircleDistance(this.AngleA, value);
            float bDist = minAbsCircleDistance(this.AngleB, value);
            if (aDist > bDist)
                return this.AngleB;
            return this.AngleA;
        }

        private bool isInRange(float value)
        {
            if (this.AngleA > this.AngleB)
                return (value < this.AngleA && value > this.AngleB);
            else
                return (value < this.AngleA || value > this.AngleB);
        }

        public Restriction()
        {
            this.constrain = false;
            this.relax = false;
            this.AngleA = 0.0f;
            this.AngleB = 0.0f;
            this.Rest = 0.0f;
            this.Strength = 0.0f;
        }

        public float Constrain(float value)
        {
            if (this.constrain == true && this.isInRange(value) == false)
                return this.Clamp(value);
            return value;
        }

        public float Relax(float value, float deltaTime)
        {
            if (this.relax == true)
            {
                float difference = this.minCircleDistance(value, this.Rest);
                if (difference > 0.0f)
                    difference = this.Strength * deltaTime;
                else
                    difference = -this.Strength * deltaTime;
                return value - difference;
            }
            return value;
        }
    }

    public Transform transform;
    public Restriction xRange;
    public Restriction yRange;
    public Restriction zRange;

    public IKJoint(Transform t)
    {
        this.transform = t;
        this.xRange = new Restriction();
        this.yRange = new Restriction();
        this.zRange = new Restriction();
    }

    public void Constrain()
    {
        Vector3 euler = this.localRotation.eulerAngles;
        euler.x = this.xRange.Constrain(euler.x);
        euler.y = this.yRange.Constrain(euler.y);
        euler.z = this.zRange.Constrain(euler.z);
        this.localRotation = Quaternion.Euler(euler);
    }

    public void Relax(float deltaTime)
    {
        Vector3 euler = this.localRotation.eulerAngles;
        euler.x = this.xRange.Relax(euler.x, deltaTime);
        euler.y = this.yRange.Relax(euler.y, deltaTime);
        euler.z = this.zRange.Relax(euler.z, deltaTime);
        this.localRotation = Quaternion.Euler(euler);
    }

    public Vector3 position 
    { 
        get { return this.transform.position; }
        set { this.transform.position = value; }
    }

    public Vector3 localPosition
    {
        get { return this.transform.localPosition; }
        set { this.transform.localPosition = value; }
    }

    public Quaternion rotation
    {
        get { return this.transform.rotation; }
        set { this.transform.rotation = value; }
    }

    public Quaternion localRotation
    {
        get { return this.transform.localRotation; }
        set { this.transform.localRotation = value; }
    }
}