using OpenTK;

namespace Erde
{
    public class Transform : Component
    {
        Transform  m_parent;

        Vector3    m_translation;
        Vector3    m_scale;
        Quaternion m_rotation;

        byte       m_static;

        public bool Static
        {
            get
            {
                return (m_static & 0x1) != 0;
            }
        }

        public byte StaticState
        {
            get
            {
                return m_static;
            }
            set
            {
                m_static = value;
            }
        }

        public Transform ()
        {
            m_parent = null;
            m_translation = Vector3.Zero;
            m_scale = Vector3.One;
            m_rotation = Quaternion.Identity;
        }

        public Transform Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        public Vector3 Translation
        {
            get
            {
                return m_translation;
            }
            set
            {
                m_translation = value;

                m_static |= 0b1 << 2;
            }
        }

        public Quaternion Quaternion
        {
            get
            {
                return m_rotation;
            }
            set
            {
                m_rotation = value;

                m_static |= 0b1 << 2;
            }
        }
        public Vector4 AxisAngle
        {
            get
            {
                return Quaternion.ToAxisAngle();
            }
            set
            {
                value.W %= MathHelper.TwoPi;

                Quaternion = Quaternion.FromAxisAngle(value.Xyz, value.W);
            }
        }
        public Vector3 EulerAngle
        {
            set
            {
                Quaternion = Quaternion.FromEulerAngles(value);
            }
        }

        public Vector3 Scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                m_scale = value;
            }
        }

        public Matrix3 RotationMatrix
        {
            get
            {
                return Matrix3.CreateFromQuaternion(Quaternion);
            }
            set
            {
                Quaternion = value.ExtractRotation();
            }
        }

        public Vector3 Forward
        {
            get
            {
                return RotationMatrix.Row2;
            }
        }
        public Vector3 Up
        {
            get
            {
                return RotationMatrix.Row1;
            }
        }
        public Vector3 Right
        {
            get
            {
                return RotationMatrix.Row0;
            }
        }

        public Matrix4 ToMatrix ()
        {
            if (Parent != null)
            {
                return m_parent.ToMatrix() * (Matrix4.CreateScale(m_scale) * Matrix4.CreateFromQuaternion(m_rotation) * Matrix4.CreateTranslation(m_translation));
            }

            return Matrix4.CreateScale(m_scale) * Matrix4.CreateFromQuaternion(m_rotation) * Matrix4.CreateTranslation(m_translation);
        }

        public void SetStatic ()
        {
            m_static |= 0x1;
        }
    }
}