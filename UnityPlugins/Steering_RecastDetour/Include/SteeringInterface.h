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

#include "Steering.h"
#define EXPORT extern "C" __declspec(dllexport)

EXPORT SteeringManager* createSteeringManager() 
{ 
	return new SteeringManager; 
}

EXPORT void destroySteeringManager(SteeringManager* manager) 
{ 
	delete manager; 
}

// Everything below here just exposes the public class functionality
EXPORT bool init(SteeringManager* manager,unsigned char* navMeshData, 
	int navMeshDataSize, int maxAgents, float maxAgentRadius)
{
	return manager->init(navMeshData, navMeshDataSize, maxAgents, maxAgentRadius);
}

EXPORT void update(SteeringManager* manager, float dT)
{
	manager->update(dT);
}

EXPORT int addAgent(SteeringManager* manager, Vector3 pos, 
	float radius, float height, float accel, float maxSpeed)
{
	return manager->addAgent(pos, radius, height, accel, maxSpeed);
}

EXPORT void removeAgent(SteeringManager* manager, int agent)
{
	manager->removeAgent(agent);
}

EXPORT void updateAgentNavigationQuality(
	SteeringManager* manager, int agent, NavigationQuality nq)
{
	manager->updateAgentNavigationQuality(agent, nq);
}

EXPORT void updateAgentPushiness(
	SteeringManager* manager, int agent, Pushiness pushiness)
{
	manager->updateAgentPushiness(agent, pushiness);
}

EXPORT void updateAgentMaxSpeed(
	SteeringManager* manager, int agent, float speed)
{
	manager->updateAgentMaxSpeed(agent, speed);
}

EXPORT void updateAgentMaxAcceleration(
	SteeringManager* manager, int agent, float accel)
{
	manager->updateAgentMaxAcceleration(agent, accel);
}

EXPORT void setAgentTarget(
	SteeringManager* manager, int agent, Vector3 pos)
{
	manager->setAgentTarget(agent, pos);
}

EXPORT void setAgentMobile(
	SteeringManager* manager, int agent, bool mobile)
{
	manager->setAgentMobile(agent, mobile);
}

EXPORT Vector3 getAgentPosition(
	SteeringManager* manager, const int agent)
{
	return manager->getAgentPosition(agent);
}

EXPORT Vector3 getAgentCurrentVelocity(
	SteeringManager* manager, const int agent)
{
	return manager->getAgentCurrentVelocity(agent);
}

EXPORT Vector3 getAgentDesiredVelocity(
	SteeringManager* manager, const int agent)
{
	return manager->getAgentDesiredVelocity(agent);
}

EXPORT Vector3 getClosestWalkablePosition(
	SteeringManager* manager, Vector3 pos)
{
	return manager->getClosestWalkablePosition(pos);
}