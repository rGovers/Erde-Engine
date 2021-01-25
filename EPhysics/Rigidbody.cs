using OpenTK;
using System;
using System.Runtime.InteropServices;

namespace Erde.Physics
{
    public class Rigidbody : CollisionObject
    {
        class BtRigidbody
        {
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Rigidbody_new (float a_mass, IntPtr a_motionState, IntPtr a_collisionShape);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_delete (IntPtr a_ptr);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_setMass (IntPtr a_ptr, float a_mass);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_getForce (IntPtr a_ptr, out float a_x, out float a_y, out float a_z);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_getTorque (IntPtr a_ptr, out float a_x, out float a_y, out float a_z);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_applyForce (IntPtr a_ptr, float a_vX, float a_vY, float a_vZ, float a_pX, float a_pY, float a_pZ);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_applyForceCentral (IntPtr a_ptr, float a_x, float a_y, float a_z);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_applyTorque (IntPtr a_ptr, float a_x, float a_y, float a_z);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_applyTorqueImpulse (IntPtr a_ptr, float a_x, float a_y, float a_z);

            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_applyImpulse (IntPtr a_ptr, float a_iX, float a_iY, float a_iZ, float a_pX, float a_pY, float a_pZ);
            [DllImport("ENativePhysics", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Rigidbody_applyImpulseCentral (IntPtr a_ptr, float a_x, float a_y, float a_z);
        }

        float m_mass;

        public float Mass
        {
            get
            {
                return m_mass;
            }
            set
            {
                m_mass = value;

                if (m_objectPtr != IntPtr.Zero)
                {
                    BtRigidbody.Rigidbody_setMass(m_objectPtr, m_mass);
                }
            }
        }

        public Vector3 Force
        {
            get
            {
                float x;
                float y;
                float z;

                BtRigidbody.Rigidbody_getForce(m_objectPtr, out x, out y, out z);

                return new Vector3(x, y, z);
            }
        }

        public Vector3 Torque
        {
            get
            {
                float x;
                float y;
                float z;

                BtRigidbody.Rigidbody_getTorque(m_objectPtr, out x, out y, out z);

                return new Vector3(x, y, z);
            }
        }

        public Rigidbody ()
        {
            m_mass = 1.0f;

            m_objectPtr = IntPtr.Zero;
        }

        public override void ModifyObject ()
        {
            if (m_objectPtr != IntPtr.Zero)
            {
                m_engine.RemoveRigidbody(this);

                BtRigidbody.Rigidbody_delete(m_objectPtr);
                m_objectPtr = IntPtr.Zero;
            }

            if (m_collisionShape != null)
            {
                m_objectPtr = BtRigidbody.Rigidbody_new(m_mass, IntPtr.Zero, m_collisionShape.Ptr);

                m_engine.AddRigidbody(this);
            }
        }
        public override void DisposeObject ()
        {
            if (m_objectPtr != IntPtr.Zero)
            {
                m_engine.RemoveRigidbody(this);

                BtRigidbody.Rigidbody_delete(m_objectPtr);
            }
        }

        public void ApplyForce (Vector3 a_force)
        {
            BtRigidbody.Rigidbody_applyForceCentral(m_objectPtr, a_force.X, a_force.Y, a_force.Z);
        }
        public void ApplyForce (Vector3 a_force, Vector3 a_pos)
        {
            BtRigidbody.Rigidbody_applyForce(m_objectPtr, a_force.X, a_force.Y, a_force.Z, a_pos.X, a_pos.Y, a_pos.Z);
        }

        public void ApplyTorque (Vector3 a_torque)
        {
            BtRigidbody.Rigidbody_applyTorque(m_objectPtr, a_torque.X, a_torque.Y, a_torque.Z);
        }
        public void ApplyTorqueImpulse (Vector3 a_impulse)
        {
            BtRigidbody.Rigidbody_applyTorqueImpulse(m_objectPtr, a_impulse.X, a_impulse.Y, a_impulse.Z);
        }

        public void ApplyImpulse (Vector3 a_impulse)
        {
            BtRigidbody.Rigidbody_applyImpulseCentral(m_objectPtr, a_impulse.X, a_impulse.Y, a_impulse.Z);
        }
        public void ApplyImpulse (Vector3 a_impulse, Vector3 a_pos)
        {
            BtRigidbody.Rigidbody_applyImpulse(m_objectPtr, a_impulse.X, a_impulse.Y, a_impulse.Z, a_pos.X, a_pos.Y, a_pos.Z);
        }
    }
}
