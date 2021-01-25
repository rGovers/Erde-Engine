// C++ to C# Wrapper functions for Bullet

#include "BulletDynamics/Dynamics/btDiscreteDynamicsWorld.h"

#include "Export.h"

#include <iostream>

EExportFunc(btDiscreteDynamicsWorld*, DiscreteDynamicsWorld_new(btCollisionDispatcher* a_dispatcher, btBroadphaseInterface* a_broadphase, btConstraintSolver* a_solver, btCollisionConfiguration* a_configuration));
EExportFunc(void, DiscreteDynamicsWorld_delete(btDiscreteDynamicsWorld* a_ptr)); 

EExportFunc(void, DiscreteDynamicsWorld_setGravity(btDiscreteDynamicsWorld* a_ptr, float a_x, float a_y, float a_z));
EExportFunc(void, DiscreteDynamicsWorld_stepSimulation(btDiscreteDynamicsWorld* a_ptr, float a_timeStep, int a_maxSteps, float a_fixedTimeStep));

EExportFunc(void, DiscreteDynamicsWorld_addRigidBody(btDiscreteDynamicsWorld* a_ptr, btRigidBody* a_rigidBody));
EExportFunc(void, DiscreteDynamicsWorld_removeRigidBody(btDiscreteDynamicsWorld* a_ptr, btRigidBody* a_rigidBody)); 

EExportFunc(void, DiscreteDynamicsWorld_addCollisionObject(btDiscreteDynamicsWorld* a_ptr, btCollisionObject* a_collisionObject)); 
EExportFunc(void, DiscreteDynamicsWorld_removeCollisionObject(btDiscreteDynamicsWorld* a_ptr, btCollisionObject* a_collisionObject));

EExportFunc(void, DiscreteDynamicsWorld_raycastClosest(btDiscreteDynamicsWorld* a_ptr, float a_xF, float a_yF, float a_zF, float a_xT, float a_yT, float a_zT, const btCollisionObject*& a_object, float& a_xN, float& a_yN, float& a_zN, float& a_xP, float& a_yP, float& a_zP));

btDiscreteDynamicsWorld* DiscreteDynamicsWorld_new(btCollisionDispatcher* a_dispatcher, btBroadphaseInterface* a_broadphase, btConstraintSolver* a_solver, btCollisionConfiguration* a_configuration) { return new btDiscreteDynamicsWorld(a_dispatcher, a_broadphase, a_solver, a_configuration); } 
void DiscreteDynamicsWorld_delete(btDiscreteDynamicsWorld* a_ptr) { delete a_ptr; }

void DiscreteDynamicsWorld_setGravity(btDiscreteDynamicsWorld* a_ptr, float a_x, float a_y, float a_z) { a_ptr->setGravity(btVector3(a_x, a_y, a_z)); }
void DiscreteDynamicsWorld_stepSimulation(btDiscreteDynamicsWorld* a_ptr, float a_timeStep, int a_maxSteps, float a_fixedTimeStep) { a_ptr->stepSimulation(a_timeStep, a_maxSteps, a_fixedTimeStep); }

void DiscreteDynamicsWorld_addRigidBody(btDiscreteDynamicsWorld* a_ptr, btRigidBody* a_rigidBody) { a_ptr->addRigidBody(a_rigidBody); }
void DiscreteDynamicsWorld_removeRigidBody(btDiscreteDynamicsWorld* a_ptr, btRigidBody* a_rigidBody) { a_ptr->removeRigidBody(a_rigidBody); }

void DiscreteDynamicsWorld_addCollisionObject(btDiscreteDynamicsWorld* a_ptr, btCollisionObject* a_collisionObject) { a_ptr->addCollisionObject(a_collisionObject); }
void DiscreteDynamicsWorld_removeCollisionObject(btDiscreteDynamicsWorld* a_ptr, btCollisionObject* a_collisionObject)  { a_ptr->removeCollisionObject(a_collisionObject); }

void DiscreteDynamicsWorld_raycastClosest(btDiscreteDynamicsWorld* a_ptr, float a_xF, float a_yF, float a_zF, float a_xT, float a_yT, float a_zT, const btCollisionObject*& a_object, float& a_xN, float& a_yN, float& a_zN, float& a_xP, float& a_yP, float& a_zP)
{ 
	const btVector3 from = btVector3(a_xF, a_yF, a_zF);
	const btVector3 to = btVector3(a_xT, a_yT, a_zT);

	btCollisionWorld::ClosestRayResultCallback callback = btCollisionWorld::ClosestRayResultCallback(from, to);

	a_ptr->rayTest(from, to, callback);

	a_object = callback.m_collisionObject;

	if (a_object != nullptr)
	{
		a_xN = callback.m_hitNormalWorld.x();
		a_yN = callback.m_hitNormalWorld.y();
		a_zN = callback.m_hitNormalWorld.z();

		a_xP = callback.m_hitPointWorld.x();
		a_yP = callback.m_hitPointWorld.y();
		a_zP = callback.m_hitPointWorld.z();
	}
}