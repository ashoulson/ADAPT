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

#include <iostream>

#include "Steering.h"

float* Vector3ToFloat(Vector3 v)
{
	float* f = new float[3];
	f[0] = v.x;
	f[1] = v.y;
	f[2] = v.z;
	return f;
}

Vector3 FloatToVec3(const float* f)
{
	Vector3 v;
	v.x = f[0];
	v.y = f[1];
	v.z = f[2];
	return v;
}

bool SteeringManager::init(
	unsigned char* navMeshData, 
	int navMeshDataSize, 
	int maxAgents,
	float maxAgentRadius)
{
	if (!initNavMesh(navMeshData, navMeshDataSize))
		return false;
	if (!initQuery())
		return false;
	if (!initCrowd(maxAgents, maxAgentRadius))
		return false;
	return true;
}

void SteeringManager::update(float dT)
{
	crowd.update(dT, nullptr);
}

int SteeringManager::addAgent(
	Vector3 pos, 
	float radius,
	float height,
	float accel,
	float maxSpeed)
{
	dtCrowdAgentParams params;
	params.radius = radius;
	params.height = height;
	params.maxAcceleration = accel;
	params.maxSpeed = maxSpeed;
	params.collisionQueryRange = params.radius * 8.0f;
	params.pathOptimizationRange = params.radius * 30.0f;
	params.updateFlags = 0
		| DT_CROWD_ANTICIPATE_TURNS 
		| DT_CROWD_OPTIMIZE_VIS
		| DT_CROWD_OPTIMIZE_TOPO
		| DT_CROWD_OBSTACLE_AVOIDANCE
        | DT_CROWD_COLLISION_RESOLUTION
//		| DT_CROWD_AGENT_PRIORITIES
//		| DT_CROWD_SEPARATION
		;
	params.obstacleAvoidanceType = 3;
	params.separationWeight = 2.0f;

	return crowd.addAgent(Vector3ToFloat(pos), &params);
}

void SteeringManager::removeAgent(int agent)
{
	crowd.removeAgent(agent);
}

void SteeringManager::updateAgentNavigationQuality(int agent, NavigationQuality nq)
{
	dtCrowdAgentParams params = crowd.getAgent(agent)->params;
	switch(nq)
	{
	case NAVIGATIONQUALITY_LOW:
		{
			params.updateFlags &= ~0
				& ~DT_CROWD_ANTICIPATE_TURNS 
				& ~DT_CROWD_OPTIMIZE_VIS
				& ~DT_CROWD_OPTIMIZE_TOPO
				& ~DT_CROWD_OBSTACLE_AVOIDANCE
				& ~DT_CROWD_COLLISION_RESOLUTION
				;
		}
		break;

	case NAVIGATIONQUALITY_MED:
		{
			params.updateFlags |= 0	
				| DT_CROWD_COLLISION_RESOLUTION
				;
			params.updateFlags &= ~0
				& ~DT_CROWD_OBSTACLE_AVOIDANCE
				& ~DT_CROWD_ANTICIPATE_TURNS 
				& ~DT_CROWD_OPTIMIZE_VIS
				& ~DT_CROWD_OPTIMIZE_TOPO
				;
		}
		break;

	case NAVIGATIONQUALITY_HIGH:
		{
			params.obstacleAvoidanceType = 3;
			params.updateFlags |= 0	
				| DT_CROWD_ANTICIPATE_TURNS 
				| DT_CROWD_OPTIMIZE_VIS
				| DT_CROWD_OPTIMIZE_TOPO
				| DT_CROWD_OBSTACLE_AVOIDANCE
				| DT_CROWD_COLLISION_RESOLUTION
				;
		}
		break;
	}

	crowd.updateAgentParameters(agent, &params);
}

void SteeringManager::updateAgentPushiness(int agent, Pushiness pushiness)
{
	dtCrowdAgentParams params = crowd.getAgent(agent)->params;
	switch(pushiness)
	{
	case PUSHINESS_LOW:
		params.separationWeight = 4.0f;
		params.collisionQueryRange = params.radius * 16.0f;
		break;

	case PUSHINESS_MEDIUM:
		params.separationWeight = 2.0f;
		params.collisionQueryRange = params.radius * 8.0f;
		break;

	case PUSHINESS_HIGH:
		params.separationWeight = 0.5f;
		params.collisionQueryRange = params.radius * 1.0f;
		break;
	}
	crowd.updateAgentParameters(agent, &params);
}

