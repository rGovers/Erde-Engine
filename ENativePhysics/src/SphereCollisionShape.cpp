// C++ to C# wrapper functions

#include "BulletCollision/CollisionShapes/btSphereShape.h"

#include "Export.h"

EExportFunc(btSphereShape*, SphereCollisionShape_new(float a_radius));
EExportFunc(void, SphereCollisionShape_delete(btSphereShape* a_ptr)); 

btSphereShape* SphereCollisionShape_new(float a_radius)  { return new btSphereShape(a_radius); }
void SphereCollisionShape_delete(btSphereShape* a_ptr) { delete a_ptr; }