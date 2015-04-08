#region License
/*
* Agent Development and Prototyping Testbed
* https://github.com/ashoulson/ADAPT
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
*
* This file is part of ADAPT.
* 
* ADAPT is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* ADAPT is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with ADAPT.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class FilterList<T>
{
    public enum ListType { Whitelist, Blacklist };
    public readonly ListType type;

    private readonly HashSet<T> elements;

    public FilterList(ListType type, params T[] entries)
    {
        this.elements = new HashSet<T>(entries);
        this.type = type;
    }

    public bool Allowed(T item)
    {
        return (elements.Contains(item) ^ this.type == ListType.Blacklist);
    }

    public bool IsWhitelist()
    {
        return this.type == ListType.Whitelist;
    }
}

public class Blacklist<T> : FilterList<T>
{
    public Blacklist(params T[] ent) : base(ListType.Blacklist, ent) { }
}

public class Whitelist<T> : FilterList<T>
{
    public Whitelist(params T[] ent) : base(ListType.Whitelist, ent) { }
}
