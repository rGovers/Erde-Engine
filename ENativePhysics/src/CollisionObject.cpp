// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionDispatch/btCollisionObject.h"

#include "Export.h"

EExportFunc(btCollisionObject*, CollisionObject_new());
EExportFunc(void, CollisionObject_delete(btCollisionObject* a_ptr));

EExportFunc(bool, CollisionObject_isStaticObject(btCollisionObject* a_ptr));
EExportFunc(bool, CollisionObject_isKinematicObject(btCollisionObject* a_ptr));
EExportFunc(bool, CollisionObject_isStaticOrKinematicObject(btCollisionObject* a_ptr));

EExportFunc(void, CollisionObject_setWorldTranslation(btCollisionObject* a_ptr, float a_x, float a_y, float a_z));
EExportFunc(void, CollisionObject_setWorldRotation(btCollisionObject* a_ptr, float a_x, float a_y, float a_z, float a_w)); 

EExportFunc(float*, CollisionObject_getTransformMatrix(btCollisionObject* a_ptr));
EExportFunc(void, CollisionObject_freeTransformMatrix(float* a_matrix)); 

EExportFunc(void, CollisionObject_setCollisionShape(btCollisionObject* a_ptr, btCollisionShape* a_shape));

btCollisionObject* CollisionObject_new() { return new btCollisionObject(); } 
void CollisionObject_delete(btCollisionObject* a_ptr) { delete a_ptr; }

bool CollisionObject_isStaticObject(btCollisionObject* a_ptr) { return a_ptr->isStaticObject(); }
bool CollisionObject_isKinematicObject(btCollisionObject* a_ptr) { return a_ptr->isKinematicObject(); } 
bool CollisionObject_isStaticOrKinematicObject(btCollisionObject* a_ptr) { return a_ptr->isStaticOrKinematicObject(); }

void CollisionObject_setWorldTranslation(btCollisionObject* a_ptr, float a_x, float a_y, float a_z) { a_ptr->getWorldTransform().setOrigin(btVector3(a_x, a_y, a_z)); }
void CollisionObject_setWorldRotation(btCollisionObject* a_ptr, float a_x, float a_y, float a_z, float a_w) { a_ptr->getWorldTransform().setRotation(btQuaternion(a_x, a_y, a_z, a_w)); }

float* CollisionObject_getTransformMatrix(btCollisionObject* a_ptr) { float* matrix = new float[16]; a_ptr->getWorldTransform().getOpenGLMatrix(matrix); return matrix; }
void CollisionObject_freeTransformMatrix(float* a_matrix) { delete[] a_matrix; }

void CollisionObject_setCollisionShape(btCollisionObject* a_ptr, btCollisionShape* a_shape) {  a_ptr->setCollisionShape(a_shape); }