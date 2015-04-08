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
public class Interpolator {
	
	public float[][] samples;
	
	public static float SqrMagnitude(float[] a) {
		float result = 0.0f;
		for (int i=0; i<a.Length; i++) {
			result += Mathf.Pow(a[i], 2);
		}
		return result;
	}
	public static float Magnitude(float[] a) {
		return Mathf.Sqrt(SqrMagnitude(a));
	}
	
	public static float SqrDistance(float[] a, float[] b) {
		float sqrMagnitude = 0.0f;
		for (int i=0; i<a.Length; i++) {
			sqrMagnitude += Mathf.Pow(a[i]-b[i], 2);
		}
		return sqrMagnitude;
	}
	public static float Distance(float[] a, float[] b) {
		return Mathf.Sqrt(SqrDistance(a,b));
	}
	public static float[] Normalized(float[] a) {
		return Multiply(a,1/Magnitude(a));
	}
	public static bool Equals(float[] a, float[] b) {
		return (SqrDistance(a,b)==0);
	}
	
	public static float[] Multiply(float[] a, float m) {
		float[] sum = new float[a.Length];
		for (int i=0; i<a.Length; i++) {
			sum[i] = a[i]*m;
		}
		return sum;
	}
	
	public static float Dot(float[] a, float[] b) {
		float product = 0.0f;
		for (int i=0; i<a.Length; i++) {
			product += a[i]*b[i];
		}
		return product;
	}
	
	public static float Angle(float[] a, float[] b) {
		float m = Magnitude(a) * Magnitude(b);
		if (m==0) return 0;
		return Mathf.Acos( Mathf.Clamp( Dot(a,b) / m, -1, 1 ) );
	}
	public static float ClockwiseAngle(float[] a, float[] b) {
		float angle = Angle(a,b);
		if ((a[1]*b[0]-a[0]*b[1]) > 0) angle = 2*Mathf.PI - angle;
		return angle;
	}
	
	public static float[] Add(float[] a, float[] b) {
		float[] sum = new float[a.Length];
		for (int i=0; i<a.Length; i++) {
			sum[i] = a[i]+b[i];
		}
		return sum;
	}
	public float[] Subtract(float[] a, float[] b) { return Add(a,Multiply(b,-1)); }
	
	public Interpolator(float[][] samplePoints) { samples = samplePoints; }
	
	public virtual float[] Interpolate(float[] output) {
		return Interpolate(output, true);
	}
	
	// Method cannot be abstract since serilazation does not work well
	// with abstract classes
	public virtual float[] Interpolate(float[] output, bool normalize) {
		throw new System.NotImplementedException();
	}
	
	// Returns the weights if simple cases are fulfilled.
	// Returns null otherwise.
	public float[] BasicChecks(float[] output) {
		if (samples.Length==1) {
			return new float[1] { 1 };
		}
		for (int i=0; i<samples.Length; i++) {
			if (Equals(output, samples[i])) {
				float[] weights = new float[samples.Length];
				weights[i] = 1;
				return weights;
			}
		}
		return null;
	}
	
}
