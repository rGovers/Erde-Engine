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
        void BindMatrix4(Program a_program, int a_binding, Matrix4 a_matrix);

        void UpdateTextureRGBA(Texture a_texture, IntPtr a_data);

        void Draw();
        
        void DrawElements(uint a_indices);
        void DrawElementsUInt(uint a_indices);
    }
}