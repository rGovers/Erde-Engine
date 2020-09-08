// Erde extensions for Bullet

#pragma once

#include "BulletCollision/CollisionShapes/btConvexShape.h"

class EDistanceFieldCollider : public btConvexShape
{
private:
	void*	  m_distanceField;
			  
	int		  m_stride;
			  
	int		  m_width;
	int		  m_height;
	int		  m_depth;
			  
	float	  m_spacing;

	btVector3 m_scaling;

	float     m_margin;

protected:

public:
	EDistanceFieldCollider(void* a_distanceField, int a_stride, int a_width, int a_height, int a_depth, float a_spacing);
    ~EDistanceFieldCollider();

	virtual void getAabb(const btTransform& a_t, btVector3& a_aabbMin, btVector3& a_aabbMax) const;
	virtual void getAabbSlow(const btTransform& a_t, btVector3& a_aabbMin, btVector3& a_aabbMax) const;

    virtual void setLocalScaling(const btVector3& a_scaling);
	virtual const btVector3& getLocalScaling() const;

	virtual const char* getName() const;

	virtual btVector3 localGetSupportingVertex(const btVector3& a_vec) const;
	virtual btVector3 localGetSupportingVertexWithoutMargin(const btVector3& a_vec) const;
	virtual void batchedUnitVectorGetSupportingVertexWithoutMargin(const btVector3* a_vectors, btVector3* a_supportVerticesOut, int a_numVectors) const;

    virtual void calculateLocalInertia(btScalar mass, btVector3 & inertia) const;

	virtual void setMargin(btScalar a_margin);
	virtual btScalar getMargin() const;

	float GetPoint(const btVector3& a_localPos) const;
	btVector3 GetNormal(const btVector3& a_localPos) const;

	btVector3 ClampLocalVector(const btVector3& a_pos) const;

	virtual int getNumPreferredPenetrationDirections() const;
	virtual void getPreferredPenetrationDirection(int a_index, btVector3& a_penetrationVector) const;
};

