using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Collider
{
    public class SphereCollider : Collider, IPObject
    {
        class BtSphereCollisionShape
        {
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr SphereCollisionShape_new (float a_radius);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SphereCollisionShape_delete (IntPtr a_ptr);
        }

        PhysicsEngine m_engine;

        float         m_radius;

        public float Radius
        {
            get
            {
                return m_radius;
            }
        }

        public SphereCollider (float a_radius, PhysicsEngine a_engine)
        {
            m_radius = a_radius;

            m_engine = a_engine;

            m_engine.InputQueue.Enqueue(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_engine.DisposalQueue.Enqueue(this);
        }

        ~SphereCollider ()
        {
            Dispose(false);
        }
        public override void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            m_objectPtr = BtSphereCollisionShape.SphereCollisionShape_new(m_radius);
        }
        public void DisposeObject ()
        {
            BtSphereCollisionShape.SphereCollisionShape_delete(m_objectPtr);
        }
    }
}
