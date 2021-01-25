// C++ to C# wrapper functions

#include "BulletDynamics/ConstraintSolver/btSequentialImpulseConstraintSolver.h"

#include "Export.h"

EExportFunc(btSequentialImpulseConstraintSolver*, SequentialImpulseConstraintSolver_new()); 
EExportFunc(void, SequentialImpulseConstraintSolver_delete(btSequentialImpulseConstraintSolver* a_ptr)); 

btSequentialImpulseConstraintSolver* SequentialImpulseConstraintSolver_new() { return new btSequentialImpulseConstraintSolver(); } 
void SequentialImpulseConstraintSolver_delete(btSequentialImpulseConstraintSolver* a_ptr) { delete a_ptr; }