#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

class Pipeline;

enum class e_ShaderType
{
    Null = -1,
    VertexShader,
    PixelShader,
    GeometryShader
};

class Shader
{
private:

protected:
    Pipeline*      m_pipeline;

    VkShaderModule m_shaderModule;

public:
    Shader(Pipeline* a_pipeline);
    virtual ~Shader();

    virtual e_ShaderType GetShaderType() const = 0;

    inline VkShaderModule GetModule() const
    {
        return m_shaderModule;
    }
};