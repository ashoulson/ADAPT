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

#include <Recast.h>
#include <DetourNavMesh.h>
#include <DetourNavMeshBuilder.h>
#include <string.h>
#include <math.h>
#include "Navmesh.h"
#include <algorithm>

unsigned char* g_navData = NULL;
int g_navDataSize = 0;

float* providedVertices = NULL;
int* providedIndices = NULL;
int numProvidedVertices = 0;
int numProvidedIndices = 0;

rcHeightfield* solid = NULL;
unsigned char* triAreas = NULL;
rcCompactHeightfield* chf = NULL;
rcContourSet* cset = NULL;
rcPolyMesh* pmesh = NULL;
rcPolyMeshDetail* dmesh = NULL;

void FreeIntermediateData()
{
	if(providedVertices)
	{
		delete [] providedVertices;
		numProvidedVertices = 0;
	}
	if(providedIndices)
	{
		delete [] providedIndices;
		numProvidedIndices = 0;
	}
	if(solid)
	{
		rcFreeHeightField(solid);
		solid = NULL;
	}
	if(triAreas)
	{
		delete [] triAreas;
		triAreas = NULL;
	}
	if(chf)
	{
		rcFreeCompactHeightfield(chf);
		chf = NULL;
	}
	if(cset)
	{
		rcFreeContourSet(cset);
		cset = NULL;
	}
	if(pmesh != NULL)
	{
		rcFreePolyMesh(pmesh);
		pmesh = NULL;
	}
	if(dmesh != NULL)
	{
		rcFreePolyMeshDetail(dmesh);
		dmesh = NULL;
	}
}

