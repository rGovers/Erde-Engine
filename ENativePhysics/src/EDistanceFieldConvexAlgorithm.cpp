// Erde Extensions for Bullet

#include "EDistanceFieldConvexAlgorithm.h"

#include "BulletCollision/CollisionShapes/btSphereShape.h"
#include "EDistanceFieldCollider.h"

// #define CLEAR_MANIFOLD 1

EDistanceFieldConvexAlgorithm::EDistanceFieldConvexAlgorithm(btPersistentManifold* a_mf, const btCollisionAlgorithmConstructionInfo& a_ci, const btCollisionObjectWrapper* a_col0Wrap, const btCollisionObjectWrapper* a_col1Wrap, bool a_swapped) :
    btActivatingCollisionAlgorithm(a_ci, a_col0Wrap, a_col1Wrap)
{
    m_swapped = a_swapped;

    if (m_swapped)
    {
        m_manifoldPtr = m_dispatcher->getNewManifold(a_col1Wrap->getCollisionObject(), a_col0Wrap->getCollisionObject());
    }
    else
    {
        m_manifoldPtr = m_dispatcher->getNewManifold(a_col0Wrap->getCollisionObject(), a_col1Wrap->getCollisionObject());
    }

    m_dispatcher->clearManifold(m_manifoldPtr);
}
EDistanceFieldConvexAlgorithm::EDistanceFieldConvexAlgorithm(const btCollisionAlgorithmConstructionInfo& a_ci) :
    btActivatingCollisionAlgorithm(a_ci)
{
    
}
EDistanceFieldConvexAlgorithm::~EDistanceFieldConvexAlgorithm()
{
    m_dispatcher->clearManifold(m_manifoldPtr);
    m_dispatcher->releaseManifold(m_manifoldPtr);
}

void EDistanceFieldConvexAlgorithm::processCollision(const btCollisionObjectWrapper* a_body0Wrap, const btCollisionObjectWrapper* a_body1Wrap, const btDispatcherInfo& a_dispatchInfo, btManifoldResult* a_resultOut)
{
    if (!m_manifoldPtr)
    {
        return;
    }

    a_resultOut->setPersistentManifold(m_manifoldPtr);

    const EDistanceFieldCollider* distanceField = (EDistanceFieldCollider*)(m_swapped ? a_body1Wrap->getCollisionShape() : a_body0Wrap->getCollisionShape());
    const btConvexShape* convex = (btConvexShape*)(m_swapped ? a_body0Wrap->getCollisionShape() : a_body1Wrap->getCollisionShape());

    const btVector3 posA = m_swapped ? a_body1Wrap->getWorldTransform().getOrigin() : a_body0Wrap->getWorldTransform().getOrigin();
    const btVector3 posB = m_swapped ? a_body0Wrap->getWorldTransform().getOrigin() : a_body1Wrap->getWorldTransform().getOrigin();

    const btVector3 diff = posB - posA;

    const float distance = distanceField->GetPoint(diff);
    const btVector3 norm = distanceField->GetNormal(diff);

    const btVector3 vertex = convex->localGetSupportingVertex(diff - norm);

    const float rad = (posB - vertex).length();

#ifdef CLEAR_MANIFOLD
	m_manifoldPtr->clearManifold();  //don't do this, it disables warmstarting
#endif

	if (distance > rad + a_resultOut->m_closestPointDistanceThreshold)
	{
#ifndef CLEAR_MANIFOLD
		a_resultOut->refreshContactPoints();
#endif  //CLEAR_MANIFOLD
		return;
	}

    ///distance (negative means penetration)
	const float dist = distance - rad;

	///point on A (worldspace)
	///btVector3 pos0 = col0->getWorldTransform().getOrigin() - radius0 * normalOnSurfaceB;
	///point on B (worldspace)
	const btVector3 pos1 = posB + rad * -norm;

	/// report a contact. internally this will be kept persistent, and contact reduction is done
	a_resultOut->addContactPoint(-norm, pos1, dist);

#ifndef CLEAR_MANIFOLD
	a_resultOut->refreshContactPoints();
#endif  //CLEAR_MANIFOLD
}

btScalar EDistanceFieldConvexAlgorithm::calculateTimeOfImpact(btCollisionObject* a_body0, btCollisionObject* a_body1, const btDispatcherInfo& a_dispatchInfo, btManifoldResult* a_resultOut)
{
    return 1.0f;
}

btCollisionAlgorithm* EDistanceFieldConvexAlgorithm::CreateFunc::CreateCollisionAlgorithm(btCollisionAlgorithmConstructionInfo& a_ci, const btCollisionObjectWrapper* a_col0Wrap, const btCollisionObjectWrapper* a_col1Wrap)
{
	void* mem = a_ci.m_dispatcher1->allocateCollisionAlgorithm(sizeof(EDistanceFieldConvexAlgorithm));
	return new (mem) EDistanceFieldConvexAlgorithm(0, a_ci, a_col0Wrap, a_col1Wrap, m_swapped);
}

void EDistanceFieldConvexAlgorithm::getAllContactManifolds(btManifoldArray& a_manifoldArray)
{
    if (m_manifoldPtr)
    {
        a_manifoldArray.push_back(m_manifoldPtr);
    }
}