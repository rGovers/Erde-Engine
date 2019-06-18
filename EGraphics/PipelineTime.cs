using System;
using System.Diagnostics;
using System.Threading;

namespace Erde.Graphics
{
    public class PipelineTime
    {
        static PipelineTime m_pipelineTime = null;

        double              m_deltaTime;

        double              m_timePassed;

        int                 m_frames;
        float               m_lastFrameCheck;
        float               m_fpsCheck;

        Stopwatch           m_stopwatch;

        public static double DeltaTime
        {
            get
            {
                if (m_pipelineTime != null)
                {
                    return m_pipelineTime.m_deltaTime;
                }

                return 0.0f;
            }
        }

        public static double TimePassed
        {
            get
            {
                if (m_pipelineTime != null)
                {
                    return m_pipelineTime.m_timePassed;
                }

                return 0.0f;
            }
        }

        public static float FPS
        {
            get
            {
                if (m_pipelineTime != null)
                {
                    return (float)Math.Floor(m_pipelineTime.m_lastFrameCheck * 100) / 100.0f;
                }

                return 0.0f;
            }
        }

        internal PipelineTime ()
        {
            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            m_deltaTime = 0.0f;
            m_timePassed = 0.0f;

            m_lastFrameCheck = 0.0f;
            m_fpsCheck = 0.0f;
            m_frames = 0;

            if (m_pipelineTime == null)
            {
                m_pipelineTime = this;
            }
        }

        internal void Update ()
        {
            m_deltaTime = m_stopwatch.Elapsed.TotalSeconds;

            while (m_deltaTime == 0.0 || m_deltaTime < 0.0001)
            {
                Thread.Yield();

                m_deltaTime = m_stopwatch.Elapsed.TotalSeconds;
            }

            m_stopwatch.Restart();
            m_timePassed += m_deltaTime;

            m_fpsCheck += (float)m_deltaTime;
            ++m_frames;

            if (m_fpsCheck >= 0.5f)
            {
                m_lastFrameCheck = m_frames / m_fpsCheck;

                m_fpsCheck -= 0.5f;
                m_frames = 0;
            }
        }
    }
};