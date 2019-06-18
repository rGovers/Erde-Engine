using System;
using System.Collections.Generic;
using System.Reflection;

namespace Erde.Physics
{
    public class Solver : ISolver
    {
        delegate bool CollisionDetection (out Engine.Resolution a_resolution, Collider a_colliderA, Collider a_colliderB);

        Dictionary<string, int> m_colliderLookup;

        CollisionDetection[] m_collisionSolvers;

        public Solver ()
        {
            List<Type> colliderTypes = new List<Type>();

            Type[] types = Assembly.GetAssembly(typeof(Collider)).GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Collider)))
                {
                    colliderTypes.Add(type);
                }
            }

            m_collisionSolvers = new CollisionDetection[colliderTypes.Count * colliderTypes.Count];
            m_colliderLookup = new Dictionary<string, int>();

            for (int i = 0; i < colliderTypes.Count; ++i)
            {
                ConstructorInfo constructorInfo = colliderTypes[i].GetConstructor(new Type[0]);

                Collider collider = constructorInfo.Invoke(new object[0]) as Collider;

                m_colliderLookup.Add(collider.ColliderType, i);

                for (int j = 0; j < colliderTypes.Count; ++j)
                {
                    m_collisionSolvers[i * colliderTypes.Count + j] = null;

                    string nameA = colliderTypes[i].Name;
                    string nameB = colliderTypes[j].Name;

                    string funcName = nameA + nameB;

                    MethodInfo methodInfo = colliderTypes[i].GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Static);

                    if (methodInfo != null)
                    {
                        Delegate del = methodInfo.CreateDelegate(typeof(CollisionDetection));

                        m_collisionSolvers[i * colliderTypes.Count + j] = (CollisionDetection)del;
                    }
                    else
                    {
                        methodInfo = colliderTypes[j].GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Static);

                        if (methodInfo != null)
                        {
                            Delegate del = methodInfo.CreateDelegate(typeof(CollisionDetection));

                            m_collisionSolvers[i * colliderTypes.Count + j] = (CollisionDetection)del;
                        }
                    }
                }
            }
        }

        public bool Collision (out Engine.Resolution a_resolution, Collider a_colliderA, Collider a_colliderB)
        {
            int colIndexA = m_colliderLookup[a_colliderA.ColliderType];
            int colIndexB = m_colliderLookup[a_colliderB.ColliderType];

            CollisionDetection collisionDetection = m_collisionSolvers[colIndexA * m_colliderLookup.Count + colIndexB];

            if (collisionDetection != null)
            {
                return collisionDetection(out a_resolution, a_colliderA, a_colliderB);
            }

            a_resolution = new Engine.Resolution();

            return false;
        }
    }
}