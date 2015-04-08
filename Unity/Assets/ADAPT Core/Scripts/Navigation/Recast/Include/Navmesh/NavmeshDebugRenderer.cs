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
//using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public enum NavmeshDebugDrawType
{
    None = 0,
    ProvidedGeometry,
    HeightfieldSolid,
    HeightfieldWalkable,
    CompactHeightfieldSolid,
    CompactHeightfieldRegions,
    CompactHeightfieldDistance,
    RawContours,
    Contours,
    PolyMesh,
    PolyMeshDetail,
    Navmesh
}

public static class NavmeshDebugRenderer 
{
	private class DebugNavmesh 
	{
		public IntPtr dtNavMesh { get; protected set; }

		public DebugNavmesh(byte[] data)
		{
			this.dtNavMesh = NativeDebugInitNavmesh(
				data, 
				data.Length);
		}

		~DebugNavmesh()
		{
			NativeDebugDestroyNavmesh(this.dtNavMesh);
		}

		[DllImport("Navmesh_RecastDetour", EntryPoint="DebugInitNavmesh")]
		private static extern IntPtr NativeDebugInitNavmesh(
			[MarshalAs(UnmanagedType.LPArray)] byte[] data, 
			int dataSize);
		[DllImport("Navmesh_RecastDetour", EntryPoint="DebugDestroyNavmesh")]
		private static extern void NativeDebugDestroyNavmesh(IntPtr ptr);
	}

	private static Material debugMaterial = null;
	private static Material DebugMaterial
	{
		get
		{
			if (debugMaterial == null)
			{
				Shader s = (Shader)Resources.Load("NavmeshDebugShader");
				debugMaterial = new Material(s);
				debugMaterial.hideFlags = HideFlags.HideAndDontSave;
				debugMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return debugMaterial;
		}
	}
	
    public static void RenderIntermediate(NavmeshDebugDrawType debugDrawType)
    {
        Vector3 upVec, rightVec, outVec;
        GetBillboardVectors(out upVec, out rightVec, out outVec);
        int numVertices, numIndices;
        NativeDebugDrawIntermediate(
            debugDrawType, upVec, rightVec, outVec, out numVertices, out numIndices);
        DrawDebugData(numVertices, numIndices);
    }

    public static void RenderNavmesh(byte[] navmeshData)
    {
		// TODO: This is TERRIBLY inefficient, but caching is nontrivial
		// since it can get out of sync with the underlying data. Resolve.
		DebugNavmesh debugNavmesh = new DebugNavmesh(navmeshData);

        Vector3 upVec, rightVec, outVec;
        GetBillboardVectors(out upVec, out rightVec, out outVec);
        int numVertices, numIndices;
        NativeDebugDrawNavmesh(
            debugNavmesh.dtNavMesh, 
			upVec, 
			rightVec, 
			outVec, 
			out numVertices, 
			out numIndices);
        DrawDebugData(numVertices, numIndices);
    }

    private static void DrawDebugData(int numVertices, int numIndices)
    {
        Vector3[] vertices = new Vector3[numVertices];
        Vector3[] normals = new Vector3[numVertices];
        Color[] colors = new Color[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] indices = new int[numIndices];

        NativeRetrieveDebugDrawData(vertices, colors, uvs, normals, indices);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors = colors;
        //mesh.uv = uvs;
        mesh.triangles = indices;
        mesh.RecalculateBounds();

        DebugMaterial.SetPass(0);

        //Graphics.DrawMesh(mesh, Matrix4x4.identity, debugMaterial, 0);
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
    }

    private static void GetBillboardVectors(out Vector3 upVec, out Vector3 rightVec, out Vector3 outVec)
    {
        upVec = Camera.current.worldToCameraMatrix.GetColumn(1);
        rightVec = Camera.current.worldToCameraMatrix.GetColumn(0);
        outVec = Camera.current.worldToCameraMatrix.GetColumn(2);
    }

    [DllImport("Navmesh_RecastDetour", EntryPoint = "DebugDrawIntermediate")]
    private static extern void NativeDebugDrawIntermediate(
        [MarshalAs(UnmanagedType.I4)] NavmeshDebugDrawType debugDrawType,
        [MarshalAs(UnmanagedType.LPStruct)] Vector3 upVec,
        [MarshalAs(UnmanagedType.LPStruct)] Vector3 rightVec,
        [MarshalAs(UnmanagedType.LPStruct)] Vector3 outVec,
        out int numVertices,
        out int numIndices);

    [DllImport("Navmesh_RecastDetour", EntryPoint = "DebugDrawNavmesh")]
    private static extern void NativeDebugDrawNavmesh(
        IntPtr navmesh,
        [MarshalAs(UnmanagedType.LPStruct)] Vector3 upVec,
        [MarshalAs(UnmanagedType.LPStruct)] Vector3 rightVec,
        [MarshalAs(UnmanagedType.LPStruct)] Vector3 outVec,
        out int numVertices,
        out int numIndices);

    [DllImport("Navmesh_RecastDetour", EntryPoint = "RetrieveDebugDrawData")]
    private static extern void NativeRetrieveDebugDrawData(
        [MarshalAs(UnmanagedType.LPArray)] Vector3[] vertices,
        [MarshalAs(UnmanagedType.LPArray)] Color[] colors,
        [MarshalAs(UnmanagedType.LPArray)] Vector2[] uvs,
        [MarshalAs(UnmanagedType.LPArray)] Vector3[] normals,
        [MarshalAs(UnmanagedType.LPArray)] int[] indices);
}
