using OpenTK;
using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Collider
{
    public class BoxCollider : Collider, IPObject
    {
        class BtBoxCollisionShape
        {
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr BoxCollisionShape_new (float a_halfX, float a_halfY, float a_halfZ);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void BoxCollisionShape_delete (IntPtr a_ptr);
        }

        Vector3       m_halfExtents;

        PhysicsEngine m_engine;

        public Vector3 HalfExtents
        {
            get
            {
                return m_halfExtents;
            }
        }

        public BoxCollider (Vector3 a_halfExtents, PhysicsEngine a_engine)
        {
            m_halfExtents = a_halfExtents;

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

        ~BoxCollider ()
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
            m_objectPtr = BtBoxCollisionShape.BoxCollisionShape_new(m_halfExtents.X, m_halfExtents.Y, m_halfExtents.Z);
        }
        public void DisposeObject ()
        {
            BtBoxCollisionShape.BoxCollisionShape_delete(m_objectPtr);
        }
    }
}
