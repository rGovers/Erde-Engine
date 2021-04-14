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
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DrawLineEvent(float a_xA, float a_yA, float a_zA, float a_xB, float a_yB, float a_zB, float a_r, float a_g, float a_b, float a_a);
        // public delegate void DrawLineEvent();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ErrorEvent(IntPtr a_str);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CollisionCallback(IntPtr a_objA, IntPtr a_objB, float a_xA, float a_yA, float a_zA, float a_xB, float a_yB, float a_zB, float a_nX, float a_nY, float a_nZ);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr DiscreteDynamicsWorld_new (IntPtr a_dispatcher, IntPtr a_broadphase, IntPtr a_solver, IntPtr a_configuration);
        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_delete (IntPtr a_ptr);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_setGravity (IntPtr a_ptr, float a_x, float a_y, float a_z);
        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_stepSimulation (IntPtr a_ptr, float a_timeStep, int a_maxSteps, float a_fixedTimeStep);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_debugDrawWorld (IntPtr a_ptr);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_addRigidBody (IntPtr a_ptr, IntPtr a_rigidBody);
        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_removeRigidBody (IntPtr a_ptr, IntPtr a_rigidBody);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_addCollisionObject (IntPtr a_ptr, IntPtr a_collisionObject);
        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_removeCollisionObject (IntPtr a_ptr, IntPtr a_collisionObject);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_raycastClosest (IntPtr a_ptr, float a_xF, float a_yF, float a_zF, float a_xT, float a_yT, float a_zT, out IntPtr a_object, out float a_xN, out float a_yN, out float a_zN, out float a_xP, out float a_yP, out float a_zP);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_checkCollidingObjects(IntPtr a_ptr, CollisionCallback a_callback);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void DiscreteDynamicsWorld_setDebugDrawer(IntPtr a_ptr, IntPtr a_drawer);

        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ENativeDebugDrawer_new(DrawLineEvent a_drawLineEvent, ErrorEvent a_errorEvent);
        [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
        static extern void ENativeDebugDrawer_delete(IntPtr a_ptr);

        Vector3                             m_gravity;

        IntPtr                              m_objectPtr;
        IntPtr                              m_debugDrawerPtr;

        // Variables to stop GC collecting objects
        DrawLineEvent                       m_drawLineEvent;
        ErrorEvent                          m_errorEvent;
        CollisionCallback                   m_collisionEvent;

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

                DiscreteDynamicsWorld_setGravity(m_objectPtr, m_gravity.X, m_gravity.Y, m_gravity.Z);
            }
        }

        public bool DebugDraw
        {
            get
            {
                return m_debugDrawerPtr != IntPtr.Zero;
            }
        }

        public DiscreteDynamicsWorld (CollisionDispatcher a_dispatcher, BroadphaseInterface a_broadphase, ConstraintSolver a_solver, CollisionConfiguration a_configuration)
        {
            m_objectPtr = DiscreteDynamicsWorld_new(a_dispatcher.Ptr, a_broadphase.Ptr, a_solver.Ptr, a_configuration.Ptr);
            m_debugDrawerPtr = IntPtr.Zero;

            m_drawLineEvent = DrawLine;
            m_errorEvent = Error;
            m_collisionEvent = OnCollision;

            m_objectLookup = new Dictionary<IntPtr, CollisionObject>();
        }

        void DrawLine(float a_xA, float a_yA, float a_zA, float a_xB, float a_yB, float a_zB, float a_r, float a_g, float a_b, float a_a)
        {
            Gizmos.DrawLine(new Vector3(a_xA, a_yA, a_zA), new Vector3(a_xB, a_yB, a_zB), 0.1f, new Vector4(a_r, a_g, a_b, a_a));
        }
        void Error(IntPtr a_errorStr)
        {
            string str = Marshal.PtrToStringAnsi(a_errorStr);

            if (!string.IsNullOrEmpty(str))
            {
                InternalConsole.Error(str);
            }
        }

        void OnCollision(IntPtr a_objA, IntPtr a_objB, float a_xA, float a_yA, float a_zA, float a_xB, float a_yB, float a_zB, float a_nX, float a_nY, float a_nZ)
        {
            CollisionObject objA = m_objectLookup[a_objA];
            CollisionObject objB = m_objectLookup[a_objB];

            GameObject gameObjectA = objA.GameObject;
            GameObject gameObjectB = objB.GameObject;

            Vector3 posA = new Vector3(a_xA, a_yA, a_zA);
            Vector3 posB = new Vector3(a_xB, a_yB, a_zB);

            Vector3 normal = new Vector3(a_nX, a_nY, a_nZ);

            gameObjectA.OnCollision(objB, normal, posA);
            gameObjectB.OnCollision(objA, normal, posB);
        }

        internal void AddRigidbody (Rigidbody a_rigidbody)
        {
            IntPtr ptr = a_rigidbody.Ptr;

            DiscreteDynamicsWorld_addRigidBody(m_objectPtr, ptr);

            m_objectLookup.Add(ptr, a_rigidbody);
        }
        internal void RemoveRigidbody (Rigidbody a_rigidbody)
        {
            IntPtr ptr = a_rigidbody.Ptr;

            DiscreteDynamicsWorld_removeRigidBody(m_objectPtr, ptr);

            m_objectLookup.Remove(ptr);
        }

        internal void AddCollisionObject (CollisionObject a_collisionObject)
        {
            IntPtr ptr = a_collisionObject.Ptr;

            DiscreteDynamicsWorld_addCollisionObject(m_objectPtr, ptr);

            m_objectLookup.Add(ptr, a_collisionObject);
        }
        internal void RemoveCollisionObject (CollisionObject a_collisionObject)
        {
            IntPtr ptr = a_collisionObject.Ptr;

            DiscreteDynamicsWorld_removeCollisionObject(m_objectPtr, ptr);

            m_objectLookup.Remove(ptr);
        }

        internal void SetDebugDrawState(bool a_state)
        {
            if (a_state && m_debugDrawerPtr == IntPtr.Zero)
            {
                m_debugDrawerPtr = ENativeDebugDrawer_new(m_drawLineEvent, m_errorEvent);

                DiscreteDynamicsWorld_setDebugDrawer(m_objectPtr, m_debugDrawerPtr);
            }
            else if (!a_state && m_debugDrawerPtr != IntPtr.Zero)
            {
                DiscreteDynamicsWorld_setDebugDrawer(m_objectPtr, IntPtr.Zero);
                
                ENativeDebugDrawer_delete(m_debugDrawerPtr);
                m_debugDrawerPtr = IntPtr.Zero;
            }
        }

        internal void Update ()
        {
            DiscreteDynamicsWorld_stepSimulation(m_objectPtr, (float)PhysicsTime.DeltaTime, 10, 1.0f / 60.0f);

            if (m_debugDrawerPtr != IntPtr.Zero)
            {
                DiscreteDynamicsWorld_debugDrawWorld(m_objectPtr);
            }

            DiscreteDynamicsWorld_checkCollidingObjects(m_objectPtr, m_collisionEvent);
        }

        internal bool RaycastClosest (Vector3 a_from, Vector3 a_to, out RaycastResultClosest a_result)
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

            DiscreteDynamicsWorld_raycastClosest(m_objectPtr, a_from.X, a_from.Y, a_from.Z, a_to.X, a_to.Y, a_to.Z, out ptr, out nX, out nY, out nZ, out pX, out pY, out pZ);

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

            DiscreteDynamicsWorld_delete(m_objectPtr);

            if (m_debugDrawerPtr != IntPtr.Zero)
            {
                ENativeDebugDrawer_delete(m_debugDrawerPtr);
                m_debugDrawerPtr = IntPtr.Zero;
            }
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
