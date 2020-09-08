// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionShapes/btCapsuleShape.h"

#include "Export.h"

EExport btCapsuleShape* __cdecl CapsuleCollisionShape_new(float a_radius, float a_height) { return new btCapsuleShape(a_radius, a_height); }
EExport void __cdecl CapsuleCollisionShape_delete(btCapsuleShape* a_ptr) { delete a_ptr; }