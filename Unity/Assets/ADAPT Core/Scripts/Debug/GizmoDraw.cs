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

public class GizmoDraw : MonoBehaviour 
{
    private static Vector3[] cylVerts = null;
    private static int[] cylTris = null;

    /// <summary>
    /// Draws a gizmo cylinder with the given TRS matrix and color
    /// </summary>
    /// <param name="trs"></param>
    /// <param name="color"></param>
    public static void DrawCylinder(Matrix4x4 trs, Color color)
    {
        if (cylVerts == null || cylTris == null)
        {
            GameObject cyl = GameObject.CreatePrimitive(
                PrimitiveType.Cylinder);
            MeshFilter filter = cyl.GetComponent<MeshFilter>();
            cylVerts = filter.sharedMesh.vertices;
            cylTris = filter.sharedMesh.triangles;
            GameObject.DestroyImmediate(cyl);
        }

        Vector3[] verts = new Vector3[cylVerts.Length];
        for (int i = 0; i < cylVerts.Length; i++)
            verts[i] = trs.MultiplyPoint(cylVerts[i]);

        Gizmos.color = color;
        for (int i = 0; i < cylTris.Length / 3; i++)
        {
            int j = i * 3;
            Gizmos.DrawLine(verts[cylTris[j]],
                verts[cylTris[j + 1]]);
            Gizmos.DrawLine(verts[cylTris[j + 1]],
                verts[cylTris[j + 2]]);
            Gizmos.DrawLine(verts[cylTris[j + 2]],
                verts[cylTris[j]]);
        }
    }

    /// <summary>
    /// Draws out the hierarchy of an object with its children
    /// </summary>
    /// <param name="root"></param>
    /// <param name="color"></param>
    public static void DrawHierarchy(Transform root, Color color)
    {
        foreach (Transform child in root)
        {
            Debug.DrawLine(root.position, child.position, color);
            DrawHierarchy(child, color);
        }
    }
}
