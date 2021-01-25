// C++ to C# Wrapper functions for Bullet

#include "BulletCollision/CollisionDispatch/btDefaultCollisionConfiguration.h"

#include "Export.h"

EExportFunc(btDefaultCollisionConfiguration*, DefaultCollisionConfiguration_new()); 
EExportFunc(void, DefaultCollisionConfiguration_delete(btDefaultCollisionConfiguration* a_ptr));

btDefaultCollisionConfiguration* DefaultCollisionConfiguration_new() { return new btDefaultCollisionConfiguration(); } 
void DefaultCollisionConfiguration_delete(btDefaultCollisionConfiguration* a_ptr) { delete a_ptr; }