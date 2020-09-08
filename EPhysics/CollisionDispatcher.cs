using Erde.Physics.Configuration;
using System;
using System.Runtime.InteropServices;

namespace Erde.Physics
{
    internal class CollisionDispatcher : IDisposable
    {
        class BtCollisionDispatcher
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CollisionDispatcher_new (IntPtr a_collisionConfiguration);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CollisionDispatcher_delete (IntPtr a_ptr);
        }

        IntPtr m_objectPtr;

        internal IntPtr Ptr
        {
            get
            {
                return m_objectPtr;
            }
        }

        public CollisionDispatcher (CollisionConfiguration a_collisionConfiguration)
        {
            m_objectPtr = BtCollisionDispatcher.CollisionDispatcher_new(a_collisionConfiguration.Ptr);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            BtCollisionDispatcher.CollisionDispatcher_delete(m_objectPtr);
        }

        ~CollisionDispatcher ()
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
