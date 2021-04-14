// Erde Extensions for Bullet

#pragma once

#include "BulletCollision/CollisionDispatch/btActivatingCollisionAlgorithm.h"
#include "BulletCollision/CollisionDispatch/btCollisionCreateFunc.h"
#include "BulletCollision/CollisionDispatch/btCollisionDispatcher.h"

class EDistanceFieldConvexAlgorithm : public btActivatingCollisionAlgorithm
{
private:
	btPersistentManifold* m_manifoldPtr;

    bool                  m_swapped;
protected:

public:
    EDistanceFieldConvexAlgorithm(btPersistentManifold* a_mf, const btCollisionAlgorithmConstructionInfo& a_ci, const btCollisionObjectWrapper* a_col0Wrap, const btCollisionObjectWrapper* a_col1Wrap, bool a_swapped);
    EDistanceFieldConvexAlgorithm(const btCollisionAlgorithmConstructionInfo& a_ci);
    virtual ~EDistanceFieldConvexAlgorithm();

    virtual void processCollision(const btCollisionObjectWrapper* a_body0Wrap, const btCollisionObjectWrapper* a_body1Wrap, const btDispatcherInfo& a_dispatchInfo, btManifoldResult* a_resultOut);

	virtual btScalar calculateTimeOfImpact(btCollisionObject* a_body0, btCollisionObject* a_body1, const btDispatcherInfo& a_dispatchInfo, btManifoldResult* a_resultOut);

	virtual void getAllContactManifolds(btManifoldArray& a_manifoldArray);

    struct CreateFunc : public btCollisionAlgorithmCreateFunc
	{
		virtual btCollisionAlgorithm* CreateCollisionAlgorithm(btCollisionAlgorithmConstructionInfo& a_ci, const btCollisionObjectWrapper* a_col0Wrap, const btCollisionObjectWrapper* a_col1Wrap);
	};
};