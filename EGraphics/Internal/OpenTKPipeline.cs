using Erde.Application.Internal;
using Erde.Graphics.Shader;
using Erde.Graphics.GUI;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Erde.Graphics.Internal
{
    public class OpenTKPipeline : IPipeline
    {
        Thread                           m_thread;
        ConcurrentQueue<IGraphicsObject> m_inputQueue;
        ConcurrentQueue<IGraphicsObject> m_disposalQueue;

        IGraphicsContext                 m_graphics;
        
        ConsoleDisplay                   m_consoleDisplay;
        
        bool                             m_shutDown;
        bool                             m_destroy;
        bool                             m_joinable;
        
        int                              m_staticVAO;
        int                              m_staticIBO;
        int                              m_staticVBO;
        
        Graphics                         m_graphicsPipeline;
        
        PipelineTime                     m_time;
        
        Pipeline                         m_pipeline;

        public Graphics GraphicsPipeline
        {
            get
            {
                return m_graphicsPipeline;
            }
        }

        public int StaticVAO
        {
            get
            {
                return m_staticVAO;
            }
        }

        public bool ConsoleVisible
        {
            get
            {
                return m_consoleDisplay.DisplayConsole;
            }
            set
            {
                m_consoleDisplay.DisplayConsole = value;
            }
        }

        public OpenTKPipeline(Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            m_shutDown = false;
            m_joinable = false;
            m_destroy = false;

            m_inputQueue = new ConcurrentQueue<IGraphicsObject>();
            m_disposalQueue = new ConcurrentQueue<IGraphicsObject>();

            m_consoleDisplay = new ConsoleDisplay(m_pipeline);

            m_graphicsPipeline = new Graphics(m_pipeline);

            m_thread = new Thread(Run)
            {
                Name = "Drawing",
                Priority = ThreadPriority.AboveNormal
            };
            m_thread.Start();
        }

        public void AddObject(IGraphicsObject a_object)
        {
            m_inputQueue.Enqueue(a_object);
        }
        public void RemoveObject(IGraphicsObject a_object)
        {
            m_disposalQueue.Enqueue(a_object);
        }

        void Input ()
        {
            while (!m_inputQueue.IsEmpty)
            {
                IGraphicsObject obj;

                if (!m_inputQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Pipeline: Input Dequeue Failed", InternalConsole.e_Alert.Warning);

                    return;
                }

                obj.ModifyObject();

#if DEBUG_INFO
                Pipeline.GLError("Pipeline: Input: ");
#endif
            }
        }

        void Disposal ()
        {
            while (!m_disposalQueue.IsEmpty)
            {
                IGraphicsObject obj;

                if (!m_disposalQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Pipeline: Disposal Dequeue Failed", InternalConsole.e_Alert.Warning);

                    return;
                }

                obj.DisposeObject();

#if DEBUG_INFO
                Pipeline.GLError("Pipeline: Disposal: ");
#endif
            }
        }

        void StartUp (OpenTKApplication a_app)
        {
#if DEBUG_INFO
            m_graphics = new GraphicsContext(GraphicsMode.Default, a_app.WindowInfo, 4, 5, GraphicsContextFlags.Debug);
#else
            m_graphics = new GraphicsContext(GraphicsMode.Default, a_app.WindowInfo, 4, 5, GraphicsContextFlags.Default);
#endif

            m_graphics.MakeCurrent(a_app.WindowInfo);

            m_graphics.LoadAll();
            GL.ClearColor(Color.FromArgb(255, 25, 25, 25));

            m_graphics.SwapInterval = 1;

            m_staticVAO = GL.GenVertexArray();
            m_staticVBO = GL.GenBuffer();
            m_staticIBO = GL.GenBuffer();

            GL.BindVertexArray(m_staticVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_staticVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_staticIBO);

            GL.Enable(EnableCap.DepthTest);

            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);

            m_time = new PipelineTime();

            Shaders.InitShaders(m_pipeline);

#if DEBUG_INFO
            Pipeline.GLError("Pipeline: Startup: ");
#endif
        }

        void Run ()
        {
            OpenTKApplication app = m_pipeline.Application.InternalApplication as OpenTKApplication;

            StartUp(app);
            m_graphicsPipeline.Init();

            while (!m_shutDown)
            {
                m_time.Update();

                Input();
                Disposal();

                if (app.WindowState != WindowState.Minimized)
                {
                    Update(app);
                }
                else
                {
                    Thread.Yield();
                }
            }

            m_graphicsPipeline.Dispose();
            m_consoleDisplay.Dispose();

            while (!m_destroy || !m_disposalQueue.IsEmpty)
            {
                Disposal();
            }

            GL.DeleteBuffer(m_staticIBO);
            GL.DeleteBuffer(m_staticVBO);
            GL.DeleteVertexArray(m_staticVAO);

            m_joinable = true;
        }

        void Update (OpenTKApplication a_app)
        {
            switch (Environment.OSVersion.Platform)
            {
            case PlatformID.MacOSX:
                {
                    m_graphics.Update(a_app.WindowInfo);

                    break;
                }
            }

            m_graphicsPipeline.Update();

            Vector2 size = new Vector2(a_app.Width, a_app.Height);

            GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            List<Canvas> gui = m_pipeline.GUIs;
            foreach (Canvas canvas in gui)
            {
                if (canvas.Visible)
                {
                    canvas.Draw(size);
                }
            }

            m_consoleDisplay.Draw(size, m_pipeline.DisplayConsole);

            lock (this)
            {
                if (!m_shutDown && m_graphics != null)
                {
                    if (PipelineTime.FPS <= 55.0f)
                    {
                        m_graphics.SwapInterval = 0;
                    }
                    else
                    {
                        m_graphics.SwapInterval = 1;
                    }

                    m_graphics.SwapBuffers();
                }
            }

#if DEBUG_INFO
            Pipeline.GLError("Pipeline: Update: ");
#endif
        }

        public void Shutdown()
        {
            lock (this)
            {
                m_destroy = true;
                m_shutDown = true;
            }
        }

        public void Dispose()
        {
            Shaders.DestroyShaders();

            if (!m_shutDown)
            {
                Shutdown();
            }

            while (!m_joinable)
            {
                Thread.Yield();
            };

            m_thread.Join();
        }
    }
}