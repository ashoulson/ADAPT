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

/*
Copyright (c) 2008, Rune Skovbo Johansen & Unity Technologies ApS

See the document "TERMS OF USE" included in the project folder for licencing details.
*/
#endregion

using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class IMotionAnalyzer {
	
	[HideInInspector] public string name;
	
	public AnimationClip animation;
	
	public MotionType motionType = MotionType.WalkCycle;
	
	public string motionGroup = "locomotion";
	
	public abstract int samples { get; }
	
	public abstract LegCycleData[] cycles { get; }
	
	public abstract Vector3 cycleDirection { get; }
	
	public abstract float cycleDistance { get; }
	
	public abstract Vector3 cycleVector { get; }
	
	public abstract float cycleDuration { get; }
	
	public abstract float cycleSpeed { get; }
	
	public abstract Vector3 cycleVelocity { get; }
	
	public abstract Vector3 GetFlightFootPosition(int leg, float flightTime, int phase);
	
	public abstract float cycleOffset { get; set; }
	
	public abstract void Analyze(GameObject o);
}
