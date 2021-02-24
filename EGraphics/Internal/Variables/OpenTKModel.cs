using Erde.Graphics.Variables;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Variables
{
    class OpenTKModel : IModel
    {
        int m_vbo;
        int m_ibo;
        int m_vao;

        public int VertexBufferObject
        {
            get
            {
                return m_vbo;
            }
        }

        internal int IndexBufferObject
        {
            get
            {
                return m_ibo;
            }
        }

        public int VertexArrayObject
        {
            get
            {
                return m_vao;
            }
        }

        public OpenTKModel()
        {
            m_vbo = -1;
            m_ibo = -1;
            m_vao = -1;
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
            if (m_vao == -1)
            {
                GL.DeleteBuffer(m_vbo);
                GL.DeleteBuffer(m_ibo);
                GL.DeleteVertexArray(m_vao);
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(m_vao);
        }

        public void SetData<T>(T[] a_data, ushort[] a_indices) where T : struct
        {
            if (m_vao == -1)
            {
                m_vbo = GL.GenBuffer();
                m_ibo = GL.GenBuffer();
                m_vao = GL.GenVertexArray();
            }

            int vertexSize = Marshal.SizeOf<T>();

            GL.BindVertexArray(m_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, a_data.Length * vertexSize, a_data, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, a_indices.Length * sizeof(ushort), a_indices, BufferUsageHint.StaticDraw);

            ModelVertexInfo[] vertexInfo = ModelVertexInfo.GetVertexInfo<T>();
            int len = vertexInfo.Length;

            for (int i = 0; i < len; ++i)
            {
                VertexAttribPointerType fieldType = VertexAttribPointerType.Float;

                switch (vertexInfo[i].Type)
                {
                    case e_FieldType.Float:
                    {
                        fieldType = VertexAttribPointerType.Float;

                        break;
                    }
                    case e_FieldType.UnsignedInt:
                    {
                        fieldType = VertexAttribPointerType.UnsignedInt;

                        break;
                    }
                }

                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(i, (int)vertexInfo[i].Count, fieldType, false, vertexSize, vertexInfo[i].Offset);
            }
        }
    }
}