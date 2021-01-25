using Erde.Graphics.Internal.Shader;
using Erde.Graphics.Rendering;
using Erde.Graphics.Variables;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Variables
{
    public class OpenTKUniformBufferObject : IUniformBufferObject
    {
        int      m_handle;
        object   m_object;

        uint     m_binding;

        internal int Handle
        {
            get
            {
                return m_handle;
            }
        }

        public OpenTKUniformBufferObject(object a_object, uint a_binding)
        {
            m_object = a_object;

            m_binding = a_binding;
        }
        
        public void Dispose ()
        {
            
        }

        public void ModifyObject ()
        {
            m_handle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.UniformBuffer, m_handle);

            int size = Marshal.SizeOf(m_object);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m_object, ptr, false);

            GL.BufferData(BufferTarget.UniformBuffer, size, ptr, BufferUsageHint.DynamicDraw);

            Marshal.FreeHGlobal(ptr);
        }

        public void DisposeObject ()
        {
            GL.DeleteBuffer(m_handle);
        }

        public void UpdateData (object a_object)
        {
            m_object = a_object;
        }

        public void UpdateBuffer ()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, m_handle);
            IntPtr buffer = GL.MapBuffer(BufferTarget.UniformBuffer, BufferAccess.WriteOnly);

            Marshal.StructureToPtr(m_object, buffer, true);

            GL.UnmapBuffer(BufferTarget.UniformBuffer);
        }

        public void Bind (BindableContainer a_container, Binding a_binding)
        {
            OpenTKProgram program = (OpenTKProgram)a_binding.Program.InternalObject;
            int handle = program.Handle;

            GL.UniformBlockBinding(handle, a_binding.Handle, Material.EndUBOIndex + a_container.UniformBufferObjects);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.EndUBOIndex + a_container.UniformBufferObjects++, m_handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Material UBO: ");
#endif
        }
    }
}