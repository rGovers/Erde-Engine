using OpenTK;

namespace Erde.Physics
{
    public enum e_ForceType
    {
        Force,
        Acceleration,
        Impulse
    }

    public class Rigidbody : PhysicsObject
    {
        byte      m_state;

        Vector3   m_force;
        Vector3   m_acceleration;
        Vector3   m_velocity;

        public bool IsActive
        {
            get
            {
                return (m_state & 1 << 0) != 0;
            }
            internal set
            {
                if (value)
                {
                    m_state |= 1 << 0;
                }
                else
                {
                    m_state ^= (byte)(m_state & (1 << 0));
                }
            }
        }
        public bool Kinematic
        {
            get
            {
                return (m_state & 1 << 1) != 0;
            }
            set
            {
                if (value)
                {
                    m_state |= 1 << 1;
                }
                else
                {
                    m_state ^= (byte)(m_state & (1 << 1));
                }
            }
        }
        public bool Gravity
        {
            get
            {
                return (m_state & 1 << 2) != 0;
            }
            set
            {
                if (value)
                {
                    m_state |= 1 << 2;
                }
                else
                {
                    m_state ^= (byte)(m_state & (1 << 2));
                }
            }
        }

        public Vector3 Force
        {
            get
            {
                return m_force;
            }
        }

        public Vector3 Acceleration
        {
            get
            {
                return m_acceleration;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return m_velocity;
            }
        }

        public Rigidbody () : base()
        {
            IsActive = true;
            Gravity = true;

            m_force = Vector3.Zero;
            m_acceleration = Vector3.Zero;
            m_velocity = Vector3.Zero;

            Mass = 1.0f;
        }

        internal void Update ()
        {
            if (!Kinematic)
            {
                m_acceleration += m_force / Mass;

                float dTime = PhysicsTime.DeltaTime;

                m_velocity += m_acceleration * dTime;

                Transform.Translation += m_velocity * dTime;
            }

            m_force = Vector3.Zero;
            m_acceleration = Vector3.Zero;
        }

        public void StopObject ()
        {
            m_force = Vector3.Zero;
            m_acceleration = Vector3.Zero;
            m_velocity = Vector3.Zero;
        }

        public Vector3 AddForce (Vector3 a_force, e_ForceType a_forceType)
        {
            if (a_force != Vector3.Zero)
            {
                IsActive = true;
            }

            switch (a_forceType)
            {
            case e_ForceType.Force:
                {
                    return m_force += a_force;
                }
            case e_ForceType.Acceleration:
                {
                    return m_acceleration += a_force;
                }
            case e_ForceType.Impulse:
                {
                    return m_velocity += a_force;
                }
            }

            return Vector3.Zero;
        }
    }
}