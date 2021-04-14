// Erde Extensions for Bullet

#pragma once

#include "BulletCollision/CollisionDispatch/btCollisionConfiguration.h"

class btConvexPenetrationDepthSolver;

class ECollisionConfiguration : public btCollisionConfiguration
{
private:
	btConvexPenetrationDepthSolver* m_pdSolver;

	btCollisionAlgorithmCreateFunc* m_convexConvexCreateFunc;
	btCollisionAlgorithmCreateFunc* m_convexConcaveCreateFunc;
	btCollisionAlgorithmCreateFunc* m_swappedConvexConcaveCreateFunc;
	btCollisionAlgorithmCreateFunc* m_compoundCreateFunc;
	btCollisionAlgorithmCreateFunc* m_compoundCompoundCreateFunc;

	btCollisionAlgorithmCreateFunc* m_swappedCompoundCreateFunc;
	btCollisionAlgorithmCreateFunc* m_emptyCreateFunc;

	btCollisionAlgorithmCreateFunc* m_sphereSphereCF;
	btCollisionAlgorithmCreateFunc* m_boxBoxCF;

	btCollisionAlgorithmCreateFunc* m_sphereTriangleCF;
	btCollisionAlgorithmCreateFunc* m_triangleSphereCF;
	btCollisionAlgorithmCreateFunc* m_planeConvexCF;
	btCollisionAlgorithmCreateFunc* m_convexPlaneCF;

	btCollisionAlgorithmCreateFunc* m_distanceFieldSphereCF;
	btCollisionAlgorithmCreateFunc* m_sphereDistanceFieldCF;

	btCollisionAlgorithmCreateFunc* m_distanceFieldConvexCF;
	btCollisionAlgorithmCreateFunc* m_convexDistanceFieldCF;

	btPoolAllocator*				m_persistentManifoldPool;
	btPoolAllocator*				m_collisionAlgorithmPool;

protected:

public:
	ECollisionConfiguration();
	~ECollisionConfiguration();

	virtual btPoolAllocator* getPersistentManifoldPool();
	virtual btPoolAllocator* getCollisionAlgorithmPool();

	virtual btCollisionAlgorithmCreateFunc* getCollisionAlgorithmCreateFunc(int a_proxyType0, int a_proxyType1);
	virtual btCollisionAlgorithmCreateFunc* getClosestPointsAlgorithmCreateFunc(int a_proxyType0, int a_proxyType1);
};