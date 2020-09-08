// Custom motion state for Bullet Physics

#include "LinearMath/btMotionState.h"

#include "Export.h"

class EMotionState : public btMotionState
{
private:
	btTransform m_transform;

protected:

public:
	EMotionState()
	{
		m_transform = btTransform();
	}

	virtual void getWorldTransform(btTransform& a_transform) const
	{
		a_transform = m_transform;
	}
	virtual void setWorldTransform(const btTransform& a_transform)
	{
		m_transform = a_transform;
	}
};

EExport btMotionState* __cdecl MotionState_new() { return new EMotionState(); }
EExport void __cdecl MotionState_delete(btMotionState* a_ptr) { delete a_ptr; }

EExport void __cdecl MotionState_setTransform(btMotionState* a_ptr, float a_x, float a_y, float a_z, float a_rX, float a_rY, float a_rZ, float a_rW) { a_ptr->setWorldTransform(btTransform(btQuaternion(a_rX, a_rY, a_rZ, a_rW), btVector3(a_x, a_y, a_z))); }

EExport float* MotionState_getTransformMatrix(btMotionState* a_ptr) { float* matrix = new float[16]; btTransform transform; a_ptr->getWorldTransform(transform); transform.getOpenGLMatrix(matrix); return matrix; }
EExport void MotionState_freeTransformMatrix(float* a_matrix) { delete[] a_matrix; }