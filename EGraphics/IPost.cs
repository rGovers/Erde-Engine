using Erde.Graphics.Variables;

namespace Erde.Graphics
{
    public interface IPost
    {
        void Effect (RenderTexture a_source, Camera a_camera, Texture a_normal, Texture a_spec, Texture a_depth);
    }
}