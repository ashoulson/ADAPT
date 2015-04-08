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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class NavmeshBuilder : MonoBehaviour
{
	private static NavmeshBuilder instance = null;
	public static NavmeshBuilder Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject go = GameObject.Find("Navmesh Builder");
				if (go == null)
				{
					go = new GameObject("Navmesh Builder");
				}
				instance = go.GetComponent<NavmeshBuilder>();
				if (instance == null)
				{
					instance = go.AddComponent<NavmeshBuilder>();
				}
			}
			return instance;
		}
	}
	
    public Vector3 center = Vector3.zero;
    public Vector3 size = new Vector3(50.0f, 50.0f, 50.0f);
    public LayerMask walkableLayers = -1;
    public float cellSize = 0.3f;
    public float cellHeight = 0.2f;
    public float walkableHeight = 2.0f;
    public float walkableSlopeAngle = 45.0f;
    public float walkableClimb = 0.9f;
    public float walkableRadius = 0.6f;
    public bool monotonePartitioning;
    public float minRegionArea = 6;
    public float mergeRegionArea = 36;
    public float maxEdgeLen = 12.0f;
    public float maxSimplificationError = 1.3f;
    public float detailSampleDist = 6.0f;
    public float detailSampleMaxError = 1.0f;
    public bool keepIntermediateData = false;
	
	protected class BigMesh
	{
		public Vector3[] vertices;
		public int[] triangles;
			
		public BigMesh(Mesh m)
		{
			this.vertices = m.vertices;
			this.triangles = m.triangles;
		}
		
		public BigMesh(PrimitiveType primitiveType)
		{
			GameObject go = GameObject.CreatePrimitive(primitiveType);
			Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
			this.vertices = m.vertices;
			this.triangles = m.triangles;
			GameObject.DestroyImmediate(go);
		}
		
	   public BigMesh(TerrainData td, Bounds b)
		{
			int xSamples = td.heightmapWidth;
			int zSamples = td.heightmapHeight;
			Vector3 patchScale = td.size / (float)(td.heightmapResolution-1);
			patchScale.y = td.size.y;
			float[,] heights = td.GetHeights(0, 0, xSamples, zSamples);

			int minX = Mathf.Max(
				0, 
				Mathf.FloorToInt((b.center.x - b.extents.x) / patchScale.x));
			int maxX = Mathf.Min(
				xSamples, 
				Mathf.CeilToInt((b.center.x + b.extents.x) / patchScale.x) + 1);
			int minZ = Mathf.Max(
				0,
				Mathf.FloorToInt((b.center.z - b.extents.z) / patchScale.z));
			int maxZ = Mathf.Min(
				zSamples,
				Mathf.CeilToInt((b.center.z + b.extents.z) / patchScale.z) + 1);
			int sizeX = maxX - minX;
			int sizeZ = maxZ - minZ;
			this.vertices = new Vector3[sizeX * sizeZ];

			for (int iz = 0; iz < sizeZ; iz++)
			{
				for(int ix = 0; ix < sizeX; ix++)
				{
					this.vertices[iz * sizeX + ix] = new Vector3(
						(ix + minX) * patchScale.x,
						heights[iz+minZ, ix+minX] * patchScale.y,
						(iz + minZ) * patchScale.z);
				}
			}

			int numQuads = (sizeX-1) * (sizeZ-1);
			int numTriangles = numQuads * 2;
			this.triangles = new int[numTriangles * 3];
			int iind = 0;
			for (int iz = 0; iz < sizeZ-1; iz++)
			{
				for (int ix = 0; ix < sizeX - 1; ix++)
				{
					int i1 = iz * sizeX + ix;
					int i2 = i1 + 1;
					int i3 = i1 + sizeX;
					int i4 = i3 + 1;
					this.triangles[iind + 0] = i1;
					this.triangles[iind + 1] = i3;
					this.triangles[iind + 2] = i2;
					this.triangles[iind + 3] = i4;
					this.triangles[iind + 4] = i2;
					this.triangles[iind + 5] = i3;
					iind += 6;
				}
			}
		}

		public BigMesh(BigCombineInstance[] instances)
		{
			int numVerts = 0;
			int numIndices = 0;
			
			foreach(BigCombineInstance bci in instances)
			{
				numVerts += bci.bigMesh.vertices.Length;
				numIndices += bci.bigMesh.triangles.Length;
			}

			this.vertices = new Vector3[numVerts];
			this.triangles = new int[numIndices];

			int vertBase = 0;
			int indexBase = 0;
			foreach (BigCombineInstance bci in instances)
			{
				Vector3[] vertIn = bci.bigMesh.vertices;
				for (int iv = 0; iv < vertIn.Length; iv++)
					vertices[vertBase + iv] = 
						bci.transform.MultiplyPoint3x4(vertIn[iv]);

				int[] triIn = bci.bigMesh.triangles;
				for (int ii=0; ii<triIn.Length; ii++)
					triangles[indexBase + ii] = triIn[ii] + vertBase;

				vertBase += bci.bigMesh.vertices.Length;
				indexBase += bci.bigMesh.triangles.Length;
			}
		}
	}

	protected struct BigCombineInstance
	{
		public BigMesh bigMesh;
		public Matrix4x4 transform;
	}
	
    private byte[] BuildNavmesh()
    {
        IEnumerable<Collider> colliders = GatherColliders(this.walkableLayers);
        Bounds bounds = new Bounds(this.center, this.size);
        BigCombineInstance[] combineInstances = 
			MakeCombineInstanceArray(colliders, bounds);
		
        if (combineInstances.Length == 0)
        {
            Debug.LogError("No colliders found");
            return null;
        }
		
        BigMesh m = new BigMesh(combineInstances);
		
        Debug.Log(string.Format(
            "Combined {0} meshes, {1} vertices, {2} triangles",
            combineInstances.Length, m.vertices.Length, m.triangles.Length / 3));

        int dataSize = NativeBuildNavmesh(
            m.vertices.Length,
            m.vertices,
            m.triangles.Length,
            m.triangles,
            bounds.min.x,
            bounds.min.y,
            bounds.min.z,
            bounds.max.x,
            bounds.max.y,
            bounds.max.z,
			this.cellSize,
			this.cellHeight,
			this.walkableHeight,
			this.walkableSlopeAngle,
			this.walkableClimb,
			this.walkableRadius,
			this.maxEdgeLen,
			this.maxSimplificationError,
			this.monotonePartitioning,
			this.minRegionArea,
			this.mergeRegionArea,
			this.detailSampleDist,
			this.detailSampleMaxError,
			this.keepIntermediateData,
            1000000);
			
        if (dataSize <= 0)
        {
            Debug.LogError("Error during navmesh generation: " + dataSize);
            return null;
        }
        else
        {
            Debug.Log("Built navmesh of size " + dataSize);
        }

        byte[] buffer = new byte[dataSize];
        NativeRetrieveNavmeshData(buffer);
        return buffer;
    }

    private static IEnumerable<Collider> GatherColliders(LayerMask layerMask)
    {
        foreach (Object obj in Object.FindObjectsOfType(typeof(Collider)))
        {
            Collider c = (Collider)obj;
            if (((layerMask & (1 << c.gameObject.layer)) != 0)
				&& (c.gameObject.active == true)
				&& (c.gameObject.isStatic == true)
				&& (c.isTrigger == false)
				&& (c is CharacterController == false)
				&& (c is WheelCollider == false))
				yield return c;
        }
    }

	private static BigCombineInstance MakeInstance(MeshCollider c, Bounds bounds)
	{
		Mesh m = c.sharedMesh;
        BigCombineInstance ci = new BigCombineInstance();
        ci.bigMesh = new BigMesh(m);
		ci.transform = c.gameObject.transform.localToWorldMatrix;
		return ci;
	}
	
	private static BigCombineInstance MakeInstance(TerrainCollider c, Bounds bounds)
	{
        Bounds localBounds = new Bounds(
			bounds.center - c.transform.position, 
			bounds.size);
        BigCombineInstance ci = new BigCombineInstance();
        ci.bigMesh = new BigMesh(((TerrainCollider)c).terrainData, localBounds);
        ci.transform = Matrix4x4.TRS(
			c.transform.position, 
			Quaternion.identity, 
			Vector3.one);
		return ci;
	}
	
	private static BigCombineInstance MakeInstance(BoxCollider c, Bounds bounds)
	{
		BigCombineInstance ci = new BigCombineInstance();
		ci.transform = c.gameObject.transform.localToWorldMatrix;
		ci.bigMesh = new BigMesh(PrimitiveType.Cube);
		ci.transform = ci.transform * Matrix4x4.TRS(
			c.center,
			Quaternion.identity,
			c.extents * 2);
		return ci;
	}
	
	private static BigCombineInstance MakeInstance(CapsuleCollider c, Bounds bounds)
	{
		BigCombineInstance ci = new BigCombineInstance();
		ci.transform = c.gameObject.transform.localToWorldMatrix;
		ci.bigMesh = new BigMesh(PrimitiveType.Capsule);
		
		Vector3 dir = Vector3.forward;
		if (c.direction == 0) 
			dir = Vector3.right;
		else if (c.direction == 1) 
			dir = Vector3.up;
		
		ci.transform = ci.transform * Matrix4x4.TRS(
			c.center,
			Quaternion.FromToRotation(Vector3.up, dir),
			new Vector3(
				c.radius / 0.5f,
				(c.height + 2 * c.radius) / 3,
				c.radius / 0.5f));
		return ci;
	}
	
	private static BigCombineInstance MakeInstance(SphereCollider c, Bounds bounds)
	{
		BigCombineInstance ci = new BigCombineInstance();
		ci.transform = c.gameObject.transform.localToWorldMatrix;
		ci.bigMesh = new BigMesh(PrimitiveType.Sphere);
		ci.transform = ci.transform * Matrix4x4.TRS(
			c.center,
			Quaternion.identity,
			Vector3.one * c.radius / 0.5f);
		return ci;
	}
	
    private static BigCombineInstance[] MakeCombineInstanceArray(IEnumerable<Collider> colliders, Bounds bounds)
    {
        List<BigCombineInstance> instances = new List<BigCombineInstance>();
        foreach(Collider c in colliders)
        {
            if(c.bounds.Intersects(bounds))
			{
				if (c is MeshCollider)
					instances.Add(MakeInstance((MeshCollider)c, bounds));
				else if(c is TerrainCollider)
					instances.Add(MakeInstance((TerrainCollider)c, bounds));
				else if (c is BoxCollider)
					instances.Add(MakeInstance((BoxCollider)c, bounds));
				else if (c is CapsuleCollider)
					instances.Add(MakeInstance((CapsuleCollider)c, bounds));
				else if (c is SphereCollider)
					instances.Add(MakeInstance((SphereCollider)c, bounds));
			}
        }
        return instances.ToArray();
    }

	public GameObject Generate()
	{
		byte[] data = this.BuildNavmesh();
		if (data != null)
		{
			GameObject go;
			go = new GameObject("Navmesh");
			Navmesh n = go.AddComponent<Navmesh>();
			n.SetData(data);
			return go;
		}
		return null;
	}
	
    void OnDrawGizmosSelected()
    {
        Color savedGizmoColor = Gizmos.color;
        Gizmos.color = new Color(145f, 244f, 139f, 255f) / 255f;
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = savedGizmoColor;
    }
	
    [DllImport("Navmesh_RecastDetour", EntryPoint="RetrieveNavmeshData")]
    private static extern void NativeRetrieveNavmeshData(
        [MarshalAs(UnmanagedType.LPArray)] byte[] buffer);
	
    [DllImport("Navmesh_RecastDetour", EntryPoint = "BuildNavmesh")]
    private static extern int NativeBuildNavmesh(
        int numVertices,
        [MarshalAs(UnmanagedType.LPArray)] Vector3[] vertices,
        int numIndices,
        [MarshalAs(UnmanagedType.LPArray)] int[] indices,
        float minX,
        float minY,
        float minZ,
        float maxX,
        float maxY,
        float maxZ,
        float cellSize,
        float cellHeight,
        float walkableHeight,
        float walkableSlopeAngle,
        float walkableClimb,
        float walkableRadius,
        float maxEdgeLen,
        float maxSimplificationError,
        bool monotonePartitioning,
        float minRegionArea,
        float mergeRegionArea,
        float detailSampleDist,
        float detailSampleMaxError,
        bool keepIntermediateData,
        int oneMillion);
}
