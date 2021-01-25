using OpenTK;
using System;
using System.Runtime.InteropServices;

namespace Erde.Physics
{
    internal class MotionState
    {
        class EMotionState
        {
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MotionState_new ();
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void MotionState_delete (IntPtr a_ptr);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void MotionState_setTransform (IntPtr a_ptr, float a_x, float a_y, float a_z, float a_rX, float a_rY, float a_rZ, float a_rW);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MotionState_getTransformMatrix (IntPtr a_ptr);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void MotionState_freeTransformMatrix (IntPtr a_matrix);
        }

        IntPtr m_objectPtr;

        internal IntPtr Ptr
        {
            get
            {
                return m_objectPtr;
            }
        }

        public MotionState ()
        {
            m_objectPtr = EMotionState.MotionState_new();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            EMotionState.MotionState_delete(m_objectPtr);
        }

        ~MotionState ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void UpdateTransform (Transform a_transform)
        {
            byte staticState = a_transform.StaticState;
            if ((staticState & 0b1 << 2) == 0)
            {
                IntPtr ptr = EMotionState.MotionState_getTransformMatrix(m_objectPtr);
                Matrix4 matrix = Marshal.PtrToStructure<Matrix4>(ptr);

                a_transform.Translation = matrix.ExtractTranslation();
                a_transform.Quaternion = matrix.ExtractRotation();

                EMotionState.MotionState_freeTransformMatrix(ptr);
            }
            else
            {
                Vector3 translation = a_transform.Translation;
                Quaternion rotation = a_transform.Quaternion;

                EMotionState.MotionState_setTransform(m_objectPtr, translation.X, translation.Y, translation.Z, rotation.X, rotation.Y, rotation.Z, rotation.W);
            }

            a_transform.StaticState = (byte)((staticState | (0b1 << 2)) ^ (0b1 << 2));
        }
    }
}
