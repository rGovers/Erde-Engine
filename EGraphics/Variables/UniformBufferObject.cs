using Erde.Application;
using Erde.Graphics.Internal.Variables;
using Erde.Graphics.Rendering;
using System;

namespace Erde.Graphics.Variables
{
    public class UniformBufferObject : IUniformBufferObject
    {
        Pipeline             m_pipeline;
        
        bool                 m_update;

        IUniformBufferObject m_internalObject;

        public IUniformBufferObject InternalObject
        {
            get
            {
                return m_internalObject;
            }
        }

        internal bool Update
        {
            get
            {
                return m_update;
            }
        }

        public UniformBufferObject (object a_object, uint a_bindingIndex, Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;
            
            m_update = false;

            if (a_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKUniformBufferObject(a_object, a_bindingIndex);
            }
            else
            {
                m_internalObject = new NativeUniformBufferObject(a_object, a_bindingIndex, a_pipeline);
            }

            m_pipeline.AddObject(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);
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
            m_internalObject.ModifyObject();
        }

        public void UpdateData (object a_object)
        {
            m_internalObject.UpdateData(a_object);

            m_update = true;
        }

        public void UpdateBuffer ()
        {
            if (m_update)
            {
                m_internalObject.UpdateBuffer();

                m_update = false;
            }
        }

        public void Bind (BindableContainer a_container, Binding a_binding)
        {
            UpdateBuffer();

            m_internalObject.Bind(a_container, a_binding);
        }

        public void DisposeObject ()
        {
            m_internalObject.DisposeObject();
        }
    }
}