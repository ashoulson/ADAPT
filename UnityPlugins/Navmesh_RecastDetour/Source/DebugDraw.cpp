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

#include <DebugDraw.h>
#include <RecastDebugDraw.h>
#include <DetourDebugDraw.h>
#include <DetourCommon.h>
#include "Navmesh.h"

#include <vector>

static const float DD_SIZE_MULTIPLIER = 0.002f;

class UnityDebugDraw : public duDebugDraw
{
public:
	UnityDebugDraw() {}
	virtual ~UnityDebugDraw() {}

	void InitDrawing(const float* screenUp, const float* screenRight, const float* screenOut)
	{
		vertices.clear();
		normals.clear();
		colors.clear();
		uvs.clear();
		indices.clear();

		dtVcopy(this->screenUp, screenUp);
		dtVcopy(this->screenRight, screenRight);
		dtVcopy(this->screenOut, screenOut);
	}
	int GetNumVertices() { return vertices.size()/3; }
	int GetNumIndices() { return indices.size(); }

	void RetrieveMesh(float* vertices, float* colors, float* uvs, float* normals, int* indices)
	{
		std::copy(this->vertices.begin(), this->vertices.end(), vertices);
		std::copy(this->colors.begin(), this->colors.end(), colors);
		std::copy(this->uvs.begin(), this->uvs.end(), uvs);
		std::copy(this->normals.begin(), this->normals.end(), normals);
		std::copy(this->indices.begin(), this->indices.end(), indices);
	}

	virtual void depthMask(bool state) { }
	virtual void texture(bool state) { }

	// Begin drawing primitives.
	// Params:
	//  prim - (in) primitive type to draw, one of rcDebugDrawPrimitives.
	//  nverts - (in) number of vertices to be submitted.
	//  size - (in) size of a primitive, applies to point size and line width only.
	virtual void begin(duDebugDrawPrimitives prim, float size = 1.0f)
	{
		currentPrim = prim;
		currentSize = size;
		verticesInPrim = 0;
	}

	// Submit a vertex
	// Params:
	//  pos - (in) position of the verts.
	//  color - (in) color of the verts.
	virtual void vertex(const float* pos, unsigned int color)
	{
		static const float uv[] = {0, 0};
		vertex(pos, color, uv);
	}

	// Submit a vertex
	// Params:
	//  x,y,z - (in) position of the verts.
	//  color - (in) color of the verts.
	virtual void vertex(const float x, const float y, const float z, unsigned int color)
	{
		const float pos[] = {x, y, z};
		static const float uv[] = {0, 0};
		vertex(pos, color, uv);
	}

	// Submit a vertex
	// Params:
	//  x,y,z - (in) position of the verts.
	//  color - (in) color of the verts.
	virtual void vertex(const float x, const float y, const float z, unsigned int color, const float u, const float v)
	{
		const float pos[] = {x, y, z};
		const float uv[] = {u, v};
		vertex(pos, color, uv);
	}

	// Submit a vertex
	// Params:
	//  pos - (in) position of the verts.
	//  color - (in) color of the verts.
	virtual void vertex(const float* pos, unsigned int color, const float* uv)
	{
		const static float norms[] = {0,0,0};
		switch(currentPrim)
		{
		case DU_DRAW_POINTS:
			doPoint(pos, color);
			break;
		case DU_DRAW_LINES:
			if(++verticesInPrim == 2)
			{
				doLine(lineVertex, lineColor, pos, color);
				verticesInPrim = 0;
			}
			else
			{
				dtVcopy(lineVertex, pos);
				lineColor = color;
			}
			break;

		case DU_DRAW_TRIS:
			doVertex(pos, color, uv, norms);
			if(++verticesInPrim == 3)
			{
				makeTriangle(-1, -2, -3);
				verticesInPrim = 0;
			}
			break;

		case DU_DRAW_QUADS:
			doVertex(pos, color, uv, norms);
			if(++verticesInPrim == 4)
			{
				makeTriangle(-1, -2, -3);
				makeTriangle(-1, -3, -4);
				verticesInPrim = 0;
			}
			break;
		}

	}

