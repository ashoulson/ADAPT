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
    public static class TreeUtils
    {
        /// <summary>
        /// Given a collection of objects, will keep calling func on them
        /// until the result is either success or failure for all items.
        /// Returns failure if any of them failed after every call completed.
        /// 
        /// TODO: Maybe make this a yield-based function that keeps a list 
        /// for efficiency - AS
        /// </summary>
        public static RunStatus DoUntilComplete<T>(
            Func<T, RunStatus> func,
            IEnumerable<T> items)
        {
            RunStatus final = RunStatus.Success;
            foreach (T item in items)
            {
                RunStatus rs = func.Invoke(item);
                if (rs == RunStatus.Running)
                    final = RunStatus.Running;
                else if (final != RunStatus.Running && rs == RunStatus.Failure)
                    final = RunStatus.Failure;
            }
            return final;
        }
    }
}
