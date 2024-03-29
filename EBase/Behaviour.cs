﻿using OpenTK;
using System;
using System.Reflection;

namespace Erde
{
    public abstract class Behaviour : Component
    {
        public delegate void Event ();
        public delegate void CollisionEvent(object a_other, Vector3 a_normal, Vector3 a_pos);

        Event          m_start;
        Event          m_update;
        Event          m_physicsUpdate;
        CollisionEvent m_collision;
        
        internal Event Start
        {
            get
            {
                return m_start;
            }
        }

        internal Event Update
        {
            get
            {
                return m_update;
            }
        }

        internal Event PhysicsUpdate
        {
            get
            {
                return m_physicsUpdate;
            }
        }

        internal CollisionEvent OnCollision
        {
            get
            {
                return m_collision;
            }
        }

        public Behaviour ()
        {
            Type type = GetType();

            MethodInfo method = type.GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                m_start = (Event)method.CreateDelegate(typeof(Event), this);
            }

            method = type.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                m_update = (Event)method.CreateDelegate(typeof(Event), this);
            }

            method = type.GetMethod("PhysicsUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                m_physicsUpdate = (Event)method.CreateDelegate(typeof(Event), this);
            }

            method = type.GetMethod("OnCollision", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                m_collision = (CollisionEvent)method.CreateDelegate(typeof(CollisionEvent), this);
            }
        }
    }
}