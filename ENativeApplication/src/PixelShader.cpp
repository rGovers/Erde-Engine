#include "PixelShader.h"

#include <assert.h>
#include <shaderc/shaderc.h>
#include <stdio.h>
#include <string.h>

#include "Export.h"
#include "Pipeline.h"

const char* TEST_P_SHADER = "#version 450 \n\
#extension GL_ARB_separate_shader_objects : enable \n\
 \n\
layout(location = 0) in vec3 fragColor; \n\
 \n\
layout(location = 0) out vec4 outColor; \n\
 \n\
void main() { \n\
    outColor = vec4(fragColor, 1.0); \n\
}";

PixelShader::PixelShader(const char* a_source, Pipeline* a_pipeline) :
    Shader(a_pipeline)
{
    shaderc_compiler_t compiler = shaderc_compiler_initialize();
    
    shaderc_compile_options_t options = shaderc_compile_options_initialize();
    shaderc_compile_options_set_target_spirv(options, shaderc_spirv_version_1_0);

    shaderc_compilation_result_t result = shaderc_compile_into_spv(compiler, TEST_P_SHADER, strlen(TEST_P_SHADER), shaderc_glsl_fragment_shader, "ShaderP", "main", options);

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
PixelShader::~PixelShader()
{

}

e_ShaderType PixelShader::GetShaderType() const
{
    return e_ShaderType::PixelShader;
}

EExportFunc(PixelShader*, PixelShader_new(const char* a_source, Pipeline* a_pipeline));
EExportFunc(void, PixelShader_delete(PixelShader* a_ptr));

PixelShader* PixelShader_new(const char* a_source, Pipeline* a_pipeline) { return new PixelShader(a_source, a_pipeline); }
void PixelShader_delete(PixelShader* a_ptr) { delete a_ptr; }