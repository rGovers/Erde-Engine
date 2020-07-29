using Erde.Graphics.GUI;
using Erde.Graphics.Shader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Erde.Graphics
{
    public class Pipeline : IDisposable
    {
        IGraphicsContext           m_graphics;
        INativeWindow              m_window;

        Thread                     m_thread;
        ConcurrentQueue<IGLObject> m_inputQueue;
        ConcurrentQueue<IGLObject> m_disposalQueue;

        Graphics                   m_graphicsPipeline;

        PipelineTime               m_time;

        bool                       m_access = false;

        bool                       m_shutDown;
        bool                       m_destroy;
        bool                       m_joinable;

        static Pipeline            m_pipeline;

        List<Canvas>               m_gui;

        ConsoleDisplay             m_consoleDisplay;

        bool                       m_displayConsole;

        public ConcurrentQueue<IGLObject> InputQueue
        {
            get
            {
                return m_inputQueue;
            }
        }

        public ConcurrentQueue<IGLObject> DisposalQueue
        {
            get
            {
                return m_disposalQueue;
            }
        }

        public static void GLError (string a_error)
        {
            ErrorCode error = GL.GetError();

            if (error != ErrorCode.NoError)
            {
                InternalConsole.AddMessage(a_error + error.ToString(), InternalConsole.e_Alert.Error);
            }
        }

        public static Pipeline Active
        {
            get
            {
                return m_pipeline;
            }
        }

        public Thread DrawingThread
        {
            get
            {
                return m_thread;
            }
        }

        public Graphics GraphicsPipeline
        {
            get
            {
                return m_graphicsPipeline;
            }
        }

        public List<Canvas> GUIs
        {
            get
            {
                return m_gui;
            }
        }

        public bool DisplayConsole
        {
            get
            {
                return m_displayConsole;
            }
            set
            {
                m_displayConsole = value;
            }
        }

        public INativeWindow Window
        {
            get
            {
                return m_window;
            }
        }

        public Pipeline (Application.Application a_application) : this(a_application as INativeWindow)
        {
            a_application.Update += SyncedUpdate;
            a_application.Closing += ShutDown;
        }
        public Pipeline (INativeWindow a_window)
        {
            m_shutDown = false;

            m_window = a_window;

            m_inputQueue = new ConcurrentQueue<IGLObject>();
            m_disposalQueue = new ConcurrentQueue<IGLObject>();
            m_graphicsPipeline = new Graphics(this);

            m_consoleDisplay = new ConsoleDisplay(this);

            m_thread = new Thread(Run)
            {
                Name = "Drawing",
                Priority = ThreadPriority.AboveNormal
            };
            m_thread.Start();
        }

        void StartUp ()
        {
            // Sets the window as the current drawing context
            m_graphics = new GraphicsContext(GraphicsMode.Default, m_window.WindowInfo);

            // Fixes crash on Linux
            m_graphics.MakeCurrent(m_window.WindowInfo);

            // Loads all the OpenGL functions
            m_graphics.LoadAll();
            GL.ClearColor(Color.FromArgb(255, 25, 25, 25));

            // Looking at the base implementation
            // -1 Adaptive VSync
            // 0 VSync Off
            // 1 VSync On
            // Adaptive implementation is not complete yet either have to wait for update or implement myself
            m_graphics.SwapInterval = 1;

            GL.Enable(EnableCap.DepthTest);

            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);

            m_time = new PipelineTime();

            m_joinable = false;

            m_gui = new List<Canvas>();

            Shaders.InitShaders(this);

            GLError("Pipeline: Startup: ");
        }

        public void ShutDown ()
        {
            while (m_access) ;

            m_shutDown = true;
        }

        public void Dispose ()
        {
            m_destroy = true;

            Shaders.DestroyShaders();

            while (!m_joinable)
            {
                Console.Write("");
            };

            m_thread.Join();
        }

        void Run ()
        {
            StartUp();

            m_destroy = false;

            while (!m_shutDown)
            {
                m_time.Update();

                Input();
                Disposal();

                if (m_window.WindowState != WindowState.Minimized)
                {
                    Update();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            m_graphicsPipeline.Dispose();
            m_consoleDisplay.Dispose();

            while (!m_destroy || !m_disposalQueue.IsEmpty)
            {
                Disposal();
            }

            m_joinable = true;
        }

        public void SetActive ()
        {
            m_pipeline = this;
            m_graphicsPipeline.SetActive();
        }

        void Input ()
        {
            while (!m_inputQueue.IsEmpty)
            {
                IGLObject obj;

                if (!m_inputQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Pipeline: Input Dequeue Failed", InternalConsole.e_Alert.Warning);

                    return;
                }

                obj.ModifyObject();

                GLError("Pipeline: Input: ");
            }
        }

        void Disposal ()
        {
            while (!m_disposalQueue.IsEmpty)
            {
                IGLObject obj;

                if (!m_disposalQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Pipeline: Disposal Dequeue Failed", InternalConsole.e_Alert.Warning);

                    return;
                }

                obj.DisposeObject();

                GLError("Pipeline: Disposal: ");
            }
        }

        void Update ()
        {
            switch (Environment.OSVersion.Platform)
            {
            case PlatformID.MacOSX:
                {
                    m_graphics.Update(m_window.WindowInfo);

                    break;
                }
            }

            m_graphicsPipeline.Update();

            Vector2 size = new Vector2(m_window.Width, m_window.Height);

            foreach (Canvas canvas in m_gui)
            {
                if (canvas.Visible)
                {
                    canvas.Draw(size);
                }
            }

            m_consoleDisplay.Draw(size, m_displayConsole);

            // Thread safety nonsense
            // The buffer takes so long to switch it can be deleted mid way
            m_access = true;
            if (m_graphics != null)
            {
                if (PipelineTime.FPS <= 50.0f)
                {
                    m_graphics.SwapInterval = 0;
                }
                else
                {
                    m_graphics.SwapInterval = 1;
                }

                m_graphics.SwapBuffers();
            }
            m_access = false;

#if DEBUG_INFO
            GLError("Pipeline: Update: ");
#endif
        }

        void SyncedUpdate ()
        {
            if (m_gui != null)
            {
                foreach (Canvas canvas in m_gui)
                {
                    canvas.Update(new Vector2(m_window.Width, m_window.Height));
                }
            }
        }
        void ShutDown (object sender, EventArgs e)
        {
            ShutDown();
        }
    }
}