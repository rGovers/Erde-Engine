// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionDispatch/btCollisionObject.h"

#include "Export.h"

EExport btCollisionObject* __cdecl CollisionObject_new() { return new btCollisionObject(); } 
EExport void __cdecl CollisionObject_delete(btCollisionObject* a_ptr) { delete a_ptr; }

EExport bool __cdecl CollisionObject_isStaticObject(btCollisionObject* a_ptr) { return a_ptr->isStaticObject(); }
EExport bool __cdecl CollisionObject_isKinematicObject(btCollisionObject* a_ptr) { return a_ptr->isKinematicObject(); }
EExport bool __cdecl CollisionObject_isStaticOrKinematicObject(btCollisionObject* a_ptr) { return a_ptr->isStaticOrKinematicObject(); }

EExport void __cdecl CollisionObject_setWorldTranslation(btCollisionObject* a_ptr, float a_x, float a_y, float a_z) { a_ptr->getWorldTransform().setOrigin(btVector3(a_x, a_y, a_z)); }
EExport void __cdecl CollisionObject_setWorldRotation(btCollisionObject* a_ptr, float a_x, float a_y, float a_z, float a_w) { a_ptr->getWorldTransform().setRotation(btQuaternion(a_x, a_y, a_z, a_w)); }

EExport float* __cdecl CollisionObject_getTransformMatrix(btCollisionObject* a_ptr) { float* matrix = new float[16]; a_ptr->getWorldTransform().getOpenGLMatrix(matrix); return matrix; }
EExport void __cdecl CollisionObject_freeTransformMatrix(float* a_matrix) { delete[] a_matrix; }

EExport void __cdecl CollisionObject_setCollisionShape(btCollisionObject* a_ptr, btCollisionShape* a_shape) {  a_ptr->setCollisionShape(a_shape); }