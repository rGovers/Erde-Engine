using Erde.Graphics.Variables;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Variables
{
    class NativeModel : IModel
    {
        NativePipeline m_pipeline;
        IntPtr         m_handle;

        public IntPtr Handle
        {
            get
            {
                return m_handle;
            }
        }

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr Model_new(IntPtr a_vertexData, uint a_vertexDataSize, IntPtr a_indexData, uint a_indexCount, IntPtr a_pipeline);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Model_delete(IntPtr a_ptr);

        public NativeModel(Pipeline a_pipeline)
        {
            m_handle = IntPtr.Zero;

            m_pipeline = (NativePipeline)a_pipeline.InternalPipeline;
        }

        public void Dispose ()
        {
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            
        }

        public void DisposeObject ()
        {
            if (m_handle != IntPtr.Zero)
            {
                Model_delete(m_handle);
            }   
        }

        public void Bind()
        {
            
        }

        public void SetData<T>(T[] a_data, uint[] a_indices, ModelVertexInfo[] a_vertexInfo) where T : struct
        {
            uint length = (uint)a_data.LongLength;
            IntPtr data = Marshal.UnsafeAddrOfPinnedArrayElement(a_data, 0);
            IntPtr indexData = Marshal.UnsafeAddrOfPinnedArrayElement(a_indices, 0);

            if (m_handle == IntPtr.Zero)
            {
                m_handle = Model_new(data, (uint)Marshal.SizeOf<T>() * length, indexData, (uint)a_indices.LongLength, m_pipeline.Handle);
            }            
            else
            {
                
            }
        }
    }
}