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

/// <summary>
/// Class that draws the skeleton using Unity's Debug.DrawLine function.
/// Used for drawing skeletons of shadows.
/// </summary>
public class ShadowGizmo : MonoBehaviour 
{
    public ShadowController parentController = null;

    private static int _curColor = 0;
    private static readonly Color[] _colors =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.black,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        Color.gray,
        Color.red
    };

    /// <summary>
    /// The color of the skeleton.
    /// </summary>
	public Color lineColor;

    void Awake()
    {
        this.lineColor = _colors[_curColor];
        _curColor = (_curColor + 1) % _colors.Length;
    }

	/// <summary>
	/// Redraws the skeleton at every frame
	/// </summary>
	void OnDrawGizmos() 
    {
        if (this.parentController == null ||
            this.parentController.showGizmo == true)
		    GizmoDraw.DrawHierarchy(
                this.transform.root.GetChild(0),
                this.lineColor);
	}
	

}
