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
using System.Collections.Generic;

public struct TimePoint {
	public float time;
	public Vector3 point;
	public TimePoint(float time, Vector3 point) {
		this.time = time;
		this.point = point;
	}
}

public class TrajectoryVisualizer {
	
	private Color color;
	private float length;
	private bool dotted;
	private List<TimePoint> trajectory = new List<TimePoint>();
	
	public TrajectoryVisualizer(Color color, float length) {
		this.color = color;
		this.length = length;
	}
	
	public void AddPoint(float time, Vector3 point) {
		trajectory.Add(new TimePoint(time,point));
		while (trajectory[0].time<time-length) {
			trajectory.RemoveAt(0);
		}
	}
	
	public void Render() {
		//Debug.Log("Point count: "+trajectory.Count);
		if (trajectory.Count==0) return;
		DrawArea draw = new DrawArea3D(Vector3.zero,Vector3.one,Matrix4x4.identity);
		float curTime = trajectory[trajectory.Count-1].time;
		GL.Begin(GL.LINES);
		for (int i=0; i<trajectory.Count-1; i++) {
			Color col = color;
			col.a = (curTime-trajectory[i].time)/length;
			col.a = 1-col.a*col.a;
			draw.DrawLine(trajectory[i].point, trajectory[i+1].point, col);
		}
		GL.End();
	}
}
