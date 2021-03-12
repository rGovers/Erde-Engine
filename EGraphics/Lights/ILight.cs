using Erde.Graphics;
using Erde.Graphics.Rendering;

namespace Erde.Graphics.Lights
{
    public interface ILight : IGraphicsObject
    {
        void CalculateSplits(Camera a_camera);

        void BindShadowMap (BindableContainer a_bindableContainer);
        Frustum BindShadowDrawing (int a_index, Camera a_camera);

        Material BindLightDrawing ();
        Graphics.LightContainer GetLightData ();
    }
}