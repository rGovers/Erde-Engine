﻿namespace Erde.Graphics.Shader
{
    internal class Shaders
    {
        public const string DEFFERED_PIXEL = @"
#version 410

in vec2 vUV;

out vec4 fragColor;

uniform sampler2D diffuse;

void main()
{
    fragColor = texture(diffuse, vUV);
}";
        public const string DEFFERED_PIXEL_INVERTED = @"
#version 410

in vec2 vUV;

out vec4 fragColor;

uniform sampler2D diffuse;

void main()
{
    fragColor = texture(diffuse, vec2(vUV.x, 1 - vUV.y));
}";

        public const string DIRECTIONAL_VERTEX = @"
#version 410

layout(location = 0) in vec4 position;

uniform mat4 lvp;

uniform mat4 world;

void main()
{
    gl_Position = lvp * world * position;
}";
        public const string QUAD_VERTEX = @"
#version 410

out vec2 vUV;

void main()
{
    vec2 pos = vec2(gl_VertexID % 2, gl_VertexID / 2);
    gl_Position = vec4(pos.x * 2 - 1, pos.y * 2 - 1, 0.0f, 1.0f);
    vUV = pos;
}";

        public const string QUAD_TRANSFORM_VERTEX = @"
#version 410

uniform mat4 transform;

out vec2 vUV;

void main()
{
    vec2 tran = vec2(gl_VertexID % 2, gl_VertexID / 2);
    vec4 pos = transform * vec4(tran.x * 2 - 1, tran.y * 2 - 1, 0, 1);
    gl_Position = vec4(pos.xyz, 1);
    vUV = tran;
}
        ";

        public static Program TRANSFORM_IMAGE_SHADER;
        public static Program TRANSFORM_IMAGE_SHADER_INVERTED;

        public static void InitShaders (Pipeline a_pipeline)
        {
            PixelShader pixelShader = new PixelShader(DEFFERED_PIXEL, a_pipeline);
            PixelShader pixelShaderInverted = new PixelShader(DEFFERED_PIXEL_INVERTED, a_pipeline);
            VertexShader vertexShader = new VertexShader(QUAD_TRANSFORM_VERTEX, a_pipeline);

            TRANSFORM_IMAGE_SHADER = new Program(pixelShader, vertexShader, a_pipeline);
            TRANSFORM_IMAGE_SHADER_INVERTED = new Program(pixelShaderInverted, vertexShader, a_pipeline);

            Pipeline.GLError("Init Shaders: ");

            pixelShader.Dispose();
            pixelShaderInverted.Dispose();
            vertexShader.Dispose();
        }

        public static void DestroyShaders ()
        {
            TRANSFORM_IMAGE_SHADER.Dispose();
            TRANSFORM_IMAGE_SHADER = null;
            TRANSFORM_IMAGE_SHADER_INVERTED.Dispose();
            TRANSFORM_IMAGE_SHADER_INVERTED = null;
        }
    }
}
