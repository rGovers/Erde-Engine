using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Collider
{
    public class DistanceFieldCollider : Collider, IPObject
    {
        class EDistanceFieldCollider
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DistanceFieldCollider_new (IntPtr a_distanceField, int a_stride, int a_width, int a_height, int a_depth, float a_spacing);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DistanceFieldCollider_delete (IntPtr a_ptr);
        }

        PhysicsEngine  m_engine;
        IDistanceField m_distanceField;

        IntPtr         m_distPtr;

        public DistanceFieldCollider (IDistanceField a_distanceField, PhysicsEngine a_engine)
        {
            m_distanceField = a_distanceField;

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

        ~DistanceFieldCollider ()
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
            m_distPtr = m_distanceField.CellPtr;
            
            m_objectPtr = EDistanceFieldCollider.DistanceFieldCollider_new(m_distPtr, m_distanceField.Stride, m_distanceField.Width, m_distanceField.Height, m_distanceField.Depth, m_distanceField.Spacing);
        }

        public void DisposeObject ()
        {
            EDistanceFieldCollider.DistanceFieldCollider_delete(m_objectPtr);
        }
    }
}
