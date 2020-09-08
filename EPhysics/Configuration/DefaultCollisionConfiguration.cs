using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Configuration
{
    internal class DefaultCollisionConfiguration : CollisionConfiguration
    {
        class BtDefaultCollisionConfiguration
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DefaultCollisionConfiguration_new ();
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DefaultCollisionConfiguration_delete (IntPtr a_ptr);
        }

        public DefaultCollisionConfiguration ()
        {
            m_objectPtr = BtDefaultCollisionConfiguration.DefaultCollisionConfiguration_new();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            BtDefaultCollisionConfiguration.DefaultCollisionConfiguration_delete(m_objectPtr);
        }

        ~DefaultCollisionConfiguration ()
        {
            Dispose(false);
        }
        public override void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
