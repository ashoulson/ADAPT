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
using System.Text;

namespace TreeSharpPlus
{
    /// <summary>
    /// The base Parallel class. Parallel nodes execute all of their children simultaneously, with
    /// varying termination conditions.
    /// </summary>
    public abstract class Parallel : NodeGroup
    {
        public Parallel(params Node[] children)
            : base(children)
        {
        }

        public abstract override IEnumerable<RunStatus> Execute();
        protected RunStatus[] childStatus = null;

        public override void Start()
        {
            if (this.childStatus == null)
                this.childStatus = new RunStatus[this.Children.Count];
            for (int i = 0; i < this.childStatus.Length; i++)
                this.childStatus[i] = RunStatus.Running;
            base.Start();
        }

        // Used for keeping track of which children have terminated
        protected RunStatus[] terminateStatus = null;
        protected void InitTerminateStatus()
        {
            if (this.terminateStatus == null)
                this.terminateStatus = new RunStatus[this.Children.Count];
            for (int i = 0; i < this.terminateStatus.Length; i++)
                this.terminateStatus[i] = RunStatus.Running;
        }

        protected RunStatus TerminateChildren()
        {
            return TreeUtils.DoUntilComplete<Node>(
                (Node n) => n.Terminate(),
                this.Children);
        }

        public override RunStatus Terminate()
        {
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;
            // Just terminate each child
            return this.ReturnTermination(this.TerminateChildren());
        }
    }
}