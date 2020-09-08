// C++ to C# wrapper functions

#include "BulletCollision/BroadphaseCollision/btDbvtBroadphase.h"

#include "Export.h"

EExport btDbvtBroadphase* __cdecl DbvtBroadphase_new() { return new btDbvtBroadphase(); } 
EExport void __cdecl DbvtBroadphaser_delete(btDbvtBroadphase* a_ptr) { delete a_ptr; }