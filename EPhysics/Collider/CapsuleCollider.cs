using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Collider
{
    public class CapsuleCollider : Collider, IPObject
    {
        class BtCapsuleCollisionShape
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CapsuleCollisionShape_new (float a_radius, float a_height);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CapsuleCollisionShape_delete (IntPtr a_ptr);
        }

        PhysicsEngine m_engine;

        float         m_height;
        float         m_radius;

        public float Height
        {
            get
            {
                return m_height;
            }
        }

        public float Radius
        {
            get
            {
                return m_radius;
            }
        }

        public CapsuleCollider (float a_height, float a_radius, PhysicsEngine a_engine)
        {
            m_engine = a_engine;

            m_height = a_height;
            m_radius = a_radius;

            m_engine.InputQueue.Enqueue(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_engine.DisposalQueue.Enqueue(this);
        }

        ~CapsuleCollider ()
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
            m_objectPtr = BtCapsuleCollisionShape.CapsuleCollisionShape_new(m_radius, m_height);
        }

        public void DisposeObject ()
        {
            BtCapsuleCollisionShape.CapsuleCollisionShape_delete(m_objectPtr);
        }
    }
}
