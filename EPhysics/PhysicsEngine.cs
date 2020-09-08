using Erde.Physics.Broadphase;
using Erde.Physics.Configuration;
using Erde.Physics.Solver;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Erde.Physics
{
    public class PhysicsEngine : IDisposable
    {
        static PhysicsEngine Instance;

        CollisionConfiguration    m_collisionConfiguration;
                                  
        CollisionDispatcher       m_collisionDispatcher;
        BroadphaseInterface       m_broadphaseInterface;
        ConstraintSolver          m_constraintSolver;
                                  
        DiscreteDynamicsWorld     m_world;

        Thread                    m_thread;

        ConcurrentQueue<IPObject> m_inputQueue;
        ConcurrentQueue<IPObject> m_disposalQueue;

        bool                      m_threaded;

        bool                      m_shutDown;
        bool                      m_destroy;
        bool                      m_joinable;

        PhysicsTime               m_time;

        List<CollisionObject>     m_collisionObjects;

        public static PhysicsEngine Active
        {
            get
            {
                return Instance;
            }
        }

        public bool Threaded
        {
            get
            {
                return m_threaded;
            }
        }

        public ConcurrentQueue<IPObject> InputQueue
        {
            get
            {
                return m_inputQueue;
            }
        }
        public ConcurrentQueue<IPObject> DisposalQueue
        {
            get
            {
                return m_disposalQueue;
            }
        }

        public Vector3 Gravity
        {
            get
            {
                return m_world.Gravity;
            }
            set
            {
                m_world.Gravity = value;
            }
        }

        public PhysicsEngine (bool a_threaded)
        {
            m_threaded = a_threaded;

            m_shutDown = false;
            m_joinable = false;
            m_destroy = false;

            m_inputQueue = new ConcurrentQueue<IPObject>();
            m_disposalQueue = new ConcurrentQueue<IPObject>();

            m_collisionObjects = new List<CollisionObject>();

            m_time = new PhysicsTime();

            if (m_threaded)
            {
                m_thread = new Thread(Run)
                {
                    Name = "Physics",
                    Priority = ThreadPriority.AboveNormal
                };
                m_thread.Start();
            }
            else
            {
                m_thread = null;

                StartUp();
            }

            if (Instance != null)
            {
                InternalConsole.Warning("Multiple physics engines created");
            }
            Instance = this;
        }

        void Run ()
        {
            StartUp();

            while (!m_shutDown)
            {
                Update();
            }

            while (!m_destroy || !m_disposalQueue.IsEmpty)
            {
                Disposal();
            }

            Destroy();

            m_joinable = true;
        }

        void StartUp ()
        {
            m_collisionConfiguration = new ErdeCollisionConfiguration();

            m_collisionDispatcher = new CollisionDispatcher(m_collisionConfiguration);
            m_broadphaseInterface = new DbvtBroadphase();
            m_constraintSolver = new SequentialmpulseConstraintSolver();

            m_world = new DiscreteDynamicsWorld(m_collisionDispatcher, m_broadphaseInterface, m_constraintSolver, m_collisionConfiguration);
            m_world.Gravity = new Vector3(0.0f, -9.87f, 0.0f);
        }
        void Destroy ()
        {
            m_world.Dispose();

            m_constraintSolver.Dispose();
            m_broadphaseInterface.Dispose();
            m_collisionDispatcher.Dispose();

            m_collisionConfiguration.Dispose();
        }

        public void Shutdown ()
        {
            m_destroy = true;
            m_shutDown = true;
        }

        void Input ()
        {
            while (!m_inputQueue.IsEmpty)
            {
                IPObject pObject;

                if (!m_inputQueue.TryDequeue(out pObject))
                {
                    InternalConsole.Warning("Physics Engine: Failed to dequeue for writing");

                    return;
                }

                pObject.ModifyObject();

                CollisionObject collisionObject = pObject as CollisionObject;
                if (collisionObject != null && !m_collisionObjects.Contains(collisionObject))
                {
                    m_collisionObjects.Add(collisionObject);   
                }
            }
        }
        void Disposal ()
        {
            while (!m_disposalQueue.IsEmpty)
            {
                IPObject pObject;

                if (!m_disposalQueue.TryDequeue(out pObject))
                {
                    InternalConsole.Warning("Physics Engine: Failed to dequeue for disposal");

                    return;
                }

                pObject.DisposeObject();

                CollisionObject collisionObject = pObject as CollisionObject;
                if (collisionObject != null && m_collisionObjects.Contains(collisionObject))
                {
                    m_collisionObjects.Remove(collisionObject);
                }
            }
        }

        public void Update ()
        {
            m_time.Update();

            Input();
            Disposal();

            m_world.Update();

            foreach (CollisionObject collisionObject in m_collisionObjects)
            {
                collisionObject.Update();
            }

            GameObject.PhysicsUpdateBehaviours();
        }

        public bool RaycastClosest (Vector3 a_from, Vector3 a_to, out RaycastResultClosest a_result)
        {
            return m_world.RaycastClosest(a_from, a_to, out a_result);
        }

        internal void AddRigidbody (Rigidbody a_rigidbody)
        {
            m_world.AddRigidbody(a_rigidbody);
        }
        internal void RemoveRigidbody (Rigidbody a_rigidbody)
        {
            m_world.RemoveRigidbody(a_rigidbody);
        }

        internal void AddCollisionObject (CollisionObject a_collisionObject)
        {
            m_world.AddCollisionObject(a_collisionObject);
        }
        internal void RemoveCollisionObject (CollisionObject a_collisionObject)
        {
            m_world.RemoveCollisionObject(a_collisionObject);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif
            Instance = null;

            if (m_threaded)
            {
                while (!m_joinable)
                {
                    Thread.Yield();
                }

                m_thread.Join();
            }
            else
            {
                Disposal();

                Destroy();
            }
        }

        ~PhysicsEngine ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
