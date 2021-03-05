using System;
using System.Collections.Generic;

namespace Erde
{
    public class GameObject : IDisposable
    {
        static List<GameObject> GameObjects = new List<GameObject>();

        string                  m_name;

        List<Component>         m_components;

        Transform               m_transform;

        Behaviour.Event         m_update;
        Behaviour.Event         m_physicsUpdate;

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public Transform Transform
        {
            get
            {
                return m_transform;
            }
        }

        public GameObject Parent
        {
            get
            {
                lock (this)
                {
                    Transform parent = m_transform.Parent;

                    if (parent != null)
                    {
                        return parent.GameObject;
                    }
                }

                return null;
            }
        }

        public IEnumerable<GameObject> Children
        {
            get
            {
                lock (this)
                {
                    IEnumerable<Transform> children = m_transform.Children;

                    foreach (Transform child in children)
                    {
                        yield return child.GameObject;
                    }
                }
            }
        }

        public GameObject ()
        {
            m_name = string.Empty;

            m_components = new List<Component>();

            m_transform = AddComponent<Transform>();

            lock (GameObjects)
            {
                GameObjects.Add(this);
            }
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif
            lock (GameObjects)
            {
                GameObjects.Remove(this);
            }

            lock (this)
            {
                m_update = null;
                m_physicsUpdate = null;

                foreach (Component comp in m_components)
                {
                    lock (comp)
                    {
                        IDisposable disp = comp as IDisposable;

                        if (disp != null)
                        {
                            disp.Dispose();
                        }
                    }
                }

                m_components.Clear();

                m_transform = null;
            }
        }

        ~GameObject ()
        {
            Dispose(false);
        }

        public virtual void Dispose ()
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
            lock (this)
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
                else if (a_component is Behaviour)
                {
                    Behaviour behaviour = a_component as Behaviour;
                    if (behaviour.Start != null)
                    {
                        behaviour.Start.Invoke();
                    }

                    m_update += behaviour.Update;
                    m_physicsUpdate += behaviour.PhysicsUpdate;
                }
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
                if (comp is T)
                {
                    return (T)comp;
                }
            }

            return null;
        }

        public void RemoveComponent (Component a_component)
        {
            lock (this)
            {
                lock (a_component)
                {
                    m_components.Remove(a_component);

                    Behaviour behaviour = a_component as Behaviour;
                    if (behaviour != null)
                    {
                        m_update -= behaviour.Update;
                    }
                }
            }
        }
        public void RemoveComponent<T> (T a_component) where T : Component
        {
            lock (this)
            {
                m_components.Remove(a_component);
            }
        }
        public void RemoveComponent<T> () where T : Component
        {
            lock (this)
            {
                for (int i = 0; i < m_components.Count; ++i)
                {
                    Component comp = m_components[i] as T;

                    if (comp != null)
                    {
                        RemoveComponent(comp);

                        lock (comp)
                        {
                            IDisposable disposable = comp as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }

                        return;
                    }
                }
            }
        }

        GameObject GetChild(GameObject a_child, string a_name)
        {
            if (a_child.Name == a_name)
            {
                return a_child;
            }

            IEnumerable<Transform> children = a_child.Transform.Children;

            foreach (Transform child in children)
            {
                GameObject childObj = child.GameObject;

                GameObject obj = GetChild(childObj, a_name);

                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        public GameObject GetChildRecursize(string a_name)
        {
            IEnumerable<Transform> children = Transform.Children;

            foreach (Transform child in children)
            {
                GameObject childObj = child.GameObject;

                GameObject obj = GetChild(childObj, a_name);

                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        public GameObject GetChild(string a_name)
        {
            IEnumerable<Transform> children = Transform.Children;

            foreach (Transform child in children)
            {
                GameObject obj = child.GameObject;

                if (obj.Name == a_name)
                {
                    return obj;
                }
            }

            return null;
        }

        public static void UpdateBehaviours ()
        {
            GameObject[] gameObjects;
            lock (GameObjects)
            {
                gameObjects = GameObjects.ToArray();
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];

                if (obj == null)
                {
                    continue;
                }

                lock (obj)
                {
                    if (obj.m_update != null)
                    {
                        obj.m_update.Invoke();
                    }        
                }
            }
        }
        public static void PhysicsUpdateBehaviours ()
        {
            GameObject[] gameObjects;
            lock (GameObjects)
            {
                gameObjects = GameObjects.ToArray();
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];

                if (obj == null)
                {
                    continue;
                }

                lock (obj)
                {
                    if (obj.m_physicsUpdate != null)
                    {
                        obj.m_physicsUpdate.Invoke();
                    }
                }
            }
        }
        public static void ClearAllGameObjects ()
        {
            lock (GameObjects)
            {
                for (int i = 0; i < GameObjects.Count; ++i)
                {
                    GameObject obj = GameObjects[i];

                    obj.Dispose();
                }

                GameObjects.Clear();
            }
        }
    }
}