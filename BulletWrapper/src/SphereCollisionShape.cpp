// C++ to C# wrapper functions

#include "BulletCollision/CollisionShapes/btSphereShape.h"

#include "Export.h"

EExport btSphereShape* __cdecl SphereCollisionShape_new(float a_radius) { return new btSphereShape(a_radius); }
EExport void __cdecl SphereCollisionShape_delete(btSphereShape* a_ptr) { delete a_ptr; }