EXPORT int BuildNavmesh(
	int numVertices,
	float* vertices,
	int numIndices,
	int* indices,
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
	bool keepIntermediate,
	int oneMillion)
{
	if(oneMillion != 1000000) return -1;

	FreeIntermediateData();

	if(keepIntermediate)
	{
		providedVertices = new float[numVertices*3];
		std::copy(vertices, vertices+numVertices*3, providedVertices);
		numProvidedVertices = numVertices;

		providedIndices = new int[numIndices];
		std::copy(indices, indices+numIndices, providedIndices);
		numProvidedIndices = numIndices;
	}

	rcContext ctx;
	rcConfig cfg;
	memset(&cfg, 0, sizeof(cfg));

	// Rasterization
	cfg.cs = cellSize;
	cfg.ch = cellHeight;

	// Agent
	cfg.walkableHeight = (int)ceilf(walkableHeight / cellHeight);
	cfg.walkableRadius = (int)ceilf(walkableRadius / cellSize);
	cfg.walkableClimb = (int)floorf(walkableClimb / cellHeight);
	cfg.walkableSlopeAngle = walkableSlopeAngle;

	// Region
	cfg.minRegionArea = (int)floorf(minRegionArea / (cellSize*cellSize));
	cfg.mergeRegionArea = (int)floorf(mergeRegionArea / (cellSize*cellSize));

	// Polygonization
	cfg.maxEdgeLen = (int)(maxEdgeLen / cellSize);
	cfg.maxSimplificationError = maxSimplificationError;
	cfg.maxVertsPerPoly = DT_VERTS_PER_POLYGON;

	// Detail Mesh
	cfg.detailSampleDist = detailSampleDist < 0.9f ? 0 : cellSize * detailSampleDist;
	cfg.detailSampleMaxError = cellHeight * detailSampleMaxError;
	
	cfg.bmin[0] = minX;
	cfg.bmin[1] = minY;
	cfg.bmin[2] = minZ;
	cfg.bmax[0] = maxX;
	cfg.bmax[1] = maxY;
	cfg.bmax[2] = maxZ;

	rcCalcGridSize(cfg.bmin, cfg.bmax, cfg.cs, &cfg.width, &cfg.height);

	solid = rcAllocHeightfield();
	if(!rcCreateHeightfield(&ctx, *solid, cfg.width, cfg.height, cfg.bmin, cfg.bmax, cfg.cs, cfg.ch))
	{
		return -2;
	}

	int ntris = numIndices / 3;

	unsigned char* triAreas = new unsigned char[ntris];
	memset(triAreas, 0, ntris);

	rcMarkWalkableTriangles(&ctx, cfg.walkableSlopeAngle, vertices, numVertices, indices, ntris, triAreas);
	rcRasterizeTriangles(&ctx, vertices, numVertices, indices, triAreas, ntris, *solid, cfg.walkableClimb);

	rcFilterLowHangingWalkableObstacles(&ctx, cfg.walkableClimb, *solid);
	rcFilterLedgeSpans(&ctx, cfg.walkableHeight, cfg.walkableClimb, *solid);
	rcFilterWalkableLowHeightSpans(&ctx, cfg.walkableHeight, *solid);

	chf = rcAllocCompactHeightfield();

	if(!rcBuildCompactHeightfield(&ctx, cfg.walkableHeight, cfg.walkableClimb, *solid, *chf))
	{
		return -3;
	}


	if (!rcErodeWalkableArea(&ctx, cfg.walkableRadius, *chf))
	{
		return -4;
	}

	if(!rcBuildDistanceField(&ctx, *chf))
	{
		return -5;
	}

	if(monotonePartitioning)
	{
		if(!rcBuildRegionsMonotone(&ctx, *chf, 0, cfg.minRegionArea, cfg.mergeRegionArea))
		{
			return -6;
		}
	}
	else
	{
		if(!rcBuildRegions(&ctx, *chf, 0, cfg.minRegionArea, cfg.mergeRegionArea))
		{
			return -6;
		}
	}

	cset = rcAllocContourSet();

	if(!rcBuildContours(&ctx, *chf, cfg.maxSimplificationError, cfg.maxEdgeLen, *cset))
	{
		return -7;
	}

	pmesh = rcAllocPolyMesh();

	if(!rcBuildPolyMesh(&ctx, *cset, cfg.maxVertsPerPoly, *pmesh))
	{
		return -8;
	}

	dmesh = rcAllocPolyMeshDetail();

	if(!rcBuildPolyMeshDetail(&ctx, *pmesh, *chf, cfg.detailSampleDist, cfg.detailSampleMaxError, *dmesh))
	{
		return -9;
	}

	for (int i = 0; i < pmesh->npolys; ++i)
	{
		if (pmesh->areas[i] == RC_WALKABLE_AREA)
		{
			pmesh->flags[i] = -1;
		}
		else
		{
			pmesh->flags[i] = 0;
		}
	}

	dtNavMeshCreateParams params;
	memset(&params, 0, sizeof(params));
	params.verts = pmesh->verts;
	params.vertCount = pmesh->nverts;
	params.polys = pmesh->polys;
	params.polyAreas = pmesh->areas;
	params.polyFlags = pmesh->flags;
	params.polyCount = pmesh->npolys;
	params.nvp = pmesh->nvp;
	params.detailMeshes = dmesh->meshes;
	params.detailVerts = dmesh->verts;
	params.detailVertsCount = dmesh->nverts;
	params.detailTris = dmesh->tris;
	params.detailTriCount = dmesh->ntris;
	/*
	params.offMeshConVerts = m_geom->getOffMeshConnectionVerts();
	params.offMeshConRad = m_geom->getOffMeshConnectionRads();
	params.offMeshConDir = m_geom->getOffMeshConnectionDirs();
	params.offMeshConAreas = m_geom->getOffMeshConnectionAreas();
	params.offMeshConFlags = m_geom->getOffMeshConnectionFlags();
	params.offMeshConUserID = m_geom->getOffMeshConnectionId();
	params.offMeshConCount = m_geom->getOffMeshConnectionCount();
	*/
	params.walkableHeight = walkableHeight;
	params.walkableRadius = walkableRadius;
	params.walkableClimb = walkableClimb;
	rcVcopy(params.bmin, pmesh->bmin);
	rcVcopy(params.bmax, pmesh->bmax);
	params.cs = cfg.cs;
	params.ch = cfg.ch;
	params.buildBvTree = true;

	if(!dtCreateNavMeshData(&params, &g_navData, &g_navDataSize))
	{
		return -10;
	}
	
	if(!keepIntermediate)
	{
		FreeIntermediateData();
	}

	return g_navDataSize;
}

EXPORT void RetrieveNavmeshData(unsigned char* buffer)
{
	memcpy(buffer, g_navData, g_navDataSize);
	dtFree(g_navData);
	g_navData = NULL;
	g_navDataSize = 0;
}


