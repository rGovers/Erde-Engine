namespace Erde
{
    public abstract class Component
    {
        GameObject m_gameObject;

        public GameObject GameObject
        {
            get
            {
                return m_gameObject;
            }
        }

        public void SetGameObject (GameObject a_gameObject)
        {
            if (m_gameObject != null)
            {
                m_gameObject.RemoveComponent(this);
            }

            m_gameObject = a_gameObject;
        }

        public Transform Transform
        {
            get
            {
                return GameObject.Transform;
            }
        }

        public Component ()
        {
            m_gameObject = null;
        }
    }
}