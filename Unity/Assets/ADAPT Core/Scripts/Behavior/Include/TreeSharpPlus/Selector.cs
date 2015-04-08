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

using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeSharpPlus
{
    /// <summary>
    /// The base selector class. This will attempt to execute all branches of logic, until one succeeds. 
    /// This composite will fail only if all branches fail as well.
    /// </summary>
    public class Selector : NodeGroup
    {
        public Selector(params Node[] children)
            : base(children)
        {
        }

        public override IEnumerable<RunStatus> Execute()
        {
            foreach (Node node in this.Children)
            {
                // Move to the next node
                this.Selection = node;
                node.Start();

                // If the current node is still running, report that. Don't 'break' the enumerator
                RunStatus result;
                while ((result = node.Tick()) == RunStatus.Running)
                    yield return RunStatus.Running;
            
                // Call Stop to allow the node to clean anything up.
                node.Stop();

                // Clear the selection
                this.Selection.ClearLastStatus();
                this.Selection = null;

                // If it succeeded, we return success without trying any subsequent nodes
                if (result == RunStatus.Success)
                {
                    yield return RunStatus.Success;
                    yield break;
                }

                // Otherwise, we're still running
                yield return RunStatus.Running;
            }

            // We ran out of children, and none succeeded. Return failed.
            yield return RunStatus.Failure;
            yield break;
        }
    }
}