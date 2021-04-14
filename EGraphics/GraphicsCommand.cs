using Erde.Application;
using Erde.Graphics.Internal;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using System;
using System.Drawing;

namespace Erde.Graphics
{
    public static class GraphicsCommand
    {
        static IGraphicsCommand InternalObject;

        internal static void Init(Pipeline a_pipeline)
        {
            if (a_pipeline.Application.ApplicationType == e_ApplicationType.Managed)
            {
                InternalObject = new OpenTKGraphicsCommand(a_pipeline);
            }
            else
            {
                
            }
        }

        public static void SetViewport(Rectangle a_rect)
        {
            InternalObject.SetViewport(a_rect);
        }

        public static void BindProgram(Program a_program)
        {
            InternalObject.BindProgram(a_program);
        }

        public static void BindRenderTexture(RenderTexture a_renderTexture)
        {
            InternalObject.BindRenderTexture(a_renderTexture);
        }

        public static void BindModel(Model a_model)
        {
            a_model.Bind();
        }

        public static void BindTexture(Program a_program, int a_binding, Texture a_texture, int a_index)
        {
            InternalObject.BindTexture(a_program, a_binding, a_texture, a_index);
        }
        
        public static void BindFloat(Program a_program, int a_binding, float a_value)
        {
            InternalObject.BindFloat(a_program, a_binding, a_value);
        }

        public static void BindVector2(Program a_program, int a_binding, Vector2 a_value)
        {
            InternalObject.BindVector2(a_program, a_binding, a_value);
        }
        public static void BindVector3(Program a_program, int a_binding, Vector3 a_value)
        {
            InternalObject.BindVector3(a_program, a_binding, a_value);
        }
        public static void BindVector4(Program a_program, int a_binding, Vector4 a_value)
        {
            InternalObject.BindVector4(a_program, a_binding, a_value);
        }

        public static void BindMatrix4(Program a_program, int a_binding, Matrix4 a_matrix)
        {
            InternalObject.BindMatrix4(a_program, a_binding, a_matrix);
        }
        public static void BindMatrix4(Program a_program, int a_binding, Matrix4[] a_data)
        {
            InternalObject.BindMatrix4(a_program, a_binding, a_data);
        }

        public static void UpdateTextureRGBA(Texture a_texture, IntPtr a_data)
        {
            a_texture.WriteData(a_texture.Width, a_texture.Height, e_PixelFormat.BGRA, e_InternalPixelFormat.RGBA, e_PixelType.UnsignedByte, a_data);
        }

        public static void Draw(uint a_indices = 4)
        {
            InternalObject.Draw(a_indices);
        }
        public static void DrawTriangles(uint a_indices = 6)
        {
            InternalObject.DrawTriangles(a_indices);
        }
        
        public static void DrawElements(uint a_indices)
        {
            InternalObject.DrawElements(a_indices);
        }
        public static void DrawElementsUInt(uint a_indices)
        {
            InternalObject.DrawElementsUInt(a_indices);
        }
    }
}