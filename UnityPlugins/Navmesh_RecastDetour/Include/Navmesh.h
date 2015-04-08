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

#ifndef NAVMESH_H
#define NAVMESH_H

#include <Recast.h>
#include <DetourNavMesh.h>
#include <DetourNavMeshQuery.h>

extern float* providedVertices;
extern int* providedIndices;
extern int numProvidedVertices;
extern int numProvidedIndices;

extern rcHeightfield* solid;
extern unsigned char* triAreas;
extern rcCompactHeightfield* chf;
extern rcContourSet* cset;
extern rcPolyMesh* pmesh;
extern rcPolyMeshDetail* dmesh;

#define EXPORT extern "C" __declspec(dllexport)

EXPORT dtNavMesh* DebugInitNavmesh(unsigned char* data, int dataSize);
EXPORT void DebugDestroyNavmesh(dtNavMesh* nm);

EXPORT void RetrieveNavmeshData(unsigned char* buffer);
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
	int oneMillion);

#endif