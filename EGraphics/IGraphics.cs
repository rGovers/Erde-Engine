using System;

using Erde.Graphics.Variables;

namespace Erde.Graphics
{
    interface IGraphics : IDisposable
    {
        MultiRenderTexture DefferedOutput
        {
            get;
        }
        MultiRenderTexture TransparentDefferedOutput
        {
            get;
        }

        void Init();
        void Update();

        void AddObject(IRenderObject a_object);
        void RemoveObject(IRenderObject a_object);
    }
}