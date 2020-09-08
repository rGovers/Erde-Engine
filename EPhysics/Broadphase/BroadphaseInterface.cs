using System;

namespace Erde.Physics.Broadphase
{
    internal abstract class BroadphaseInterface : IDisposable
    {
        protected IntPtr m_objectPtr;

        internal IntPtr Ptr
        {
            get
            {
                return m_objectPtr;
            }
        }

        public virtual void Dispose ()
        {
        }
    }
}
