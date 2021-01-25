// C++ to C# wrapper functions

#include "BulletDynamics/Dynamics/btRigidBody.h"

#include "Export.h"

EExportFunc(btRigidBody*, Rigidbody_new(float a_mass, btMotionState* a_motionState, btCollisionShape* a_collisionShape)); 
EExportFunc(void, Rigidbody_delete(btRigidBody* a_ptr)); 

EExportFunc(void, Rigidbody_setMass(btRigidBody* a_ptr, float a_mass));

EExportFunc(void, Rigidbody_getForce(btRigidBody* a_ptr, float& a_x, float& a_y, float& a_z));
EExportFunc(void, Rigidbody_getTorque(btRigidBody* a_ptr, float& a_x, float& a_y, float& a_z)); 

EExportFunc(void, Rigidbody_applyForce(btRigidBody* a_ptr, float a_vX, float a_vY, float a_vZ, float a_pX, float a_pY, float a_pZ));
EExportFunc(void, Rigidbody_applyForceCentral(btRigidBody* a_ptr, float a_x, float a_y, float a_z));

EExportFunc(void, Rigidbody_applyTorque(btRigidBody* a_ptr, float a_x, float a_y, float a_z)); 
EExportFunc(void, Rigidbody_applyTorqueImpulse(btRigidBody* a_ptr, float a_x, float a_y, float a_z)); 

EExportFunc(void, Rigidbody_applyImpulse(btRigidBody* a_ptr, float a_iX, float a_iY, float a_iZ, float a_pX, float a_pY, float a_pZ)); 
EExportFunc(void, Rigidbody_applyImpulseCentral(btRigidBody* a_ptr, float a_x, float a_y, float a_z)); 

btRigidBody* Rigidbody_new(float a_mass, btMotionState* a_motionState, btCollisionShape* a_collisionShape) { return new btRigidBody(a_mass, a_motionState, a_collisionShape); } 
void Rigidbody_delete(btRigidBody* a_ptr) { delete a_ptr; }

void Rigidbody_setMass(btRigidBody* a_ptr, float a_mass)  { a_ptr->setMassProps(a_mass, btVector3(0, 0, 0)); }

void Rigidbody_getForce(btRigidBody* a_ptr, float& a_x, float& a_y, float& a_z) { btVector3 vec = a_ptr->getTotalForce(); a_x = vec.x(); a_y = vec.y(); a_z = vec.z(); }
void Rigidbody_getTorque(btRigidBody* a_ptr, float& a_x, float& a_y, float& a_z) { btVector3 vec = a_ptr->getTotalTorque(); a_x = vec.x(); a_y = vec.y(); a_z = vec.z(); }

void Rigidbody_applyForce(btRigidBody* a_ptr, float a_vX, float a_vY, float a_vZ, float a_pX, float a_pY, float a_pZ) { a_ptr->applyForce(btVector3(a_vX, a_vY, a_vZ), btVector3(a_pX, a_pY, a_pZ)); }
void Rigidbody_applyForceCentral(btRigidBody* a_ptr, float a_x, float a_y, float a_z) { a_ptr->applyCentralForce(btVector3(a_x, a_y, a_z)); }

void Rigidbody_applyTorque(btRigidBody* a_ptr, float a_x, float a_y, float a_z) { a_ptr->applyTorque(btVector3(a_x, a_y, a_z)); }
void Rigidbody_applyTorqueImpulse(btRigidBody* a_ptr, float a_x, float a_y, float a_z) { a_ptr->applyTorqueImpulse(btVector3(a_x, a_y, a_z)); }

void Rigidbody_applyImpulse(btRigidBody* a_ptr, float a_iX, float a_iY, float a_iZ, float a_pX, float a_pY, float a_pZ) { a_ptr->applyImpulse(btVector3(a_iX, a_iY, a_iZ), btVector3(a_pX, a_pY, a_pZ)); }
void Rigidbody_applyImpulseCentral(btRigidBody* a_ptr, float a_x, float a_y, float a_z) { a_ptr->applyCentralImpulse(btVector3(a_x, a_y, a_z)); }