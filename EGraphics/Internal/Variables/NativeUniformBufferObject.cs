using Erde.Graphics.Rendering;
using Erde.Graphics.Variables;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Variables
{
    class NativeUniformBufferObject : IUniformBufferObject
    {   
        class UniformBufferInitializer : IGraphicsObject
        {
            NativePipeline            m_pipeline;

            NativeUniformBufferObject m_ubo;

            uint                      m_bindingIndex;

            public UniformBufferInitializer(NativeUniformBufferObject a_ubo, uint a_index, NativePipeline a_pipeline)
            {
                m_pipeline = a_pipeline;

                m_ubo = a_ubo;

                m_bindingIndex = a_index;
            }

            public void Dispose()
            {

            }

            public void ModifyObject ()
            {
                uint size = (uint)Marshal.SizeOf(m_ubo.Object);

                m_ubo.Handle = UniformBufferObject_new(size, m_bindingIndex, m_pipeline.Handle);
            }

            public void DisposeObject ()
            {

            }
        }

        IntPtr  m_handle;
            
        object  m_object;

        internal IntPtr Handle
        {
            get
            {
                return m_handle;
            }
            set
            {
                m_handle = value;
            }
        }

        internal object Object
        {
            get
            {
                return m_object;
            }
        }

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr UniformBufferObject_new(uint a_size, uint a_index, IntPtr a_pipeline);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void UniformBufferObject_delete(IntPtr a_ptr);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void UniformBufferObject_UpdateBuffer(IntPtr a_ptr, IntPtr a_data);

        public NativeUniformBufferObject(object a_object, uint a_bindingIndex, Pipeline a_pipeline)
        {
            m_object = a_object;

            a_pipeline.AddObject(new UniformBufferInitializer(this, a_bindingIndex, (NativePipeline)a_pipeline.InternalPipeline));
        }

        public void Dispose()
        {

        }

        public void ModifyObject()
        {
            
        }
        public void DisposeObject()
        {
            UniformBufferObject_delete(m_handle);
        }

        public void UpdateData(object a_object)
        {
            m_object = a_object;
        }

        public void UpdateBuffer()
        {
            int size = Marshal.SizeOf(m_object);

            IntPtr data = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m_object, data, true);

            UniformBufferObject_UpdateBuffer(m_handle, data);

            Marshal.FreeHGlobal(data);
        }

        public void Bind (BindableContainer a_container, Binding a_binding)
        {

        }
    }
}