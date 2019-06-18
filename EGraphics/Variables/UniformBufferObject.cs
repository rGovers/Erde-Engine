using Erde.Graphics.Rendering;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Variables
{
    public class UniformBufferObject : IGLObject, IMaterialBindable
    {
        int      m_ubo;
        object   m_object;

        bool     m_update;

        Pipeline m_pipeline;

        public bool Update
        {
            get
            {
                return m_update;
            }
        }

        public int Handle
        {
            get
            {
                return m_ubo;
            }
        }

        public UniformBufferObject (Pipeline a_pipeline, object a_object)
        {
            m_object = a_object;
            m_update = false;

            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~UniformBufferObject ()
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
            m_ubo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.UniformBuffer, m_ubo);

            int size = Marshal.SizeOf(m_object);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m_object, ptr, false);

            GL.BufferData(BufferTarget.UniformBuffer, size, ptr, BufferUsageHint.DynamicDraw);

            Marshal.FreeHGlobal(ptr);
        }

        public void UpdateData (object a_object)
        {
            m_object = a_object;
            m_update = true;
        }

        public void UpdateBuffer ()
        {
            if (m_update)
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, m_ubo);
                IntPtr buffer = GL.MapBuffer(BufferTarget.UniformBuffer, BufferAccess.WriteOnly);

                Marshal.StructureToPtr(m_object, buffer, true);

                GL.UnmapBuffer(BufferTarget.UniformBuffer);

                m_update = false;
            }
        }

        public void Bind (BindableContainer a_container, Material.Binding a_binding)
        {
            UpdateBuffer();

            GL.UniformBlockBinding(a_binding.ProgramHandle, a_binding.Handle, Material.EndUBOIndex + a_container.UniformBufferObjects);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.EndUBOIndex + a_container.UniformBufferObjects++, Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Material UBO: ");
#endif
        }

        public void DisposeObject ()
        {
            GL.DeleteBuffer(m_ubo);
        }
    }
}