using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using System;
using System.Drawing;

namespace Erde.Graphics
{
    public interface IGraphicsCommand
    {
        void SetViewport(Rectangle a_rect);

        void BindProgram(Program a_program);

        void BindRenderTexture(RenderTexture a_renderTexture);

        void BindTexture(Program a_program, int a_binding, Texture a_texture, int a_index);
        
        void BindFloat(Program a_program, int a_binding, float a_value);

        void BindVector2(Program a_program, int a_binding, Vector2 a_value);
        void BindVector3(Program a_program, int a_binding, Vector3 a_value);
        void BindVector4(Program a_program, int a_binding, Vector4 a_value);

        void BindMatrix4(Program a_program, int a_binding, Matrix4 a_matrix);
        void BindMatrix4(Program a_program, int a_binding, Matrix4[] a_data);

        void Draw(uint a_indicies);
        void DrawTriangles(uint a_indices);

        void DrawElements(uint a_indices);
        void DrawElementsUInt(uint a_indices);
    }
}