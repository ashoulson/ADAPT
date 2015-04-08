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
    /// When terminated, a Catch decorator will terminate its child as normal,
    /// but can then run an additional given function to clean up after the
    /// child node terminates.
    /// </summary>
    public class DecoratorCatch : Decorator
    {
        private readonly Action term_noReturn = null;
        private readonly Func<RunStatus> term_return = null;
        private readonly Func<bool> term_assert = null;

        private DecoratorCatch(Node child)
            : base(child)
        {
            this.term_noReturn = null;
            this.term_return = null;
            this.term_assert = null;
        }

        public DecoratorCatch(
            Func<RunStatus> function,
            Node child)
            : this(child)
        {
            this.term_return = function;
        }

        public DecoratorCatch(
            Func<bool> assertion,
            Node child)
            : this(child)
        {
            this.term_assert = assertion;
        }

        public DecoratorCatch(
            Action function,
            Node child)
            : this(child)
        {
            this.term_noReturn = function;
        }

        private RunStatus InvokeFunction()
        {
            if (this.term_return != null)
            {
                return this.term_return.Invoke();
            }
            else if (this.term_assert != null)
            {
                if (this.term_assert.Invoke() == true)
                    return RunStatus.Success;
                else
                    return RunStatus.Failure;
            }
            else //if (this.term_noReturn != null)
            {
                this.term_noReturn.Invoke();
                return RunStatus.Success;
            }
        }

        public override RunStatus Terminate()
        {
            // See if we've already finished terminating completely
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;

            // See if the child is still terminating
            RunStatus childTerm = this.DecoratedChild.Terminate();
            if (childTerm == RunStatus.Running)
                return this.ReturnTermination(childTerm);

            // Otherwise, use our given function
            return this.ReturnTermination(this.InvokeFunction());
        }
    }
}