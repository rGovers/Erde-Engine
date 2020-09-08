using System;

namespace Erde.Physics.Collider
{
    public abstract class Collider : IDisposable
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
