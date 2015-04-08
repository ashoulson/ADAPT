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
using TreeSharpPlus;
using System.Collections;

public static class CharacterNodes
{
    public static Node Node_Gesture(this Character c, Val<string> name)
    {
        return new LeafInvoke(() => c.Gesture(name), () => c.GestureStop());
    }

    public static Node Node_GoTo(this Character c, Val<Vector3> targ)
    {
        return new LeafInvoke(() => c.NavGoTo(targ), () => c.NavStop());
    }

    public static Node Node_Reach(this Character c, Val<Vector3> targ)
    {
        return new LeafInvoke(() => c.ReachFor(targ), () => c.ReachStop());
    }

    public static Node Node_HeadLook(this Character c, Val<Vector3> targ)
    {
        return new LeafInvoke(() => c.HeadLook(targ), () => c.HeadLookStop());
    }
}
