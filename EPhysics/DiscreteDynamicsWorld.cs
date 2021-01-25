using Erde.Physics.Broadphase;
using Erde.Physics.Configuration;
using Erde.Physics.Solver;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Erde.Physics
{
    internal class DiscreteDynamicsWorld : IDisposable
    {
        class BtDiscreteDynamicsWorld
        {
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DiscreteDynamicsWorld_new (IntPtr a_dispatcher, IntPtr a_broadphase, IntPtr a_solver, IntPtr a_configuration);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_delete (IntPtr a_ptr);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_setGravity (IntPtr a_ptr, float a_x, float a_y, float a_z);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_stepSimulation (IntPtr a_ptr, float a_timeStep, int a_maxSteps, float a_fixedTimeStep);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_addRigidBody (IntPtr a_ptr, IntPtr a_rigidBody);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_removeRigidBody (IntPtr a_ptr, IntPtr a_rigidBody);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_addCollisionObject (IntPtr a_ptr, IntPtr a_collisionObject);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_removeCollisionObject (IntPtr a_ptr, IntPtr a_collisionObject);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DiscreteDynamicsWorld_raycastClosest (IntPtr a_ptr, float a_xF, float a_yF, float a_zF, float a_xT, float a_yT, float a_zT, out IntPtr a_object, out float a_xN, out float a_yN, out float a_zN, out float a_xP, out float a_yP, out float a_zP);
        }

        Vector3                             m_gravity;

        IntPtr                              m_objectPtr;

        Dictionary<IntPtr, CollisionObject> m_objectLookup;

        public Vector3 Gravity
        {
            get
            {
                return m_gravity;
            }
            set
            {
                m_gravity = value;

                BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_setGravity(m_objectPtr, m_gravity.X, m_gravity.Y, m_gravity.Z);
            }
        }

        public DiscreteDynamicsWorld (CollisionDispatcher a_dispatcher, BroadphaseInterface a_broadphase, ConstraintSolver a_solver, CollisionConfiguration a_configuration)
        {
            m_objectPtr = BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_new(a_dispatcher.Ptr, a_broadphase.Ptr, a_solver.Ptr, a_configuration.Ptr);

            m_objectLookup = new Dictionary<IntPtr, CollisionObject>();
        }

        internal void AddRigidbody (Rigidbody a_rigidbody)
        {
            IntPtr ptr = a_rigidbody.Ptr;

            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_addRigidBody(m_objectPtr, ptr);

            m_objectLookup.Add(ptr, a_rigidbody);
        }
        internal void RemoveRigidbody (Rigidbody a_rigidbody)
        {
            IntPtr ptr = a_rigidbody.Ptr;

            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_removeRigidBody(m_objectPtr, ptr);

            m_objectLookup.Remove(ptr);
        }

        internal void AddCollisionObject (CollisionObject a_collisionObject)
        {
            IntPtr ptr = a_collisionObject.Ptr;

            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_addCollisionObject(m_objectPtr, ptr);

            m_objectLookup.Add(ptr, a_collisionObject);
        }
        internal void RemoveCollisionObject (CollisionObject a_collisionObject)
        {
            IntPtr ptr = a_collisionObject.Ptr;

            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_removeCollisionObject(m_objectPtr, ptr);

            m_objectLookup.Remove(ptr);
        }

        public void Update ()
        {
            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_stepSimulation(m_objectPtr, (float)PhysicsTime.DeltaTime, 10, 1.0f / 60.0f);
        }

        public bool RaycastClosest (Vector3 a_from, Vector3 a_to, out RaycastResultClosest a_result)
        {
            a_result = new RaycastResultClosest();

            // Saves time if there is no objects there is no possible way for it to hit anything
            if (m_objectLookup.Count <= 0)
            {
                return false;
            }

            IntPtr ptr;

            float nX;
            float nY;
            float nZ;

            float pX;
            float pY;
            float pZ;

            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_raycastClosest(m_objectPtr, a_from.X, a_from.Y, a_from.Z, a_to.X, a_to.Y, a_to.Z, out ptr, out nX, out nY, out nZ, out pX, out pY, out pZ);

            a_result.HitObject = null;

            if (ptr != IntPtr.Zero)
            {
                a_result.HitPosition = new Vector3(pX, pY, pZ);
                a_result.HitNormal = new Vector3(nX, nY, nZ);

                a_result.HitObject = m_objectLookup[ptr];

                return true;
            }

            return false;
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            BtDiscreteDynamicsWorld.DiscreteDynamicsWorld_delete(m_objectPtr);
        }

        ~DiscreteDynamicsWorld ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
