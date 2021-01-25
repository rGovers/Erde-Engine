// Erde extensions for Bullet

#include "EDistanceFieldCollider.h"

#include "Export.h"

#include <iostream>

EDistanceFieldCollider::EDistanceFieldCollider(void* a_distanceField, int a_stride, int a_width, int a_height, int a_depth, float a_spacing) : btConvexShape()
{
	m_distanceField = a_distanceField;

	m_stride = a_stride;

	m_width = a_width;
	m_height = a_height;
	m_depth = a_depth;

	m_spacing = a_spacing;

	m_scaling = btVector3(1, 1, 1);

	m_shapeType = CUSTOM_CONVEX_SHAPE_TYPE;

	m_margin = 0;
}
EDistanceFieldCollider::~EDistanceFieldCollider()
{

}

void EDistanceFieldCollider::getAabb(const btTransform& a_t, btVector3& a_aabbMin, btVector3& a_aabbMax) const
{
	const btVector3 translation = a_t.getOrigin();

	const float halfWidth = m_width * 0.5f * m_spacing;
	const float halfHeight = m_height * 0.5f * m_spacing;
	const float halfDepth = m_depth * 0.5f * m_spacing;

	a_aabbMin = translation + btVector3(-halfWidth, -halfHeight, -halfDepth);
	a_aabbMax = translation + btVector3(halfWidth, halfHeight, halfDepth);
}
void EDistanceFieldCollider::getAabbSlow(const btTransform& a_t, btVector3& a_aabbMin, btVector3& a_aabbMax) const
{
	getAabb(a_t, a_aabbMin, a_aabbMax);
}

void EDistanceFieldCollider::setLocalScaling(const btVector3& a_scaling)
{
	m_scaling = a_scaling;
}
const btVector3& EDistanceFieldCollider::getLocalScaling() const
{
	return m_scaling;
}

const char* EDistanceFieldCollider::getName() const
{
	return "EDistanceField";
}

btVector3 EDistanceFieldCollider::localGetSupportingVertex(const btVector3& a_vec) const
{
	const float dist = GetPoint(a_vec);

	const btVector3 norm = -GetNormal(a_vec);

	const btVector3 ver = ClampLocalVector(a_vec) + (norm * (dist + m_margin));

	return ver;
}
btVector3 EDistanceFieldCollider::localGetSupportingVertexWithoutMargin(const btVector3& a_vec) const
{
	const float dist = GetPoint(a_vec);

	const btVector3 norm = -GetNormal(a_vec);

	const btVector3 ver = ClampLocalVector(a_vec) + (norm * dist);

	return ver;
}
void EDistanceFieldCollider::batchedUnitVectorGetSupportingVertexWithoutMargin(const btVector3* a_vectors, btVector3* a_supportVerticesOut, int a_numVectors) const
{
	for (int i = 0; i < a_numVectors; ++i)
	{
		a_supportVerticesOut[i].setValue(0, 0, 0);
	}
}

void EDistanceFieldCollider::calculateLocalInertia(btScalar a_mass, btVector3& a_inertia) const
{
	a_inertia.setValue(0, 0, 0);
}

void EDistanceFieldCollider::setMargin(btScalar a_margin)
{
	m_margin = a_margin;
}
btScalar EDistanceFieldCollider::getMargin() const
{
	return m_margin;
}

