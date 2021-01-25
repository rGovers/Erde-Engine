using Erde.Application;
using Erde.Application.Internal;
using Erde.Graphics.Internal;
using Erde.Graphics.GUI;
using Erde.Graphics.Shader;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Erde.Graphics
{
    public class Pipeline : IDisposable
    {
        static Pipeline Instance;

        Application.Application m_application;

        IPipeline               m_internalPipeline;

        List<Canvas>            m_gui;

        bool                    m_displayConsole;

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
                return Instance;
            }
        }

        public e_ApplicationType ApplicationType
        {
            get
            {
                return m_application.ApplicationType;
            }
        }

        public IPipeline InternalPipeline
        {
            get
            {
                return m_internalPipeline;
            }
        }

        public Graphics GraphicsPipeline
        {
            get
            {
                return m_internalPipeline.GraphicsPipeline;
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

        public Application.Application Application
        {
            get
            {
                return m_application;
            }
        }

        public Pipeline (Application.Application a_application) 
        {
            Debug.Assert(Instance == null);
            Instance = this;

            m_application = a_application;

            m_gui = new List<Canvas>();

            if (a_application.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalPipeline = new OpenTKPipeline(this);
            }
            else
            {
                m_internalPipeline = new NativePipeline(this);
            }

            m_application.Update += SyncedUpdate;
            m_application.Closing += ShutDown;

            GraphicsCommand.Init(this);
        }

        public void AddObject(IGraphicsObject a_object)
        {
            m_internalPipeline.AddObject(a_object);
        }
        public void RemoveObject(IGraphicsObject a_object)
        {
            m_internalPipeline.RemoveObject(a_object);
        }

        void ShutDown ()
        {
            m_internalPipeline.Shutdown();
        }

        public void Dispose ()
        {
            m_internalPipeline.Dispose();
        }

        void SyncedUpdate ()
        {
            if (m_gui != null && m_application != null)
            {
                Canvas[] canvasList = m_gui.ToArray();

                foreach (Canvas canvas in canvasList)
                {
                    if (canvas != null)
                    {
                        canvas.Update(new Vector2(m_application.Width, m_application.Height));
                    }
                }
            }
        }
    }
}