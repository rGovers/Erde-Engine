// Erde Extensions for Bullet

#include "ECollisionConfiguration.h"

#include "BulletCollision/CollisionDispatch/btBoxBoxCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btCompoundCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btCompoundCompoundCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btConvexConcaveCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btConvexConvexAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btConvexPlaneCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btEmptyCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btSphereSphereCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btSphereTriangleCollisionAlgorithm.h"
#include "BulletCollision/NarrowPhaseCollision/btGjkEpaPenetrationDepthSolver.h"
#include "BulletCollision/NarrowPhaseCollision/btVoronoiSimplexSolver.h"
#include "EDistanceFieldConvexAlgorithm.h"
#include "EDistanceFieldSphereAlgorithm.h"
#include "LinearMath/btPoolAllocator.h"

#include "Export.h"

ECollisionConfiguration::ECollisionConfiguration()
{
	void* mem = nullptr;

	mem = btAlignedAlloc(sizeof(btGjkEpaPenetrationDepthSolver), 16);
	m_pdSolver = new (mem) btGjkEpaPenetrationDepthSolver;

	mem = btAlignedAlloc(sizeof(btConvexConvexAlgorithm::CreateFunc), 16);
	m_convexConvexCreateFunc = new (mem) btConvexConvexAlgorithm::CreateFunc(m_pdSolver);
	mem = btAlignedAlloc(sizeof(btConvexConcaveCollisionAlgorithm::CreateFunc), 16);
	m_convexConcaveCreateFunc = new (mem) btConvexConcaveCollisionAlgorithm::CreateFunc;
	mem = btAlignedAlloc(sizeof(btConvexConcaveCollisionAlgorithm::CreateFunc), 16);
	m_swappedConvexConcaveCreateFunc = new (mem) btConvexConcaveCollisionAlgorithm::SwappedCreateFunc;
	mem = btAlignedAlloc(sizeof(btCompoundCollisionAlgorithm::CreateFunc), 16);
	m_compoundCreateFunc = new (mem) btCompoundCollisionAlgorithm::CreateFunc;

	mem = btAlignedAlloc(sizeof(btCompoundCompoundCollisionAlgorithm::CreateFunc), 16);
	m_compoundCompoundCreateFunc = new (mem) btCompoundCompoundCollisionAlgorithm::CreateFunc;

	mem = btAlignedAlloc(sizeof(btCompoundCollisionAlgorithm::SwappedCreateFunc), 16);
	m_swappedCompoundCreateFunc = new (mem) btCompoundCollisionAlgorithm::SwappedCreateFunc;
	mem = btAlignedAlloc(sizeof(btEmptyAlgorithm::CreateFunc), 16);
	m_emptyCreateFunc = new (mem) btEmptyAlgorithm::CreateFunc;

	mem = btAlignedAlloc(sizeof(btSphereSphereCollisionAlgorithm::CreateFunc), 16);
	m_sphereSphereCF = new (mem) btSphereSphereCollisionAlgorithm::CreateFunc;

	mem = btAlignedAlloc(sizeof(btSphereTriangleCollisionAlgorithm::CreateFunc), 16);
	m_sphereTriangleCF = new (mem) btSphereTriangleCollisionAlgorithm::CreateFunc;
	mem = btAlignedAlloc(sizeof(btSphereTriangleCollisionAlgorithm::CreateFunc), 16);
	m_triangleSphereCF = new (mem) btSphereTriangleCollisionAlgorithm::CreateFunc;
	m_triangleSphereCF->m_swapped = true;

	mem = btAlignedAlloc(sizeof(btBoxBoxCollisionAlgorithm::CreateFunc), 16);
	m_boxBoxCF = new (mem) btBoxBoxCollisionAlgorithm::CreateFunc;

	mem = btAlignedAlloc(sizeof(btConvexPlaneCollisionAlgorithm::CreateFunc), 16);
	m_convexPlaneCF = new (mem) btConvexPlaneCollisionAlgorithm::CreateFunc;
	mem = btAlignedAlloc(sizeof(btConvexPlaneCollisionAlgorithm::CreateFunc), 16);
	m_planeConvexCF = new (mem) btConvexPlaneCollisionAlgorithm::CreateFunc;
	m_planeConvexCF->m_swapped = true;

	mem = btAlignedAlloc(sizeof(EDistanceFieldSphereAlgorithm::CreateFunc), 16);
	m_distanceFieldSphereCF = new (mem) EDistanceFieldSphereAlgorithm::CreateFunc;
	mem = btAlignedAlloc(sizeof(EDistanceFieldSphereAlgorithm::CreateFunc), 16);
	m_sphereDistanceFieldCF = new (mem) EDistanceFieldSphereAlgorithm::CreateFunc;
	m_sphereDistanceFieldCF->m_swapped = true;

	mem = btAlignedAlloc(sizeof(EDistanceFieldConvexAlgorithm::CreateFunc), 16);
	m_distanceFieldConvexCF = new (mem) EDistanceFieldConvexAlgorithm::CreateFunc;
	mem = btAlignedAlloc(sizeof(EDistanceFieldConvexAlgorithm::CreateFunc), 16);
	m_convexDistanceFieldCF = new (mem) EDistanceFieldConvexAlgorithm::CreateFunc;
	m_convexDistanceFieldCF->m_swapped = true;
 
	int maxSize = sizeof(btConvexConvexAlgorithm);
	int maxSize2 = sizeof(btConvexConcaveCollisionAlgorithm);
	int maxSize3 = sizeof(btCompoundCollisionAlgorithm);
	int maxSize4 = sizeof(btCompoundCompoundCollisionAlgorithm);

	int collisionAlgorithmMaxElementSize = btMax(maxSize, btMax(maxSize2, btMax(maxSize3, maxSize4)));

	mem = btAlignedAlloc(sizeof(btPoolAllocator), 16);
	m_persistentManifoldPool = new (mem) btPoolAllocator(sizeof(btPersistentManifold), 4096);

	mem = btAlignedAlloc(sizeof(btPoolAllocator), 16);
	m_collisionAlgorithmPool = new (mem) btPoolAllocator(collisionAlgorithmMaxElementSize, 4096);


}
ECollisionConfiguration::~ECollisionConfiguration()
{
	m_collisionAlgorithmPool->~btPoolAllocator();
	btAlignedFree(m_collisionAlgorithmPool);
	m_persistentManifoldPool->~btPoolAllocator();
	btAlignedFree(m_persistentManifoldPool);

	m_convexConvexCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_convexConvexCreateFunc);

	m_convexConcaveCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_convexConcaveCreateFunc);
	m_swappedConvexConcaveCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_swappedConvexConcaveCreateFunc);

	m_compoundCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_compoundCreateFunc);

	m_compoundCompoundCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_compoundCompoundCreateFunc);

	m_swappedCompoundCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_swappedCompoundCreateFunc);

	m_emptyCreateFunc->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_emptyCreateFunc);

	m_sphereSphereCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_sphereSphereCF);

	m_sphereTriangleCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_sphereTriangleCF);
	m_triangleSphereCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_triangleSphereCF);
	m_boxBoxCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_boxBoxCF);

	m_convexPlaneCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_convexPlaneCF);
	m_planeConvexCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_planeConvexCF);

	m_distanceFieldSphereCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_distanceFieldSphereCF);
	m_sphereDistanceFieldCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_sphereDistanceFieldCF);

	m_distanceFieldConvexCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_distanceFieldConvexCF);
	m_convexDistanceFieldCF->~btCollisionAlgorithmCreateFunc();
	btAlignedFree(m_convexDistanceFieldCF);

	m_pdSolver->~btConvexPenetrationDepthSolver();
	btAlignedFree(m_pdSolver);
}

