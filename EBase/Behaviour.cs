using System.Reflection;

namespace Erde
{
    public abstract class Behaviour : Component
    {
        public delegate void Event ();

        Event m_start;
        Event m_update;
        
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

        public Behaviour ()
        {
            MethodInfo method = GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                m_start = (Event)method.CreateDelegate(typeof(Event), this);
            }

            method = GetType().GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                m_update = (Event)method.CreateDelegate(typeof(Event), this);
            }
        }
    }
}