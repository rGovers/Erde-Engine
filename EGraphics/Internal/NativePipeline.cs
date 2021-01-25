using Erde.Application.Internal;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Erde.Graphics.Internal
{
    public class NativePipeline : IPipeline
    {
        IntPtr       m_handle;

        Pipeline     m_pipeline;
    
        Graphics     m_graphics;
    
        Thread       m_thread;

        bool         m_shutDown;
        bool         m_destroy;
        bool         m_joinable;

        PipelineTime m_time;

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr Pipeline_new(string a_appName, IntPtr a_app);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Pipeline_delete(IntPtr a_ptr);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern bool Pipeline_PreUpdate(IntPtr a_ptr, IntPtr a_graphics);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Pipeline_PostUpdate(IntPtr a_ptr, IntPtr a_graphics);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Pipeline_Resize(IntPtr a_ptr);

        public Graphics GraphicsPipeline
        {
            get
            {
                return m_graphics;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return m_handle;
            }
        }

        public NativePipeline(Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            m_shutDown = false;
            m_joinable = false;
            m_destroy = false;

            m_graphics = new Graphics(m_pipeline);

            m_thread = new Thread(Run)
            {
                Name = "Drawing",
                Priority = ThreadPriority.AboveNormal
            };
            m_thread.Start();
        }

        public void AddObject(IGraphicsObject a_object)
        {
            while (m_handle == IntPtr.Zero)
            {
                Thread.Yield();
            }

            a_object.ModifyObject();   
        }
        public void RemoveObject(IGraphicsObject a_object)
        {
            while (m_handle == IntPtr.Zero)
            {
                Thread.Yield();
            }

            a_object.DisposeObject();
        }

        void StartUp(Application.Application a_app)
        {
            NativeApplication appInternal = a_app.InternalApplication as NativeApplication;

            m_time = new PipelineTime();

            m_handle = Pipeline_new(a_app.Title, appInternal.Handle);
        }

        void Resize()
        {
            Pipeline_Resize(m_handle);
        }

        void Run()
        {
            Application.Application app = m_pipeline.Application;

            app.Resize += Resize;

            StartUp(app);

            m_graphics.Init();

            IntPtr graphicsHandle = ((NativeGraphics)m_graphics.InternalGraphics).Handle;

            while (!m_shutDown)
            {
                if (Pipeline_PreUpdate(m_handle, graphicsHandle))
                {
                    m_time.Update();

                    m_graphics.Update();

                    Pipeline_PostUpdate(m_handle, graphicsHandle);
                }
            }

            m_graphics.Dispose();

            Pipeline_delete(m_handle);

            m_joinable = true;
        }

        public void Shutdown()
        {
            m_destroy = true;
            m_shutDown = true;
        }

        public void Dispose()
        {
            while (!m_joinable)
            {
                Thread.Yield();
            }

            m_thread.Join();
        }
    }
}