btPoolAllocator* ECollisionConfiguration::getPersistentManifoldPool()
{
	return m_persistentManifoldPool;
}
btPoolAllocator* ECollisionConfiguration::getCollisionAlgorithmPool()
{
	return m_collisionAlgorithmPool;
}

btCollisionAlgorithmCreateFunc* ECollisionConfiguration::getCollisionAlgorithmCreateFunc(int a_proxyType0, int a_proxyType1)
{
	if ((a_proxyType0 == SPHERE_SHAPE_PROXYTYPE) && (a_proxyType1 == CUSTOM_CONVEX_SHAPE_TYPE))
	{
		return m_sphereDistanceFieldCF;
	}
	if ((a_proxyType0 == CUSTOM_CONVEX_SHAPE_TYPE) && (a_proxyType1 == SPHERE_SHAPE_PROXYTYPE))
	{
		return m_distanceFieldSphereCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && (a_proxyType1 == CUSTOM_CONVEX_SHAPE_TYPE))
	{
		return m_convexDistanceFieldCF;
	}

	if ((a_proxyType0 == CUSTOM_CONVEX_SHAPE_TYPE) && btBroadphaseProxy::isConvex(a_proxyType1))
	{
		return m_distanceFieldConvexCF;
	}

	if ((a_proxyType0 == SPHERE_SHAPE_PROXYTYPE) && (a_proxyType1 == SPHERE_SHAPE_PROXYTYPE))
	{
		return m_sphereSphereCF;
	}

	if ((a_proxyType0 == SPHERE_SHAPE_PROXYTYPE) && (a_proxyType1 == TRIANGLE_SHAPE_PROXYTYPE))
	{
		return m_sphereTriangleCF;
	}

	if ((a_proxyType0 == TRIANGLE_SHAPE_PROXYTYPE) && (a_proxyType1 == SPHERE_SHAPE_PROXYTYPE))
	{
		return m_triangleSphereCF;
	}

	if ((a_proxyType0 == BOX_SHAPE_PROXYTYPE) && (a_proxyType1 == BOX_SHAPE_PROXYTYPE))
	{
		return m_boxBoxCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && (a_proxyType1 == STATIC_PLANE_PROXYTYPE))
	{
		return m_convexPlaneCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType1) && (a_proxyType0 == STATIC_PLANE_PROXYTYPE))
	{
		return m_planeConvexCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && btBroadphaseProxy::isConvex(a_proxyType1))
	{
		return m_convexConvexCreateFunc;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && btBroadphaseProxy::isConcave(a_proxyType1))
	{
		return m_convexConcaveCreateFunc;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType1) && btBroadphaseProxy::isConcave(a_proxyType0))
	{
		return m_swappedConvexConcaveCreateFunc;
	}

	if (btBroadphaseProxy::isCompound(a_proxyType0) && btBroadphaseProxy::isCompound(a_proxyType1))
	{
		return m_compoundCompoundCreateFunc;
	}

	if (btBroadphaseProxy::isCompound(a_proxyType0))
	{
		return m_compoundCreateFunc;
	}
	else
	{
		if (btBroadphaseProxy::isCompound(a_proxyType1))
		{
			return m_swappedCompoundCreateFunc;
		}
	}

	//failed to find an algorithm
	return m_emptyCreateFunc;
}
btCollisionAlgorithmCreateFunc* ECollisionConfiguration::getClosestPointsAlgorithmCreateFunc(int a_proxyType0, int a_proxyType1)
{
	if ((a_proxyType0 == SPHERE_SHAPE_PROXYTYPE) && (a_proxyType1 == CUSTOM_CONVEX_SHAPE_TYPE))
	{
		return m_sphereDistanceFieldCF;
	}
	if ((a_proxyType0 == CUSTOM_CONVEX_SHAPE_TYPE) && (a_proxyType1 == SPHERE_SHAPE_PROXYTYPE))
	{
		return m_distanceFieldSphereCF;
	}

	if ((a_proxyType0 == SPHERE_SHAPE_PROXYTYPE) && (a_proxyType1 == SPHERE_SHAPE_PROXYTYPE))
	{
		return m_sphereSphereCF;
	}

	if ((a_proxyType0 == SPHERE_SHAPE_PROXYTYPE) && (a_proxyType1 == TRIANGLE_SHAPE_PROXYTYPE))
	{
		return m_sphereTriangleCF;
	}

	if ((a_proxyType0 == TRIANGLE_SHAPE_PROXYTYPE) && (a_proxyType1 == SPHERE_SHAPE_PROXYTYPE))
	{
		return m_triangleSphereCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && (a_proxyType1 == STATIC_PLANE_PROXYTYPE))
	{
		return m_convexPlaneCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType1) && (a_proxyType0 == STATIC_PLANE_PROXYTYPE))
	{
		return m_planeConvexCF;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && btBroadphaseProxy::isConvex(a_proxyType1))
	{
		return m_convexConvexCreateFunc;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType0) && btBroadphaseProxy::isConcave(a_proxyType1))
	{
		return m_convexConcaveCreateFunc;
	}

	if (btBroadphaseProxy::isConvex(a_proxyType1) && btBroadphaseProxy::isConcave(a_proxyType0))
	{
		return m_swappedConvexConcaveCreateFunc;
	}

	if (btBroadphaseProxy::isCompound(a_proxyType0) && btBroadphaseProxy::isCompound(a_proxyType1))
	{
		return m_compoundCompoundCreateFunc;
	}

	if (btBroadphaseProxy::isCompound(a_proxyType0))
	{
		return m_compoundCreateFunc;
	}
	else
	{
		if (btBroadphaseProxy::isCompound(a_proxyType1))
		{
			return m_swappedCompoundCreateFunc;
		}
	}

	//failed to find an algorithm
	return m_emptyCreateFunc;
}

EExportFunc(ECollisionConfiguration*, ECollisionConfiguration_new()); 
EExportFunc(void, ECollisionConfiguration_delete(ECollisionConfiguration* a_ptr)); 

ECollisionConfiguration* ECollisionConfiguration_new() { return new ECollisionConfiguration(); }
void ECollisionConfiguration_delete(ECollisionConfiguration* a_ptr) { delete a_ptr; }