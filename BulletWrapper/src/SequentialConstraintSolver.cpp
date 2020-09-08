// C++ to C# wrapper functions

#include "BulletDynamics/ConstraintSolver/btSequentialImpulseConstraintSolver.h"

#include "Export.h"

EExport btSequentialImpulseConstraintSolver* __cdecl SequentialImpulseConstraintSolver_new() { return new btSequentialImpulseConstraintSolver(); } 
EExport void __cdecl SequentialImpulseConstraintSolver_delete(btSequentialImpulseConstraintSolver* a_ptr) { delete a_ptr; }