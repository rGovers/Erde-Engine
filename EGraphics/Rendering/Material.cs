using Erde.Graphics.Shader;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Erde.Graphics.Rendering
{
    public class Material : IRenderObject, IGLObject
    {
        public class Binding : IGLObject
        {
            string            m_binding;
            int               m_bindingIndex = -1;
            IMaterialBindable m_object;

            int               m_program = -1;

            internal int Handle
            {
                get
                {
                    return m_bindingIndex;
                }
                set
                {
                    m_bindingIndex = value;
                }
            }

            internal int ProgramHandle
            {
                get
                {
                    return m_program;
                }
            }

            public string BindingName
            {
                get
                {
                    return m_binding;
                }
            }

            public IMaterialBindable Target
            {
                get
                {
                    return m_object;
                }
                set
                {
                    m_object = value;
                }
            }

            public Binding (string a_binding, IMaterialBindable a_object, int a_program)
            {
                m_binding = a_binding;
                m_program = a_program;
                m_object = a_object;
            }

            public void ModifyObject ()
            {
                if (m_object is Variables.UniformBufferObject)
                {
                    m_bindingIndex = GL.GetUniformBlockIndex(m_program, m_binding);

                    Pipeline.GLError("Material: Binding: ");

                    return;
                }

                m_bindingIndex = GL.GetUniformLocation(m_program, m_binding);

                Pipeline.GLError("Material: Binding: ");
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }
        }

        // Stores the shader associated with [this]
        // This can be a cloned shader or shared
        Program       m_shader;

        // Stores the binding location of the UBOs in the shader
        int           m_transformBinding;

        int           m_cameraBinding;
        int           m_timeBinding;

        // Stores the GPU pipeline associated with this
        Pipeline      m_pipeline;

        // Stores the drawing pipeline that [this] is using
        Graphics      m_graphics;

        // Stores the variables to be bound to the shader
        List<Binding> m_bindings;

        internal int TransformBinding
        {
            get
            {
                return m_transformBinding;
            }
        }

        internal int CameraBinding
        {
            get
            {
                return m_cameraBinding;
            }
        }

        internal int TimeBinding
        {
            get
            {
                return m_timeBinding;
            }
        }

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

        internal const int EndUBOIndex = 5;
        internal const int TransformUBOIndex = 0;
        internal const int CameraUBOIndex = 1;
        internal const int LightUBOIndex = 2;
        internal const int TimeUBOIndex = 3;

        public void BindObject (string a_binding, IMaterialBindable a_object)
        {
            Binding bind = null;
            // Checks if the variable is bound to the shader
            for (int i = 0; i < m_bindings.Count; ++i)
            {
                bind = m_bindings[i];
                if (bind.BindingName == a_binding)
                {
                    // Updates the variables value
                    bind.Target = a_object;

                    return;
                }
            }

            // Waits for the shader to be initialised by the drawing thread
            while (m_shader.Handle == -1)
            {
                Thread.Yield();
            }

            // Adds a new variable to the shader
            bind = new Binding(a_binding, a_object, m_shader.Handle);

            m_bindings.Add(bind);

            // Queues the initiaisation of the binding
            m_pipeline.InputQueue.Enqueue(bind);
        }

        public Material (Program a_shader, Pipeline a_pipeline, Graphics a_graphics)
        {
            m_shader = a_shader;

            m_transformBinding = -1;
            m_cameraBinding = -1;

            m_bindings = new List<Binding>();

            m_pipeline = a_pipeline;
            m_graphics = a_graphics;

            m_pipeline.InputQueue.Enqueue(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            // Queue the destruction of the material in rendering pipeline
            m_graphics.DisposalQueue.Enqueue(this);
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
            // Finds the transform UBO
            m_transformBinding = GL.GetUniformBlockIndex(m_shader.Handle, "transform");
            if (m_transformBinding != -1)
            {
                // Binds the transform UBO to the transform index
                GL.UniformBlockBinding(m_shader.Handle, m_transformBinding, TransformUBOIndex);
            }

            // Finds the camera UBO
            m_cameraBinding = GL.GetUniformBlockIndex(m_shader.Handle, "camera");
            if (m_cameraBinding != -1)
            {
                // Binds the camera UBO to the camera index
                GL.UniformBlockBinding(m_shader.Handle, m_cameraBinding, CameraUBOIndex);
            }

            m_timeBinding = GL.GetUniformBlockIndex(m_shader.Handle, "time");
            if (m_timeBinding != -1)
            {
                GL.UniformBlockBinding(m_shader.Handle, m_timeBinding, TimeUBOIndex);
            }
        }

        public void DisposeObject ()
        {
        }

        public void AddObject (LinkedList<Graphics.DrawingContainer> a_objects)
        {
            
        }

        public void RemoveObject (LinkedList<Graphics.DrawingContainer> a_objects)
        {
            // Removes the material from the list of objects
            for (LinkedListNode<Graphics.DrawingContainer> cont = a_objects.First; cont != null; cont = cont.Next)
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