btVector3 EDistanceFieldCollider::ClampLocalVector(const btVector3& a_pos) const
{
	const float halfWidth = m_width * 0.5f * m_spacing;
	const float halfHeight = m_height * 0.5f * m_spacing;
	const float halfDepth = m_depth * 0.5f * m_spacing;

	return btVector3
	{
		btMin(halfWidth, btMax(-halfWidth, a_pos.x())),
		btMin(halfHeight, btMax(-halfHeight, a_pos.y())),
		btMin(halfDepth, btMax(-halfDepth, a_pos.z()))
	};
}
float EDistanceFieldCollider::GetPoint(const btVector3& a_localPos) const
{
	const float halfWidth = m_width * m_spacing * 0.5f;
	const float halfHeight = m_height * m_spacing * 0.5f;
	const float halfDepth = m_depth * m_spacing * 0.5f;

	const int trueWidth = m_width - 1;
	const int trueHeight = m_height - 1;
	const int trueDepth = m_depth - 1;

	const float lerpX = (a_localPos.x() / m_spacing) - (int)(a_localPos.x() / m_spacing);
	const float lerpY = (a_localPos.y() / m_spacing) - (int)(a_localPos.y() / m_spacing);
	const float lerpZ = (a_localPos.z() / m_spacing) - (int)(a_localPos.z() / m_spacing);

	const int xA = btMin(trueWidth, btMax(0,  (int)((a_localPos.x() + halfWidth)  / m_spacing)));
	const int yA = btMin(trueHeight, btMax(0, (int)((a_localPos.y() + halfHeight) / m_spacing)));
	const int zA = btMin(trueDepth, btMax(0,  (int)((a_localPos.z() + halfDepth)  / m_spacing)));

	const int xB = btMin(trueWidth, xA + 1);
	const int yB = btMin(trueHeight, yA + 1);
	const int zB = btMin(trueDepth, zA + 1);

	const int index  = ((zA * m_width * m_height) + (yA * m_width) + xA) * m_stride;
	const int indexX = ((zA * m_width * m_height) + (yA * m_width) + xB) * m_stride;
	const int indexY = ((zA * m_width * m_height) + (yB * m_width) + xA) * m_stride;
	const int indexZ = ((zB * m_width * m_height) + (yA * m_width) + xA) * m_stride;

	const float dist = *(float*)((char*)m_distanceField + index);
	const float distX = *(float*)((char*)m_distanceField + indexX);
	const float distY = *(float*)((char*)m_distanceField + indexY);
	const float distZ = *(float*)((char*)m_distanceField + indexZ);

	const float mulX = distX - dist;
	const float mulY = distY - dist;
	const float mulZ = distZ - dist;

	const float finDist = dist + ((mulX * lerpX) + (mulY * lerpY) + (mulZ * lerpZ));

	const btVector3 diff = (btVector3(xA - halfWidth, yA - halfHeight, zA - halfDepth) * m_spacing) - a_localPos;

	return /*copysignf(diff.length(), finDist) +*/ finDist;
}
btVector3 EDistanceFieldCollider::GetNormal(const btVector3& a_localPos) const
{
	const float halfWidth = m_width * m_spacing * 0.5f;
	const float halfHeight = m_height * m_spacing * 0.5f;
	const float halfDepth = m_depth * m_spacing * 0.5f;

	const int x = btMin(m_width  - 1, btMax(0, (int)((a_localPos.x() + halfWidth)  / m_spacing)));
	const int y = btMin(m_height - 1, btMax(0, (int)((a_localPos.y() + halfHeight) / m_spacing)));
	const int z = btMin(m_depth  - 1, btMax(0, (int)((a_localPos.z() + halfDepth)  / m_spacing)));

	btVector3 vec = btVector3(0, 0, 0);

	for (int xI = 0; xI < 3; ++xI)
	{
		const int unitX = xI - 1;
		const int xInd = x + unitX;

		if (xInd >= m_width)
		{
			return btVector3(0, 1, 0);
		}

		if (xInd < 0)
		{
			return btVector3(0, 1, 0);
		}

		for (int yI = 0; yI < 3; ++yI)
		{
			const int unitY = yI - 1;
			const int yInd = y + unitY;

			if (yInd >= m_height)
			{
				return btVector3(0, 1, 0);
			}

			if (yInd <= 0)
			{
				return btVector3(0, 1, 0);
			}

			for (int zI = 0; zI < 3; ++zI)
			{
				const int unitZ = zI - 1;
				const int zInd = z + unitZ;

				if (zInd >= m_depth)
				{
					return btVector3(0, 1, 0);
				}

				if (zInd < 0)
				{
					return btVector3(0, 1, 0);
				}

				const int index = ((zInd * m_width * m_height) + (yInd * m_width) + xInd) * m_stride;

				vec += (btVector3(unitX, unitY, unitZ) * *(float*)((char*)m_distanceField + index));
			}
		}
	}

	return vec.normalized();
}

int EDistanceFieldCollider::getNumPreferredPenetrationDirections() const
{
	return 0;
}
void EDistanceFieldCollider::getPreferredPenetrationDirection(int a_index, btVector3& a_penetrationVector) const
{

}

EExportFunc(EDistanceFieldCollider*, DistanceFieldCollider_new(void* a_distanceField, int a_stride, int a_width, int a_height, int a_depth, float a_spacing)); 
EExportFunc(void, DistanceFieldCollider_delete(EDistanceFieldCollider* a_ptr)); 

EDistanceFieldCollider* DistanceFieldCollider_new(void* a_distanceField, int a_stride, int a_width, int a_height, int a_depth, float a_spacing) { return new EDistanceFieldCollider(a_distanceField, a_stride, a_width, a_height, a_depth, a_spacing); }
void DistanceFieldCollider_delete(EDistanceFieldCollider* a_ptr) { delete a_ptr; }