// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionShapes/btBoxShape.h"

#include "Export.h"

EExportFunc(btBoxShape*, BoxCollisionShape_new(float a_halfX, float a_halfY, float a_halfZ));
EExportFunc(void, BoxCollisionShape_delete(btBoxShape* a_ptr));

btBoxShape* BoxCollisionShape_new(float a_halfX, float a_halfY, float a_halfZ) { return new btBoxShape(btVector3(a_halfX, a_halfY, a_halfZ)); }
void BoxCollisionShape_delete(btBoxShape* a_ptr) { delete a_ptr; }