namespace Erde.Physics
{
    public class PhysicsObject : Component
    {
        Transform m_transform;

        Collider  m_collider;

        float     m_mass;

        Engine    m_engine;

        public Engine Engine
        {
            get
            {
                return m_engine;
            }
            set
            {
                m_transform = GameObject.GetComponent<Transform>();

                m_engine = value;
                m_engine.AddObject(this);
            }
        }

        public float Mass
        {
            get
            {
                return m_mass;
            }
            set
            {
                m_mass = value;
            }
        }

        public Collider Collider
        {
            get
            {
                return m_collider;
            }
            set
            {
                m_collider = value;

                if (m_collider.PhysicsObject != this)
                {
                    m_collider.PhysicsObject = this;
                }
            }
        }

        public PhysicsObject ()
        {
            m_mass = float.PositiveInfinity;
        }
        ~PhysicsObject ()
        {
            if (m_engine != null)
            {
                m_engine.RemoveObject(this);
            }
        }
    }
}