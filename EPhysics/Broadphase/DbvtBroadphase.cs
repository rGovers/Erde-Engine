using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Broadphase
{
    internal class DbvtBroadphase : BroadphaseInterface
    {
        class BtDvbtBroadphase
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DbvtBroadphase_new ();
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DbvtBroadphaser_delete (IntPtr a_ptr);
        }

        public DbvtBroadphase ()
        {
            m_objectPtr = BtDvbtBroadphase.DbvtBroadphase_new();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            BtDvbtBroadphase.DbvtBroadphaser_delete(m_objectPtr);
        }

        ~DbvtBroadphase ()
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
