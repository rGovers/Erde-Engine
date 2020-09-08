using System;

namespace Erde.Physics.Solver
{
    internal abstract class ConstraintSolver : IDisposable
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
