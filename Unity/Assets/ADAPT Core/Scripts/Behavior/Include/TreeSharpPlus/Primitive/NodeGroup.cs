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

namespace TreeSharpPlus
{
    public abstract class NodeGroup : Node
    {
        protected NodeGroup(params Node[] children)
        {
            this.Children = new List<Node>(children);
            foreach (Node node in Children)
                if (node != null)
                    node.Parent = this;
        }

        public List<Node> Children { get; set; }
        public Node Selection { get; protected set; }

        public override void Start()
        {
            this.CleanupHandlers.Push(new ChildrenCleanupHandler(this));
            base.Start();
        }

        public override void Stop()
        {
            foreach (Node child in this.Children)
                child.Stop();
            base.Stop();
        }

        public override RunStatus Terminate()
        {
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;

            // If we had a node active, terminate it
            if (this.Selection != null)
                return this.ReturnTermination(this.Selection.Terminate());
            return this.ReturnTermination(RunStatus.Success);
        }

        /// <summary>
        /// Clears the LastStatus field. Useful for debugging and tracing
        /// the source of a success or failure when a tree terminates.
        /// </summary>
        public override void ClearLastStatus()
        {
            this.LastStatus = null;
            foreach (Node child in this.Children)
                child.ClearLastStatus();
        }

        #region Nested type: ChildrenCleanupHandler

        protected class ChildrenCleanupHandler : CleanupHandler
        {
            public ChildrenCleanupHandler(NodeGroup owner)
                : base(owner)
            {
            }

            protected override void DoCleanup()
            {
                foreach (Node composite in (Owner as NodeGroup).Children)
                {
                    composite.Stop();
                }
            }
        }

        #endregion
    }
}