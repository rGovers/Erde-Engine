// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionDispatch/btCollisionDispatcher.h"

#include "Export.h"

EExportFunc(btCollisionDispatcher*, CollisionDispatcher_new(btCollisionConfiguration* a_collisionConfiguration));
EExportFunc(void, CollisionDispatcher_delete(btCollisionDispatcher* a_ptr));

btCollisionDispatcher* CollisionDispatcher_new(btCollisionConfiguration* a_collisionConfiguration) { return new btCollisionDispatcher(a_collisionConfiguration); } 
void CollisionDispatcher_delete(btCollisionDispatcher* a_ptr) { delete a_ptr; }