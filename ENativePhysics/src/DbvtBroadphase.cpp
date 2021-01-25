// C++ to C# wrapper functions

#include "BulletCollision/BroadphaseCollision/btDbvtBroadphase.h"

#include "Export.h"

EExportFunc(btDbvtBroadphase*, DbvtBroadphase_new()); 
EExportFunc(void, DbvtBroadphaser_delete(btDbvtBroadphase* a_ptr));

btDbvtBroadphase* DbvtBroadphase_new() { return new btDbvtBroadphase(); } 
void DbvtBroadphaser_delete(btDbvtBroadphase* a_ptr) { delete a_ptr; }