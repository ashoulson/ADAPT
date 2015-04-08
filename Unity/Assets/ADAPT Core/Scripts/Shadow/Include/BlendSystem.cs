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

/// <summary>
/// Wrapper pair for passing weighted shadow input into the
/// blending system
/// </summary>
public struct BlendPair
{
    public readonly ShadowTransform[] shadow;
    public readonly float weight;

    public BlendPair(ShadowTransform[] shadow, float weight)
    {
        this.shadow = shadow;
        this.weight = weight;
    }
}

/// <summary>
/// Class that contains static functions to help out with interpolation.
/// </summary>
public static class BlendSystem
{
    /// <summary>
    ///HERP is an interpolation function that outputs a final weight, given a source weight, target weight, time, and sum of
    ///other weights.
    /// 
    ///Increasing the weight of one controller linearly does not make the controlelr
    ///fade in linearly - the result is quite visible and doesn't look very good at all.
    ///Increasing the weight, x, of one controller causes it's percentage influence P(x) to rise 
    ///like so: P(x) = x/(s+x), where s is the sum of all the other weights besides x. The function P(x) 
    ///is implemented by hypFunction(). Obviously, this is nonlinear. What we do here is determine
    ///the source percentage influence, and the target percentage influence by passing the srcWeight
    ///and the tgtWeight to hypFunction, then LERPing to find the desired percentage influence. 
    ///THEN, once we know the desired percentage influence, we apply the inverse function, inverseHyp(),
    ///to determine the weight required for that percentage. 
    ///
    /// P(x) is a hyperbola when plotted, hence the name HERP for this function.
    /// 
    /// \todo HERP gets tricky when we implement a blend tree (feeding the blended output of some controllers
    /// into other controllers as input), since for HERP to work, we need to know the weights of all the other
    /// controllers. Unity's own animation system doesn't fade controllers in/out linearly when their weights
    /// are increased/decreased linearly. Since we are primarily using interpolation to blend controllers in 
    /// and out smoothly, it may not even be necessary to use HERP. When we are fading things in and out quickly, the 
    /// "non-linear fading" problem may not even be noticable.
    /// 
    /// </summary>
    /// <param name="srcWeight">Starting weight we are interpolating from</param>
    /// <param name="tgtWeight">Target weight we are interpolating to</param>
    /// <param name="sum">Sum of all other weights on the particular controller/joint that we are fading</param>
    /// <param name="time">Parameter used in interpolation, between 0 and 1. If time = 0, then 
    /// herp will return srcWeight. If time = 1, then herp will return tgtWeight.</param>
    /// <returns></returns>
    public static float herp(float srcWeight, float tgtWeight, float sum, float time)
    {
        //clamp time to between 0 and 1
        if (time < 0)
            time = 0;
        else if (time > 1)
            time = 1;

        //find the percentage influence that represents tgtController.weight and tgtWeight
        float srcPercentage = hypFunction(sum, srcWeight); //from, our source
        float tgtPercentage = hypFunction(sum, tgtWeight); //to, our destination
        //use time to LERP the parameter between tgtController.weight and tgtWeight
        float desiredPercentage = Mathf.Lerp(srcPercentage, tgtPercentage, time);

        //pass the sum to inverseHyp(). 
        float finalWeight = 0;

        //inverseHyp will return infinity if desiredPercentage is 1. It will return 0
        //if the sum is zero. In either case, we want tgtController to have full influence
        //over the joint. 
        if (desiredPercentage == 1 || (desiredPercentage > 0 && sum == 0))
            finalWeight = tgtWeight;
        else
            finalWeight = inverseHyp(sum, desiredPercentage);

        return finalWeight;
    }

    /// <summary>
    /// Evaluates the rational function P(x) = x/(s+x). See the herp() documentation for more details.
    /// </summary>
    /// <returns>Value of P(x), given x and s.</returns>
    private static float hypFunction(float s, float x)
    {
        if (x == 0)
            return 0;
        else
            return x / (s + x);
    }

    /// <summary>
    /// Evaluates the rational function P<SUP>-1</SUP>(x) = sx/(1-x). Inverse function of P(x) = x/(s+x). 
    /// See the herp() documentation for more details on P(x).
    /// </summary>
    /// <returns>Value of P<SUP>-1</SUP>(x), given x and s.</returns>
    private static float inverseHyp(float s, float x)
    {
        return (s * x) / (1 - x);
    }

    /// <summary>
    /// Normalizes array of weights such that all weights will sum to 1.
    /// </summary>
    /// <param name="weights">Array of weights to normalize.</param>
    public static void NormalizeWeights(float[] weights)
    {
        float sum = 0;
        for (int i = 0; i < weights.Length; i++)
            sum += weights[i];
        if (sum == 0)
            Debug.LogError("Weights sum to zero!");
        else
            for (int i = 0; i < weights.Length; i++)
                weights[i] /= sum;
    }

    /// <summary>
    /// Averages a sequence of weighted vectors
    /// </summary>
    public static Vector3 BlendVector3(
        IEnumerable<Vector3> values, 
        IEnumerable<float> weights)
    {
        Vector3 result = Vector3.zero;

        IEnumerator<float> weightIter = weights.GetEnumerator();
        IEnumerator<Vector3> valueIter = values.GetEnumerator();

        while (weightIter.MoveNext() && valueIter.MoveNext())
            result += weightIter.Current * valueIter.Current;

        return result;
    }

    public static Quaternion BlendQuaternion(
        IEnumerable<Quaternion> values, 
        IEnumerable<float> weights)
    {
        Quaternion? first = null;
        Quaternion result = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);

        IEnumerator<float> weightIter = weights.GetEnumerator();
        IEnumerator<Quaternion> valueIter = values.GetEnumerator();

        while (weightIter.MoveNext() && valueIter.MoveNext())
        {
            float weight = weightIter.Current;
            Quaternion q = valueIter.Current;

            // The "dot trick". If any quaternion dots negatively with the
            // first quaternion in the list, invert it.
            if (first != null && Quaternion.Dot((Quaternion)first, q) < 0)
                q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
            else if (first == null)
                first = q;

            // Very, very naively blend the quaternions
            result.x += q.x * weight;
            result.y += q.y * weight;
            result.z += q.z * weight;
            result.w += q.w * weight;
        }

        // Perform the blend on the converted unit vectors and convert back
        return result;
    }

    public static ShadowTransform[] Blend(
        ShadowTransform[] buffer,
        params BlendPair[] shadows)
    {
        int boneCount = shadows[0].shadow.Length;  // Bones per shadow
        int shadowCount = shadows.Length;       // Total number of shadows

        for (int i = 0; i < boneCount; i++)
        {
            List<float> weights = new List<float>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();

            for (int j = 0; j < shadowCount; j++)
            {
                // Extract the features from the bone
                ShadowTransform bone = shadows[j].shadow[i];
                if (ShadowTransform.IsValid(bone) == true)
                {
                    weights.Add(shadows[j].weight);
                    positions.Add(bone.Position);
                    rotations.Add(bone.Rotation);
                }
            }

            // If we just have one weight for this bone
            if (weights.Count == 1)
            {
                buffer[i].ReadFrom(positions[0], rotations[0]);
            }
            // If we have anything to blend for this bone
            else if (weights.Count > 1)
            {
                float[] weightsArray = weights.ToArray();
                NormalizeWeights(weightsArray);

                buffer[i].ReadFrom(
                    BlendVector3(positions, weightsArray),
                    BlendQuaternion(rotations, weightsArray));
            }
        }

        return buffer;
    }
}
