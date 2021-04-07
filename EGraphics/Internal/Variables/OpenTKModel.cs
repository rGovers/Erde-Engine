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
            if (m_vao != -1)
            {
                GL.DeleteBuffer(m_vbo);
                GL.DeleteBuffer(m_ibo);
                GL.DeleteVertexArray(m_vao);
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(m_vao);

            // I feel dirty for this
            GL.VertexAttrib4(3, 0.0f, 0.0f, 0.0f, 0.0f);
            GL.VertexAttrib4(4, 0.0f, 0.0f, 0.0f, 0.0f);
        }

        public void SetData<T>(T[] a_data, uint[] a_indices, ModelVertexInfo[] a_vertexInfo) where T : struct
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
            GL.BufferData(BufferTarget.ElementArrayBuffer, a_indices.Length * sizeof(uint), a_indices, BufferUsageHint.StaticDraw);

            int len = a_vertexInfo.Length;
            for (int i = 0; i < len; ++i)
            {
                VertexAttribPointerType fieldType = VertexAttribPointerType.Float;

                switch (a_vertexInfo[i].Type)
                {
                    case e_FieldType.Float:
                    {
                        fieldType = VertexAttribPointerType.Float;

                        break;
                    }
                    case e_FieldType.Int:
                    {
                        fieldType = VertexAttribPointerType.Int;

                        break;
                    }
                    case e_FieldType.UnsignedInt:
                    {
                        fieldType = VertexAttribPointerType.UnsignedInt;

                        break;
                    }
                    case e_FieldType.UnsignedByte:
                    {
                        fieldType = VertexAttribPointerType.UnsignedByte;

                        break;
                    }
                }

                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(i, (int)a_vertexInfo[i].Count, fieldType, a_vertexInfo[i].Normalize, vertexSize, a_vertexInfo[i].Offset);
            }
        }
    }
}