void SteeringManager::updateAgentMaxSpeed(int agent, float speed)
{
	dtCrowdAgentParams params = crowd.getAgent(agent)->params;
	params.maxSpeed = speed;
	crowd.updateAgentParameters(agent, &params);
}

void SteeringManager::updateAgentMaxAcceleration(int agent, float accel)
{
	dtCrowdAgentParams params = crowd.getAgent(agent)->params;
	params.maxAcceleration = accel;
	crowd.updateAgentParameters(agent, &params);
}

void SteeringManager::setAgentTarget(int agent, Vector3 target)
{
	dtPolyRef polyRef;
	float nearestPos[3];
	dtStatus status = query.findNearestPoly(
		Vector3ToFloat(target),
		crowd.getQueryExtents(),
		crowd.getFilter(),
		&polyRef,
		nearestPos);

	if((status & DT_FAILURE) == 0)
	{
		if(!crowd.requestMoveTarget(agent, polyRef, nearestPos))
		{
			// TODO: Handle Failure (Couldn't request new target.)
		}
	}
	else
	{
		// TODO: Handle failure (Couldn't find nearest polygon.)
	}
}

void SteeringManager::setAgentMobile(int person, bool mobile)
{
	crowd.updateAgentState(person, 
		mobile ? DT_CROWDAGENT_STATE_WALKING : DT_CROWDAGENT_STATE_STANDING);
}

Vector3 SteeringManager::getAgentPosition(int person)
{
	return FloatToVec3(crowd.getAgent(person)->npos);
}

Vector3 SteeringManager::getAgentCurrentVelocity(int person)
{
	return FloatToVec3(crowd.getAgent(person)->vel);
}

Vector3 SteeringManager::getAgentDesiredVelocity(int person)
{
	return FloatToVec3(crowd.getAgent(person)->dvel);
}

Vector3 SteeringManager::getClosestWalkablePosition(Vector3 pos)
{
	float closest[3];
	const static float extents[] = { 1.0f, 20.0f, 1.0f };
	dtPolyRef closestPoly;
	dtQueryFilter filter;
	dtStatus status = query.findNearestPoly(
		Vector3ToFloat(pos),
		extents, 
		&filter, 
		&closestPoly, 
		closest);
	return FloatToVec3(closest);
}

bool SteeringManager::initNavMesh(unsigned char* navmeshData, int navmeshDataSize)
{
	dtStatus status = navMesh.init(navmeshData, navmeshDataSize, 0);
	if (status & DT_FAILURE)
		return false;
	return true;
}

bool SteeringManager::initQuery()
{
	dtStatus status = query.init(&navMesh, 4096);
	if (status & DT_FAILURE)
		return false;
	return true;
}

bool SteeringManager::initCrowd(int maxAgents, float maxAgentRadius)
{
	bool result = crowd.init(maxAgents, maxAgentRadius, &navMesh);
	if (result == false)
		return false;

	// Use mostly default settings, copy from dtCrowd
	dtObstacleAvoidanceParams params;
	memcpy(
		&params, 
		crowd.getObstacleAvoidanceParams(0), 
		sizeof(dtObstacleAvoidanceParams));

	params.weightSide = 0.0f;
	params.weightCurVel = 0.0f;

	// Low (11)
	params.adaptiveDivs = 5;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 1;
	crowd.setObstacleAvoidanceParams(0, &params);

	// Medium (22)
	params.adaptiveDivs = 5;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 2;
	crowd.setObstacleAvoidanceParams(1, &params);

	// Good (45)
	params.adaptiveDivs = 7;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 3;
	crowd.setObstacleAvoidanceParams(2, &params);

	// High (66)
	params.adaptiveDivs = 7;
	params.adaptiveRings = 3;
	params.adaptiveDepth = 3;
	crowd.setObstacleAvoidanceParams(3, &params);

	return true;
}



/*void NavigationManager::HoldPersonAtCurrentPoint(int person)
{
	dtPolyRef polyRef;

	float nearestPos[3];

	query.findNearestPoly(
		crowd.getAgent(person)->npos,
		crowd.getQueryExtents(),
		crowd.getFilter(),
		&polyRef,
		nearestPos);

	crowd.requestMoveTarget(person, polyRef, nearestPos);
}*/


