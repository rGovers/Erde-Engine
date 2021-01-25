#include "Shader.h"

#include "Pipeline.h"

Shader::Shader(Pipeline* a_pipeline)
{
    m_pipeline = a_pipeline;
}
Shader::~Shader()
{   
    vkDestroyShaderModule(m_pipeline->GetDevice(), m_shaderModule, nullptr);
}