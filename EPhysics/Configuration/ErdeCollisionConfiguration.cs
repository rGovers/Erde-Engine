using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Configuration
{
    internal class ErdeCollisionConfiguration : CollisionConfiguration
    {
        class ECollisionConfiguration
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr ECollisionConfiguration_new ();
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ECollisionConfiguration_delete (IntPtr a_ptr);
        }

        public ErdeCollisionConfiguration ()
        {
            m_objectPtr = ECollisionConfiguration.ECollisionConfiguration_new();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            ECollisionConfiguration.ECollisionConfiguration_delete(m_objectPtr);
        }

        ~ErdeCollisionConfiguration ()
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
