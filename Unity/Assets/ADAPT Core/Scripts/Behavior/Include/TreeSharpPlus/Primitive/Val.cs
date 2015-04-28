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

namespace TreeSharpPlus
{
    [System.Serializable]
    public class Val
    {
        /// <summary>
        /// An adaptor function to help C# implicitly cast Func to Val
        /// See: http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/09b27cdd-e261-4025-acfc-66bfcd0d92cf
        /// </summary>
        public static Val<T> Value<T>(Func<T> f)
        {
            return new Val<T>(f);
        }

        public static Val<T> Value<T>(T v)
        {
            return new Val<T>(v);
        }
    }

    [System.Serializable]
    public class Val<T> : Val
    {
        private enum ValType
        {
            Const,
            Dynamic
        }

        private ValType type;
        private Func<T> getter;
        private T value;

        // Have we fetched a value yet?
        private bool valid;

        /// <summary>
        /// Set this value to true if you want the Val to fetch and cache a
        /// value from the getter function manually by calling Fetch(). Setting
        /// this to false will cause the function to fetch whenever accessed.
        /// </summary>
        public bool UseCache;

        public Val(T value)
        {
            this.type = ValType.Const;
            this.getter = null;
            this.UseCache = false;
            this.value = value;
            this.valid = true;
        }

        public Val(Func<T> getter, bool useCache = false)
        {
            this.type = ValType.Dynamic;
            this.getter = getter;
            this.UseCache = useCache;
            this.valid = false;
        }

        public T Value
        {
            get
            {
                if (this.type == ValType.Dynamic && this.UseCache == false)
                    this.Fetch();
                if (this.valid == false)
                    throw new ApplicationException("Invalid Val<T> value");
                return this.value;
            }
        }

        public void Fetch()
        {
            this.valid = true;
            this.value = this.getter();
        }

        public static implicit operator Val<T>(Func<T> value)
        {
            return new Val<T>(value);
        }

        public static implicit operator Val<T>(T value)
        {
            return new Val<T>(value);
        }
    }
}
