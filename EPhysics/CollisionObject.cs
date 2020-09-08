using OpenTK;
using System;
using System.Runtime.InteropServices;

namespace Erde.Physics
{
    public class CollisionObject : Component, IDisposable, IPObject
    {
        class BtCollisionObject
        {
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CollisionObject_new ();
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CollisionObject_delete (IntPtr a_ptr);


            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool CollisionObject_isStaticObject (IntPtr a_ptr);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool CollisionObject_isKinematicObject (IntPtr a_ptr);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool CollisionObject_isStaticOrKinematicObject (IntPtr a_ptr);

            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CollisionObject_setWorldTranslation (IntPtr a_ptr, float a_x, float a_y, float a_z);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CollisionObject_setWorldRotation (IntPtr a_ptr, float a_x, float a_y, float a_z, float a_w);

            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CollisionObject_getTransformMatrix (IntPtr a_ptr);
            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CollisionObject_freeTransformMatrix (IntPtr a_matrix);

            [DllImport("BtWrapNative", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CollisionObject_setCollisionShape (IntPtr a_ptr, IntPtr a_collisionShape);
        }

        protected PhysicsEngine     m_engine;

        protected IntPtr            m_objectPtr;

        protected Collider.Collider m_collisionShape;

        bool                        m_static;
        bool                        m_kinematic;

        bool                        m_resetObject;

        public Collider.Collider Collider
        {
            get
            {
                return m_collisionShape;
            }
        }

        internal IntPtr Ptr
        {
            get
            {
                return m_objectPtr;
            }
        }

        public bool IsStatic
        {
            get
            {
                return m_static;
            }
        }
        public bool IsKinematic
        {
            get
            {
                return m_kinematic;
            }
        }
        public bool IsStaticOrKinematicObject
        {
            get
            {
                return m_static || m_kinematic;
            }
        }

        protected void GetState ()
        {
            m_static = BtCollisionObject.CollisionObject_isStaticObject(m_objectPtr);
            m_kinematic = BtCollisionObject.CollisionObject_isKinematicObject(m_objectPtr);
        }

        public CollisionObject ()
        {
            m_objectPtr = IntPtr.Zero;

            m_collisionShape = null;
        }

        internal void Update ()
        {
            if (m_objectPtr != IntPtr.Zero)
            {
                Transform transform = Transform;

                if (transform != null)
                {
                    lock (transform)
                    {
                        byte staticState = transform.StaticState;
                        if ((staticState & 0b1 << 2) == 0)
                        {
                            IntPtr ptr = BtCollisionObject.CollisionObject_getTransformMatrix(m_objectPtr);
                            Matrix4 matrix = Marshal.PtrToStructure<Matrix4>(ptr);

                            transform.Translation = matrix.ExtractTranslation();
                            transform.Quaternion = matrix.ExtractRotation();

                            BtCollisionObject.CollisionObject_freeTransformMatrix(ptr);
                        }
                        else
                        {
                            Vector3 translation = transform.Translation;
                            Quaternion rotation = transform.Quaternion;

                            BtCollisionObject.CollisionObject_setWorldTranslation(m_objectPtr, translation.X, translation.Y, translation.Z);
                            BtCollisionObject.CollisionObject_setWorldRotation(m_objectPtr, rotation.X, rotation.Y, rotation.Z, rotation.W);
                        }

                        transform.StaticState = (byte)((staticState | (0b1 << 2)) ^ (0b1 << 2));
                    }
                }
            }
        }

        public virtual void SetCollider (Collider.Collider a_collider, PhysicsEngine a_engine)
        {
            m_engine = a_engine;

            m_resetObject = m_collisionShape != null;

            m_collisionShape = a_collider;

            m_engine.InputQueue.Enqueue(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_engine.DisposalQueue.Enqueue(this);
        }

        ~CollisionObject ()
        {
            Dispose(false);
        }   
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void ModifyObject ()
        {
            if (m_objectPtr == IntPtr.Zero)
            {
                m_objectPtr = BtCollisionObject.CollisionObject_new();

                GetState();
            }

            if (m_resetObject)
            {
                m_engine.RemoveCollisionObject(this);
            }

            if (m_collisionShape != null)
            {
                BtCollisionObject.CollisionObject_setCollisionShape(m_objectPtr, m_collisionShape.Ptr);

                m_engine.AddCollisionObject(this);
            }
            else
            {
                BtCollisionObject.CollisionObject_setCollisionShape(m_objectPtr, IntPtr.Zero);
            }
        }
        public virtual void DisposeObject ()
        {
            if (m_objectPtr != IntPtr.Zero)
            {
                if (m_collisionShape != null)
                {
                    m_engine.RemoveCollisionObject(this);
                }

                BtCollisionObject.CollisionObject_delete(m_objectPtr);
            }
        }
    }
}
