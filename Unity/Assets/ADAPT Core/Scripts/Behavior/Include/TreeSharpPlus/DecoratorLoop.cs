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
    /// Loops will continue executing their child indefinitely unless that child reports
    /// failure. If the child reports success, the proceed loop will restart it from the beginning.
    /// </summary>
    public class DecoratorLoop : Decorator
    {
        /// <summary>
        ///     The number of iterations to run (-1 is infinite)
        /// </summary>
        public int Iterations { get; set; }

        public DecoratorLoop(Node child, int iterations = -1)
            : base(child)
        {
            this.Iterations = iterations;
        }

        public override IEnumerable<RunStatus> Execute()
        {
            // Keep track of the running iterations
            int curIter = 0;

            while(true)
            {
                this.DecoratedChild.Start();

                RunStatus result;
                while ((result = DecoratedChild.Tick()) == RunStatus.Running)
                    yield return RunStatus.Running;

                this.DecoratedChild.Stop();

                // If the child failed, break and report the failure
                if (result == RunStatus.Failure)
                {
                    yield return RunStatus.Failure;
                    yield break;
                }

                // Increase the iteration count and see if we're done
                curIter++;
                if ((Iterations > 0) && (curIter >= Iterations))
                {
                    yield return RunStatus.Success;
                    yield break;
                }

                // Take one tick to prevent infinite loops
                yield return RunStatus.Running;
            }
        }
    }
}