	void DrawProvidedGeometry() 
	{
		begin(DU_DRAW_TRIS);
		for(int i=0; i<numProvidedIndices; i++)
		{
			vertex(providedVertices + providedIndices[i]*3, duRGBAf(1,0,0,1));
		}
		end();
	}

	// End drawing primitives.
	virtual void end()
	{

	}

private:
	void doPoint(const float* pos, unsigned int color)
	{
		/*
		const static float uv[] = {0,0};
		const static float norms[4][3] = {
			{ -currentSize*DD_SIZE_MULTIPLIER, 0, 0 },
			{ 0, -currentSize*DD_SIZE_MULTIPLIER, 0 },
			{ currentSize*DD_SIZE_MULTIPLIER, 0, 0 },
			{ 0, currentSize*DD_SIZE_MULTIPLIER, 0 }
		};
		for(int i=0; i<4; i++)
		{
			doVertex(pos, color, uv, norms[i]);
		}
		makeTriangle(-1, -2, -3);
		makeTriangle(-1, -3, -4);
		*/
	}

	void doLine(const float* p1, const int c1, const float* p2, const int c2)
	{
		const static float uv[] = {0,0};
		const static float norms[4][3] = {
			{ -currentSize*DD_SIZE_MULTIPLIER, 0, 0 },
			{ currentSize*DD_SIZE_MULTIPLIER, 0, 0 },
			{ 0, -currentSize*DD_SIZE_MULTIPLIER, 0 },
			{ 0, currentSize*DD_SIZE_MULTIPLIER, 0 },
		};
		doVertex(p1, c1, uv, norms[0]);
		doVertex(p2, c2, uv, norms[0]);
		doVertex(p2, c2, uv, norms[1]);
		doVertex(p1, c1, uv, norms[1]);
		makeTriangle(-4, -3, -2);
		makeTriangle(-2, -1, -4);/*
		doVertex(p1, c1, uv, norms[2]);
		doVertex(p2, c2, uv, norms[2]);
		doVertex(p2, c2, uv, norms[3]);
		doVertex(p1, c1, uv, norms[3]);
		makeTriangle(-4, -3, -2);
		makeTriangle(-2, -1, -4);*/
	}

	void doVertex(const float* pos, unsigned int color, const float* uv, const float* norms)
	{
		vertices.resize(vertices.size()+3);
		dtVcopy(&vertices[vertices.size()-3], pos);
		colors.resize(colors.size()+4);
		ExtractColor(color, &colors[colors.size()-4]);
		uvs.push_back(uv[0]);
		uvs.push_back(uv[1]);
		normals.resize(normals.size()+3);
		dtVcopy(&normals[normals.size()-3], norms);
	}
	/*
	void doVertex(const float x, const float y, const float z, unsigned int color)
	{
		vertices.resize(vertices.size()+3);
		vertices[vertices.size()-3] = x;
		vertices[vertices.size()-2] = y;
		vertices[vertices.size()-1] = z;
		colors.resize(colors.size()+4);
		ExtractColor(color, &colors[colors.size()-4]);
		uvs.resize(uvs.size()+2, 0);
	}
	*/
	static void ExtractColor(unsigned int color, float* rgba)
	{
		rgba[0] = ((color >> 0) & 0xff) / 255.0f;
		rgba[1] = ((color >> 8) & 0xff) / 255.0f;
		rgba[2] = ((color >> 16) & 0xff) / 255.0f;
		rgba[3] = ((color >> 24) & 0xff) / 255.0f;
	}

	void makeTriangle(int offA, int offB, int offC)
	{
		int base = vertices.size()/3;
		indices.resize(indices.size()+3);
		indices[indices.size()-3] = base+offA;
		indices[indices.size()-2] = base+offB;
		indices[indices.size()-1] = base+offC;
	}

	duDebugDrawPrimitives currentPrim;
	float currentSize;
	int verticesInPrim;

	float lineVertex[3];
	int lineColor;

