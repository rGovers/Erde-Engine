using System;
using System.Diagnostics;
using System.Threading;

namespace Erde.Application
{
    public class Time
    {
        static Time m_time = null;
        
        double      m_deltaTime;
        double      m_timeScale;

        double      m_timePassed;

        int         m_updates;
        float       m_lastUpdateCheck;
        float       m_upsCheck;

        Stopwatch   m_stopwatch;

        public static float UnscaledDetaTime
        {
            get
            {
                return (float)m_time.m_deltaTime;
            }
        }
        public static double UnscaledDeltaTimeD
        {
            get
            {
                return m_time.m_deltaTime;
            }
        }

        public static float TimeScale
        {
            get
            {
                return (float)m_time.m_timeScale;
            }
            set
            {
                m_time.m_timeScale = (double)value;
            }
        }
        public static double TimeScaleD
        {
            get
            {
                return m_time.m_timeScale;
            }
            set
            {
                m_time.m_timeScale = value;
            }
        }

        public static double DeltaTimeD
        {
            get
            {
                return UnscaledDeltaTimeD * TimeScaleD;
            }
        }
        public static float DeltaTime
        {
            get
            {
                return UnscaledDetaTime * TimeScale;
            }
        }

        public static double TimePassedD
        {
            get
            {
                return m_time.m_timePassed;
            }
        }
        public static float TimePassed
        {
            get
            {
                return (float)TimePassedD;
            }
        }

        public static float UPS
        {
            get
            {
                return (float)Math.Floor(m_time.m_lastUpdateCheck * 100) / 100.0f;
            }
        }

        internal Time ()
        {
            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            m_timeScale = 1.0;

            m_timePassed = 0.0f;

            m_updates = 0;
            m_upsCheck = 0.5f;
            m_lastUpdateCheck = 0;

            if (m_time == null)
            {
                m_time = this;
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

            m_upsCheck += (float)m_deltaTime;
            ++m_updates;

            if (m_upsCheck >= 0.5f)
            {
                m_lastUpdateCheck = m_updates / m_upsCheck;

                m_upsCheck -= 0.5f;
                m_updates = 0;
            }
        }
    }
}