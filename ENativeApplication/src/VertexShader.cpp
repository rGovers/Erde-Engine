#include "VertexShader.h"

#include <assert.h>
#include <shaderc/shaderc.h>
#include <stdio.h>
#include <string.h>

#include "Export.h"
#include "Pipeline.h"

const char* TEST_V_SHADER = "#version 450 \n\
#extension GL_ARB_separate_shader_objects : enable \n\
 \n\
layout(location = 0) out vec3 fragColor; \n\
 \n\
vec2 positions[3] = vec2[]( \n\
    vec2(0.0, -0.5), \n\
    vec2(0.5, 0.5), \n\
    vec2(-0.5, 0.5) \n\
); \n\
 \n\
vec3 colors[3] = vec3[]( \n\
    vec3(1.0, 0.0, 0.0), \n\
    vec3(0.0, 1.0, 0.0), \n\
    vec3(0.0, 0.0, 1.0) \n\
); \n\
 \n\
void main() { \n\
    gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0); \n\
    fragColor = colors[gl_VertexIndex]; \n\
}";

VertexShader::VertexShader(const char* a_source, Pipeline* a_pipeline) :
    Shader(a_pipeline)
{
    shaderc_compiler_t compiler = shaderc_compiler_initialize();
    
    shaderc_compile_options_t options = shaderc_compile_options_initialize();
    shaderc_compile_options_set_target_spirv(options, shaderc_spirv_version_1_0);

    shaderc_compilation_result_t result = shaderc_compile_into_spv(compiler, a_source, strlen(a_source), shaderc_glsl_vertex_shader, "ShaderV", "main", options);

    if (shaderc_result_get_compilation_status(result) != shaderc_compilation_status_success)
    {
        printf("GLSL to SPIR-V: Compilation Error: %s \n", shaderc_result_get_error_message(result));
    }

    VkShaderModuleCreateInfo createInfo;
    memset(&createInfo, 0, sizeof(createInfo));

    createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
    createInfo.codeSize = shaderc_result_get_length(result);
    createInfo.pCode = (unsigned int*)shaderc_result_get_bytes(result);

    assert(vkCreateShaderModule(m_pipeline->GetDevice(), &createInfo, nullptr, &m_shaderModule) == VK_SUCCESS);

    shaderc_result_release(result);
    shaderc_compile_options_release(options);
    shaderc_compiler_release(compiler);
}
VertexShader::~VertexShader()
{
    
}

e_ShaderType VertexShader::GetShaderType() const
{
    return e_ShaderType::VertexShader;
}

EExportFunc(VertexShader*, VertexShader_new(const char* a_source, Pipeline* a_pipeline));
EExportFunc(void, VertexShader_delete(VertexShader* a_ptr));

VertexShader* VertexShader_new(const char* a_source, Pipeline* a_pipeline) { return new VertexShader(a_source, a_pipeline); }
void VertexShader_delete(VertexShader* a_ptr) { delete a_ptr; }