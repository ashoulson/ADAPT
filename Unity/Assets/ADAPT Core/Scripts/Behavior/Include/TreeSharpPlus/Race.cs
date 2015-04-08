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
using UnityEngine;

namespace TreeSharpPlus
{
    /// <summary>
    /// Race nodes execute all of their children in parallel. As soon as any one
    /// child terminates, we finish all of the other ticks, but then stop
    /// all other children and report whatever the first finisher reported.
    /// </summary>
    public class Race : Parallel
    {
        private int runningNodes;

        public Race(params Node[] children)
            : base(children)
        {
        }

        public override void Start()
        {
            // Start all children
            this.runningNodes = this.Children.Count;
            foreach (Node node in this.Children)
            {
                node.Start();
            }
            base.Start();
        }

        public override void Stop()
        {
            // Stop all children
            this.runningNodes = 0;
            foreach (Node node in this.Children)
            {
                node.Stop();
            }
            base.Stop();
        }

        public override IEnumerable<RunStatus> Execute()
        {
            while (true)
            {
                RunStatus tickResult = RunStatus.Success;
                for (int i = 0; i < this.Children.Count; i++)
                {
                    if (this.childStatus[i] == RunStatus.Running)
                    {
                        Node node = this.Children[i];
                        tickResult = node.Tick();

                        // Check to see if anything finished
                        if (tickResult != RunStatus.Running)
                        {
                            // Clean up the node
                            node.Stop();
                            this.childStatus[i] = tickResult;
                            this.runningNodes--;

                            // Terminate everything else
                            while (this.TerminateChildren() == RunStatus.Running)
                                yield return RunStatus.Running;

                            // Clear out the LastStatus trail for all other nodes.
                            foreach (Node n in this.Children)
                                if (n != node)
                                    n.ClearLastStatus();

                            yield return tickResult;
                            yield break;
                        }
                    }
                }

                yield return RunStatus.Running;
            }
        }
    }
}