// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionShapes/btCapsuleShape.h"

#include "Export.h"

EExportFunc(btCapsuleShape*, CapsuleCollisionShape_new(float a_radius, float a_height));
EExportFunc(void, CapsuleCollisionShape_delete(btCapsuleShape* a_ptr));

btCapsuleShape* CapsuleCollisionShape_new(float a_radius, float a_height) { return new btCapsuleShape(a_radius, a_height); }
void CapsuleCollisionShape_delete(btCapsuleShape* a_ptr) { delete a_ptr; }