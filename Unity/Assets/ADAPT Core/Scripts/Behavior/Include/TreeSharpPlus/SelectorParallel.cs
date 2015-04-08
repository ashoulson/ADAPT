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

namespace TreeSharpPlus
{
    /// <summary>
    /// Parallel Selector nodes execute all of their children in parallel. If any
    /// sequence reports success, we finish all of the other ticks, but then stop
    /// all other children and report success. We report failure when all children
    /// report failure.
    /// </summary>
    public class SelectorParallel : Parallel
    {
        private int runningNodes;

        public SelectorParallel(params Node[] children)
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
                for (int i = 0; i < this.Children.Count; i++)
                {
                    if (this.childStatus[i] == RunStatus.Running)
                    {
                        Node node = this.Children[i];
                        RunStatus tickResult = node.Tick();

                        // Check to see if anything finished
                        if (tickResult != RunStatus.Running)
                        {
                            // Clean up the node
                            node.Stop();
                            this.childStatus[i] = tickResult;
                            this.runningNodes--;

                            // If the node succeeded, we're done
                            if (tickResult == RunStatus.Success)
                            {
                                // We may be stopping nodes in progress, so it's best
                                // to do a clean terminate and give them time to end
                                while (this.TerminateChildren() == RunStatus.Running)
                                    yield return RunStatus.Running;
                                // TODO: Timeout? - AS
                                // TODO: What if Terminate() fails? - AS

                                // Clear out the LastStatus trail for all other nodes.
                                foreach (Node n in this.Children)
                                    if (n != node)
                                        n.ClearLastStatus();

                                // Report success
                                yield return RunStatus.Success;
                                yield break;
                            }
                        }
                    }
                }

                // If we're out of running nodes, we're done
                if (this.runningNodes == 0)
                {
                    yield return RunStatus.Failure;
                    yield break;
                }

                // For forked ticking
                yield return RunStatus.Running;
            }
        }
    }
}