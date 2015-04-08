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
    /// The base Decorator class. Decorators have only one child, and modify the execution
    /// of that child in some fundamental way
    /// </summary>
    public abstract class Decorator : NodeGroup
    {
        public Decorator(Node child)
            : base(child)
        {
            // Store the selection
            Selection = child;
        }

        public Node DecoratedChild { get { return Children[0]; } }

        public override void Start()
        {
            if (Children.Count != 1)
                throw new ApplicationException(
                    this + ".Start(): Decorator with multiple children");
            base.Start();
        }

        public override RunStatus Terminate()
        {
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;
            // Just pass the termination down to the child
            return this.ReturnTermination(this.DecoratedChild.Terminate());
        }

        // A vacuous execute function that executes the child transparently
        public override IEnumerable<RunStatus> Execute()
        {
            DecoratedChild.Start();

            RunStatus result;
            while ((result = DecoratedChild.Tick()) == RunStatus.Running)
                yield return RunStatus.Running;

            DecoratedChild.Stop();

            yield return result;
            yield break;
        }
    }
}