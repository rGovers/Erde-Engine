// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionDispatch/btCollisionDispatcher.h"

#include "Export.h"

EExport btCollisionDispatcher* __cdecl CollisionDispatcher_new(btCollisionConfiguration* a_collisionConfiguration) { return new btCollisionDispatcher(a_collisionConfiguration); } 
EExport void __cdecl CollisionDispatcher_delete(btCollisionDispatcher* a_ptr) { delete a_ptr; }