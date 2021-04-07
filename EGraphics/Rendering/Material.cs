using Erde.Graphics;
using Erde.Graphics.Shader;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Erde.Graphics.Rendering
{
    public enum e_TransparencyMode
    {
        Null = 0,
        Opaque = 1,
        Transparent = 2
    };

    public class Material : IRenderObject, IGraphicsObject
    {
        Program            m_shader;
        
        e_TransparencyMode m_transparencyMode;

        Pipeline           m_pipeline;
        Graphics           m_graphics;

        List<Binding>      m_bindings;

        internal List<Binding> Bindings
        {
            get
            {
                return m_bindings;
            }
        }

        public Program Program
        {
            get
            {
                return m_shader;
            }
        }

        public e_TransparencyMode Transparency
        {
            get
            {
                return m_transparencyMode;
            }
        }

        internal const int EndUBOIndex = 5;
        internal const int TransformUBOIndex = 0;
        internal const int CameraUBOIndex = 1;
        internal const int LightUBOIndex = 2;
        internal const int TimeUBOIndex = 3;

        public void BindObject (int a_binding, IMaterialBindable a_object)
        {
            Binding bind = null;
            for (int i = 0; i < m_bindings.Count; ++i)
            {
                bind = m_bindings[i];
                if (bind.Handle == a_binding)
                {
                    bind.Target = a_object;

                    return;
                }
            }

            while (!m_shader.Initialized)
            {
                Thread.Yield();
            }

            bind = new Binding(a_binding, a_object, m_shader);

            m_bindings.Add(bind);
        }

        public Material (Program a_shader, e_TransparencyMode a_transparencyMode, Pipeline a_pipeline, Graphics a_graphics)
        {
            m_shader = a_shader;

            m_transparencyMode = a_transparencyMode;

            m_bindings = new List<Binding>();

            m_pipeline = a_pipeline;
            m_graphics = a_graphics;

            m_pipeline.AddObject(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_graphics.RemoveObject(this);
        }

        ~Material ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            
        }

        public void DisposeObject ()
        {
        }

        public void AddObject (LinkedList<DrawingContainer> a_objects, LinkedList<Renderer> a_renderers)
        {
            
        }

        public void RemoveObject (LinkedList<DrawingContainer> a_objects, LinkedList<Renderer> a_renderers)
        {
            for (LinkedListNode<DrawingContainer> cont = a_objects.First; cont != null; cont = cont.Next)
            {
                if (cont.Value.Material == this)
                {
                    cont.Value.Renderers.Clear();
                    a_objects.Remove(cont);
                }
            }
        }
    }
}