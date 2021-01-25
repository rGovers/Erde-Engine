using Erde.Graphics.Variables;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal
{
    public class NativeGraphics : IGraphics
    {
        Graphics m_graphics;

        IntPtr   m_handle;

        public MultiRenderTexture DefferedOutput
        {
            get
            {
                return null;
            }
        }

        internal IntPtr Handle
        {
            get
            {
                return m_handle;
            }
        }

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr Graphics_new(IntPtr a_pipeline);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Graphics_delete(IntPtr a_ptr);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Graphics_Update(IntPtr a_ptr, double a_delta);

        public NativeGraphics(Graphics a_graphics)
        {
            m_graphics = a_graphics;

            m_handle = IntPtr.Zero;
        }

        public void Init()
        {
            m_handle = Graphics_new(((NativePipeline)m_graphics.Pipeline.InternalPipeline).Handle);
        }

        public void AddObject(IRenderObject a_object)
        {
            
        }
        public void RemoveObject(IRenderObject a_object)
        {
            
        }

        public void Update()
        {
            Graphics_Update(m_handle, PipelineTime.DeltaTime);
        }

        public void Dispose()
        {
            Graphics_delete(m_handle);
        }
    }
}