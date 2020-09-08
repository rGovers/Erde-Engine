using System;

namespace Erde.Physics.Configuration
{
    internal abstract class CollisionConfiguration : IDisposable
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
