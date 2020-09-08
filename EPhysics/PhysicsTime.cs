using System;
using System.Diagnostics;
using System.Threading;

namespace Erde.Physics
{
    public class PhysicsTime
    {
        static PhysicsTime Instance = null;

        double    m_deltaTime;

        double    m_timePassed;

        int       m_frames;
        float     m_lastPhysicsUpdateCheck;
        float     m_pupsCheck;

        Stopwatch m_stopwatch;

        public static double DeltaTime
        {
            get
            {
                if (Instance != null)
                {
                    return Instance.m_deltaTime;
                }

                return 0.0f;
            }
        }

        public static double TimePassed
        {
            get
            {
                if (Instance != null)
                {
                    return Instance.m_timePassed;
                }

                return 0.0f;
            }
        }

        public static float PUPS
        {
            get
            {
                if (Instance != null)
                {
                    return (float)Math.Floor(Instance.m_lastPhysicsUpdateCheck * 100) / 100.0f;
                }

                return 0.0f;
            }
        }

        internal PhysicsTime ()
        {
            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            m_deltaTime = 0.0f;
            m_timePassed = 0.0f;

            m_lastPhysicsUpdateCheck = 0.0f;
            m_pupsCheck = 0.0f;
            m_frames = 0;

            if (Instance == null)
            {
                Instance = this;
            }
        }

        internal void Update ()
        {
            m_deltaTime = m_stopwatch.Elapsed.TotalSeconds;

            while (m_deltaTime < 0.0001)
            {
                Thread.Yield();

                m_deltaTime = m_stopwatch.Elapsed.TotalSeconds;
            }

            m_stopwatch.Restart();
            m_timePassed += m_deltaTime;

            m_pupsCheck += (float)m_deltaTime;
            ++m_frames;

            if (m_pupsCheck >= 0.5f)
            {
                m_lastPhysicsUpdateCheck = m_frames / m_pupsCheck;

                m_pupsCheck -= 0.5f;
                m_frames = 0;
            }
        }
    }
}
