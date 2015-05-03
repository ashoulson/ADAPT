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

public class ShadowAnimationController : ShadowController
{
    private bool started = false;

    public bool IsPlaying()
    {
        return transform.GetComponent<Animation>().isPlaying;
    }

    public bool IsPlaying(string name)
    {
        return transform.GetComponent<Animation>().IsPlaying(name);
    }

    public override void ControlledStart()
    {
        this.started = false;
    }

    public override void ControlledUpdate()
    {
        if (this.started == true && this.IsPlaying() == false)
        {
            this.started = false;
            this.Coordinator.SendMessage(
                "EvtDoneAnimation",
                SendMessageOptions.DontRequireReceiver);
        }
    }

    public void AnimPlay(string name)
    {
        this.started = true;
        transform.GetComponent<Animation>().CrossFade(name);
    }

    public void AnimStop()
    {
        this.started = false;
        transform.GetComponent<Animation>().Stop();
    }

    #region Messages
    void CmdStartAnimation(string name)
    {
        this.AnimPlay(name);
    }

    void CmdStopAnimation()
    {
        this.AnimStop();
    }
    #endregion
}
