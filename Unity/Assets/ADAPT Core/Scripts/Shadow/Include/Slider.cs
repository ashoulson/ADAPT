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
using System.Collections;

public class Slider
{
    private static readonly float EPSILON = 0.05f;
    private enum State { Max, Min, SlideUp, SlideDown };
    private State state = State.Min;

    private float maxVal;
    private float minVal;
    private float multiplier;

    public float Value { get; private set; }
    public float Inverse { get { return this.maxVal - this.Value; } }
    public bool IsMin { get { return this.Value < (this.minVal + EPSILON); } }
    public bool IsMax { get { return this.Value > (this.maxVal - EPSILON); } }
    public bool IsMinPrecise { get { return this.state == State.Min; } }
    public bool IsMaxPrecise { get { return this.state == State.Max; } }

    public Slider()
    {
        this.state = State.Min;
        this.maxVal = 1.0f;
        this.minVal = 0.0f;
        this.multiplier = 1.0f;
        this.Value = this.minVal;
    }

    public Slider(float mult)
    {
        this.state = State.Min;
        this.maxVal = 1.0f;
        this.minVal = 0.0f;
        this.multiplier = mult;
        this.Value = this.minVal;
    }

    public void Tick(float deltaTime)
    {
        switch (this.state)
        {
            case State.SlideUp:
                this.Slide(deltaTime, this.multiplier);
                break;
            case State.SlideDown:
                this.Slide(deltaTime, -this.multiplier);
                break;
        }
    }

    public void ToMax()
    {
        switch (this.state)
        {
            case State.Min:
                this.state = State.SlideUp;
                break;
            case State.SlideDown:
                this.state = State.SlideUp;
                break;
        }
    }

    public void ForceMax()
    {
        this.state = State.Max;
        this.Value = this.maxVal;
    }

    public void ForceMin()
    {
        this.state = State.Min;
        this.Value = this.minVal;
    }

    public void ToMin()
    {
        switch (this.state)
        {
            case State.Max:
                this.state = State.SlideDown;
                break;
            case State.SlideUp:
                this.state = State.SlideDown;
                break;
        }
    }

    private void Slide(float deltaTime, float scale)
    {
        this.Value += deltaTime * scale;
        if (this.Value > this.maxVal)
        {
            this.Value = this.maxVal;
            this.state = State.Max;
        }
        else if (this.Value < this.minVal)
        {
            this.Value = this.minVal;
            this.state = State.Min;
        }
    }
}
