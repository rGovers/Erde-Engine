using System;
using System.Diagnostics;
using System.Threading;

namespace Erde.Physics
{
    public class PhysicsTime
    {
        static PhysicsTime m_time = null;

        double             m_deltaTime;

        int                m_updates;
        float              m_lastUpdateCheck;
        float              m_pupsCheck;

        Stopwatch          m_stopwatch;

        public static float DeltaTime
        {
            get
            {
                return (float)m_time.m_deltaTime;
            }
        }

        public static double DeltaTimeD
        {
            get
            {
                return m_time.m_deltaTime;
            }
        }

        public static float PUPS
        {
            get
            {
                return (float)Math.Floor(m_time.m_lastUpdateCheck * 100) / 100.0f;
            }
        }

        internal PhysicsTime ()
        {
            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            m_updates = 0;
            m_pupsCheck = 0.5f;
            m_lastUpdateCheck = 0;

            if (m_time == null)
            {
                m_time = this;
            }
        }

        internal void Update ()
        {
            long time = Environment.TickCount;

            m_deltaTime = m_stopwatch.Elapsed.TotalSeconds;

            while (m_deltaTime == 0.0 || m_deltaTime < 0.001)
            {
                Thread.Yield();

                m_deltaTime = m_stopwatch.Elapsed.TotalSeconds;
            }

            m_stopwatch.Restart();

            m_pupsCheck += (float)m_deltaTime;
            ++m_updates;

            if (m_pupsCheck >= 0.5f)
            {
                m_lastUpdateCheck = m_updates / m_pupsCheck;

                m_pupsCheck -= 0.5f;
                m_updates = 0;
            }
        }
    }
}
