using System;
using System.Runtime.InteropServices;

namespace Erde.Physics.Solver
{
    internal class SequentialmpulseConstraintSolver : ConstraintSolver
    {
        class BtSequentialImpulseConstraintSolver
        {
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr SequentialImpulseConstraintSolver_new ();
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SequentialImpulseConstraintSolver_delete (IntPtr a_ptr);
        }

        public SequentialmpulseConstraintSolver ()
        {
            m_objectPtr = BtSequentialImpulseConstraintSolver.SequentialImpulseConstraintSolver_new();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            BtSequentialImpulseConstraintSolver.SequentialImpulseConstraintSolver_delete(m_objectPtr);
        }

        ~SequentialmpulseConstraintSolver ()
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
