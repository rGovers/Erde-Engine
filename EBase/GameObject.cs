using System;
using System.Collections.Generic;

namespace Erde
{
    public class GameObject : IDisposable
    {
        static List<GameObject> m_gameObjects = new List<GameObject>();

        List<Component>         m_components;

        Transform               m_transform;

        Behaviour.Event         m_update;

        public Transform Transform
        {
            get
            {
                return m_transform;
            }
        }

        public GameObject ()
        {
            m_components = new List<Component>();

            m_transform = AddComponent<Transform>();

            m_gameObjects.Add(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_gameObjects.Remove(this);

            foreach (Component comp in m_components)
            {
                IDisposable disp = comp as IDisposable;

                if (disp != null && !(comp is Transform))
                {
                    disp.Dispose();
                }
            }

            m_components.Clear();

            m_transform = null;
        }

        ~GameObject ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Component AddComponent (Type a_type)
        {
            Component comp = Activator.CreateInstance(a_type) as Component;
            AddComponent(comp);

            return comp;
        }

        public T AddComponent<T> () where T : Component
        {
            T comp = Activator.CreateInstance<T>();

            AddComponent(comp);

            return comp;
        }

        internal void AddComponent (Component a_component)
        {
            if (a_component.GameObject != this)
            {
                a_component.SetGameObject(this);
            }

            m_components.Add(a_component);

            if (a_component is Transform)
            {
                m_transform = a_component as Transform;
            }
            else if (a_component.GetType().BaseType == typeof(Behaviour))
            {
                Behaviour behaviour = a_component as Behaviour;
                if (behaviour.Start != null)
                {
                    behaviour.Start.Invoke();
                }

                m_update += behaviour.Update;
            }
        }

        public Component GetComponent (Type a_type)
        {
            foreach (Component comp in m_components)
            {
                if (comp.GetType() == a_type)
                {
                    return comp;
                }
            }

            return null;
        }

        public T GetComponent<T> () where T : Component
        {
            foreach (Component comp in m_components)
            {
                if (comp.GetType() == typeof(T))
                {
                    return (T)comp;
                }
            }

            return null;
        }

        public void RemoveComponent (Component a_component)
        {
            m_components.Remove(a_component);

            if (a_component is Behaviour)
            {
                Behaviour behaviour = a_component as Behaviour;
                m_update -= behaviour.Update;
            }
        }
        public void RemoveComponent<T> (T a_component) where T : Component
        {
            m_components.Remove(a_component);
        }
        public void RemoveComponent<T> () where T : Component
        {
            for (int i = 0; i < m_components.Count; ++i)
            {
                if (m_components[i].GetType() == typeof(T))
                {
                    m_components[i].SetGameObject(null);

                    m_components.Remove(m_components[i]);

                    return;
                }
            }
        }

        public static void UpdateBehaviours ()
        {
            for (int i = 0; i < m_gameObjects.Count; ++i)
            {
                GameObject obj = m_gameObjects[i];

                if (obj == null)
                {
                    continue;
                }

                if (obj.m_update != null)
                {
                    lock (obj)
                    {
                        if (obj.m_update != null)
                        {
                            obj.m_update.Invoke();
                        }        
                    }
                }
            }
        }
        public static void ClearAllGameObjects ()
        {
            for (int i = 0; i < m_gameObjects.Count; ++i)
            {
                GameObject obj = m_gameObjects[i];

                obj.Dispose();
            }

            m_gameObjects.Clear();
        }
    }
}