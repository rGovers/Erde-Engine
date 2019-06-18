using Erde.Physics.Colliders;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Erde.Physics
{
    public class Engine : IDisposable
    {
        static Engine                            m_active;

        ISolver                                  m_solver;

        Thread                                   m_thread;

        bool                                     m_shutdown;
        bool                                     m_join;

        ConcurrentDictionary<int, PhysicsObject> m_objects;
        ConcurrentDictionary<int, Rigidbody>     m_dynamicObjects;

        int                                      m_index = int.MinValue;

        Vector3                                  m_gravity;

        bool                                     m_runSim;

#if DEBUG_INFO
        int                                      m_collisionChecks;
        int                                      m_lastCollisionChecks;

        public int CollisionChecks
        {
            get
            {
                return m_lastCollisionChecks;
            }
        }
#endif

        public static Engine Active
        {
            get
            {
                return m_active;
            }
        }

        public Vector3 Gravity
        {
            get
            {
                return m_gravity;
            }
            set
            {
                m_gravity = value;
            }
        }

        public struct Resolution
        {
            Vector3 m_normal;
            float   m_intersectDistance;

            public Vector3 Normal
            {
                get
                {
                    return m_normal;
                }
                set
                {
                    m_normal = value;
                }
            }

            public float IntersectDistance
            {
                get
                {
                    return m_intersectDistance;
                }
                set
                {
                    m_intersectDistance = value;
                }
            }
        }

        public Engine () : this(new Solver()) { }
        public Engine (ISolver a_solver)
        {
            if (m_active == null)
            {
                m_active = this;
            }
            m_solver = a_solver;

            m_thread = new Thread(Run)
            {
                Name = "Physics",
                Priority = ThreadPriority.AboveNormal
            };

            m_runSim = true;

            m_objects = new ConcurrentDictionary<int, PhysicsObject>();
            m_dynamicObjects = new ConcurrentDictionary<int, Rigidbody>();

            m_thread.Start();
        }

        public bool RunSimulation
        {
            get
            {
                return m_runSim;
            }
            set
            {
                m_runSim = value;
            }
        }

        void Run ()
        {
            PhysicsTime time = new PhysicsTime();

            m_shutdown = false;
            m_join = false;

            Resolution res;

            while (!m_shutdown)
            {
#if DEBUG_INFO
                m_lastCollisionChecks = m_collisionChecks;
                m_collisionChecks = 0;
#endif

                time.Update();

                if (m_runSim)
                {
                    ICollection<Rigidbody> dObjects = m_dynamicObjects.Values;
                    ICollection<PhysicsObject> pObjects = m_objects.Values;

                    foreach (Rigidbody body in dObjects)
                    {
                        lock (body)
                        {
                            if (body.IsActive)
                            {
                                if (body.Gravity)
                                {
                                    body.AddForce(m_gravity, e_ForceType.Acceleration);
                                }

                                foreach (PhysicsObject obj in pObjects)
                                {
                                    lock (obj)
                                    {
                                        if (obj == body)
                                        {
                                            continue;
                                        }

#if DEBUG_INFO
                                        ++m_collisionChecks;
#endif

                                        if (m_solver.Collision(out res, body.Collider, obj.Collider))
                                        {
                                            // e = The physics material of the object

                                            // vAB = vA - vB
                                            // j = -(1 + e) * vAB {dot} n / n {dot} n(1/massA + 1/massB)

                                            // vA2 = vB1 + j / mA * normal

                                            float mA = body.Mass;
                                            float mB = obj.Mass;
                                            float mAB = mA + mB;

                                            float e = 0.2f;

                                            Vector3 n = res.Normal;

                                            Rigidbody bodyB = obj as Rigidbody;
                                            if (bodyB != null)
                                            {

                                            }
                                            else
                                            {
                                                Vector3 vAB = body.Velocity;

                                                if (vAB.LengthSquared != 0)
                                                {
                                                    float j = Vector3.Dot(-(1 + e) * vAB, n) / (1 / mA + 1 / mB);

                                                    Vector3 vA1 = body.Velocity;
                                                    Vector3 vA2 = body.AddForce(j / mA * n, e_ForceType.Impulse);

                                                    body.Transform.Translation -= res.Normal * (res.IntersectDistance);
                                                }
                                            }
                                        }
                                    }
                                }

                                body.Update();

                                if (body.Velocity.LengthSquared <= 0.2f * PhysicsTime.DeltaTime)
                                {
                                    body.IsActive = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            m_join = true;
        }

        internal void AddObject (PhysicsObject a_object)
        {
            int ind = m_index++;

            while (!m_objects.TryAdd(ind, a_object));

            if (a_object is Rigidbody)
            {
                while (!m_dynamicObjects.TryAdd(ind, a_object as Rigidbody));
            }
        }
        internal void RemoveObject (PhysicsObject a_object)
        {
            ICollection<int> keys = m_objects.Keys;

            foreach (int index in keys)
            {
                if (m_objects[index] == a_object)
                {
                    if (a_object is Rigidbody)
                    {
                        Rigidbody tmp;
                        m_dynamicObjects.TryRemove(index, out tmp);
                    }

                    m_objects.TryRemove(index, out a_object);

                    return;
                }
            }
        }

        void Dispose (bool a_value)
        {
            m_shutdown = true;

            while (!m_join)
            {
                // Fixes issues with infinite loops for some reason
                Console.Write(string.Empty);
            }

            m_thread.Join();
        }

        public Collider RayCast (Vector3 a_position, Vector3 a_direction, bool a_rbOnly = false)
        {
            if (!a_rbOnly) 
            {
                ICollection<PhysicsObject> pObjects = m_objects.Values;

                foreach (PhysicsObject obj in pObjects)
                {
                    Vector3 t = obj.Transform.Translation;

                    switch (obj.Collider.ColliderType)
                    {
                    case "Sphere":
                        {
                            Sphere sphere = obj.Collider as Sphere;

                            float r = sphere.Radius;

                            Vector3 diff = a_position - t;

                            float b = Vector3.Dot(diff, a_direction);
                            float c = diff.LengthSquared - r * r;

                            if (c > 0.0f && b > 0.0f)
                            {
                                continue;
                            }

                            float discr = b * b - c;

                            if (discr < 0.0f)
                            {
                                continue;
                            }

                            return sphere;
                        }
                    }
                }
            }

            return null;
        }

        ~Engine ()
        {
            Dispose(false);
        }

        public void SetActive ()
        {
            m_active = this;
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}