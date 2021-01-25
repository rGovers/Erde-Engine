#include "GeometryShader.h"

#include <assert.h>
#include <shaderc/shaderc.h>
#include <stdio.h>
#include <string.h>

#include "Export.h"
#include "Pipeline.h"

GeometryShader::GeometryShader(const char* a_source, Pipeline* a_pipeline) :
    Shader(a_pipeline)
{
    shaderc_compiler_t compiler = shaderc_compiler_initialize();
    
    shaderc_compile_options_t options = shaderc_compile_options_initialize();
    shaderc_compile_options_set_target_spirv(options, shaderc_spirv_version_1_0);

    shaderc_compilation_result_t result = shaderc_compile_into_spv(compiler, a_source, strlen(a_source), shaderc_glsl_geometry_shader, "ShaderP", "main", options);

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
GeometryShader::~GeometryShader()
{

}

e_ShaderType GeometryShader::GetShaderType() const
{
    return e_ShaderType::GeometryShader;
}

EExportFunc(GeometryShader*, GeometryShader_new(const char* a_source, Pipeline* a_pipeline));
EExportFunc(void, GeometryShader_delete(GeometryShader* a_ptr));

GeometryShader* GeometryShader_new(const char* a_source, Pipeline* a_pipeline) { return new GeometryShader(a_source, a_pipeline); }
void GeometryShader_delete(GeometryShader* a_ptr) { delete a_ptr; }