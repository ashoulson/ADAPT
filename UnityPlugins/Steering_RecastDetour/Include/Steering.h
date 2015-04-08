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

#ifndef NAVIGATIONMANAGER_H
#define NAVIGATIONMANAGER_H

#include <DetourNavMesh.h>
#include <DetourCrowd.h>
#include <DetourNavMeshQuery.h>

struct Vector3
{
	float x, y, z;
};

enum NavigationQuality 
{
	NAVIGATIONQUALITY_LOW,
	NAVIGATIONQUALITY_MED,
	NAVIGATIONQUALITY_HIGH
};

enum Pushiness 
{
	PUSHINESS_LOW,
	PUSHINESS_MEDIUM,
	PUSHINESS_HIGH
};

class SteeringManager
{
public:
	bool init(unsigned char* navMeshData, int navMeshDataSize, int maxAgents, float maxAgentRadius);
	void update(float dT);

	int addAgent(Vector3 pos, float radius, float height, float accel, float maxSpeed);
	void removeAgent(int agent);

	void updateAgentNavigationQuality(int agent, NavigationQuality nq);
	void updateAgentPushiness(int agent, Pushiness pushiness);
	void updateAgentMaxSpeed(int agent, float maxSpeed);
	void updateAgentMaxAcceleration(int agent, float accel);

	void setAgentTarget(int agent, Vector3 target);
	void setAgentMobile(int agent, bool mobile);

	Vector3 getAgentPosition(int agent);
	Vector3 getAgentCurrentVelocity(int agent);
	Vector3 getAgentDesiredVelocity(int agent);

	Vector3 getClosestWalkablePosition(Vector3 pos);

private:
	dtNavMesh navMesh;
	dtNavMeshQuery query;
	dtCrowd crowd;

	bool initNavMesh(unsigned char* navmeshData, int navmeshDataSize);
	bool initQuery();
	bool initCrowd(int maxAgents, float maxAgentRadius);
};

#endif