	float screenUp[3];
	float screenRight[3];
	float screenOut[3];

	std::vector<float> vertices;
	std::vector<float> normals;
	std::vector<float> colors;
	std::vector<float> uvs;
	std::vector<int> indices;
};

UnityDebugDraw unityDebugDraw;

enum DebugDrawType
{
	DDT_None = 0,
	DDT_ProvidedGeometry,
	DDT_HeightfieldSolid,
	DDT_HeightfieldWalkable,
	DDT_CompactHeightfieldSolid,
	DDT_CompactHeightfieldRegions,
	DDT_CompactHeightfieldDistance,
	DDT_RawContours,
	DDT_Contours,
	DDT_PolyMesh,
	DDT_PolyMeshDetail,
	DDT_NavMesh
};

EXPORT void DebugDrawNavmesh(dtNavMesh* navMesh, float* screenUp, float* screenRight, float* screenOut, int* outNumVertices, int* outNumIndices)
{
	unityDebugDraw.InitDrawing(screenUp, screenRight, screenOut);
	duDebugDrawNavMesh(&unityDebugDraw, *navMesh, 0);
	*outNumVertices = unityDebugDraw.GetNumVertices();
	*outNumIndices = unityDebugDraw.GetNumIndices();
}

EXPORT void DebugDrawIntermediate(DebugDrawType ddt, float* screenUp, float* screenRight, float* screenOut, int* outNumVertices, int* outNumIndices)
{
	unityDebugDraw.InitDrawing(screenUp, screenRight, screenOut);

	switch(ddt)
	{
	case DDT_ProvidedGeometry:
		if(providedVertices && providedIndices)
		{
			unityDebugDraw.DrawProvidedGeometry();
		}
		break;
	case DDT_HeightfieldSolid: 
		if(solid) {
			duDebugDrawHeightfieldSolid(&unityDebugDraw, *solid);
		}
		break;
	case DDT_HeightfieldWalkable: 
		if(solid)
		{
			duDebugDrawHeightfieldWalkable(&unityDebugDraw, *solid); 
		}
		break;
	case DDT_CompactHeightfieldSolid: 
		if(chf)
		{
			duDebugDrawCompactHeightfieldSolid(&unityDebugDraw, *chf); 
		}
		break;
	case DDT_CompactHeightfieldRegions: 
		if(chf)
		{
			duDebugDrawCompactHeightfieldRegions(&unityDebugDraw, *chf); 
		}
		break;
	case DDT_CompactHeightfieldDistance: 
		if(chf)
		{
			duDebugDrawCompactHeightfieldDistance(&unityDebugDraw, *chf); 
		}
		break;
	case DDT_RawContours: 
		if(cset)
		{
			duDebugDrawRawContours(&unityDebugDraw, *cset); 
		}
		break;
	case DDT_Contours: 
		if(cset)
		{
			duDebugDrawContours(&unityDebugDraw, *cset); 
		}
		break;
	case DDT_PolyMesh: 
		if(pmesh)
		{
			duDebugDrawPolyMesh(&unityDebugDraw, *pmesh); 
		}
		break;
	case DDT_PolyMeshDetail: 
		if(dmesh)
		{
			duDebugDrawPolyMeshDetail(&unityDebugDraw, *dmesh); 
		}
		break;
	}

	*outNumVertices = unityDebugDraw.GetNumVertices();
	*outNumIndices = unityDebugDraw.GetNumIndices();
}

EXPORT void RetrieveDebugDrawData(float* vertices, float* colors, float* uvs, float* normals, int* indices)
{
	unityDebugDraw.RetrieveMesh(vertices, colors, uvs, normals, indices);
}

EXPORT dtNavMesh* DebugInitNavmesh(unsigned char* data, int dataSize)
{
	dtNavMesh* nm = new dtNavMesh;
	if(!nm->init(data, dataSize, 0))
	{
		delete nm;
		return nullptr;
	}
	return nm;
}

EXPORT void DebugDestroyNavmesh(dtNavMesh* nm)
{
	delete nm;
}