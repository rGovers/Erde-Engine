// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionDispatch/btDefaultCollisionConfiguration.h"

#include "Export.h"

EExport btDefaultCollisionConfiguration* __cdecl DefaultCollisionConfiguration_new() { return new btDefaultCollisionConfiguration(); } 
EExport void __cdecl DefaultCollisionConfiguration_delete(btDefaultCollisionConfiguration* a_ptr) { delete a_ptr; }