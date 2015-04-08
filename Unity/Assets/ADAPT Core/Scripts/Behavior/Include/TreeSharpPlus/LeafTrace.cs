#region License
/*
* A simplistic Behavior Tree implementation in C#
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
* (TreeSharp Copyright (C) 2010-2011 ApocDev apocdev@gmail.com)
* 
* This file is part of TreeSharpPlus.
* 
* TreeSharpPlus is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* TreeSharpPlus is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with TreeSharpPlus.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using UnityEngine;
using System;

namespace TreeSharpPlus
{
    /// <summary>
    ///    Waits for a given period of time, set by the wait parameter
    /// </summary>
    public class LeafTrace : Node
    {
        protected string text;

        /// <summary>
        ///    Initializes with the wait period
        /// </summary>
        /// <param name="waitMax">The time (in seconds) for which to wait</param>
        public LeafTrace(string text)
        {
            this.text = text;
        }

        public override sealed IEnumerable<RunStatus> Execute()
        {
            Debug.Log(this.text);
            yield return RunStatus.Success;
            yield break;
        }
    }
}