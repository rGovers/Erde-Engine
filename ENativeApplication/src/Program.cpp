#include "Program.h"

#include <assert.h>
#include <string.h>

#include "Export.h"
#include "GeometryShader.h"
#include "Pipeline.h"
#include "PixelShader.h"
#include "RenderTexture.h"
#include "SwapChain.h"
#include "VertexShader.h"

VkPipelineShaderStageCreateInfo CreateShaderStageInfo(const Shader* a_shader)
{
    VkPipelineShaderStageCreateInfo createInfo;
    memset(&createInfo, 0, sizeof(createInfo));

    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    createInfo.module = a_shader->GetModule();
    createInfo.pName = "main";

    switch (a_shader->GetShaderType())
    {
    case e_ShaderType::VertexShader:
    {
        createInfo.stage = VK_SHADER_STAGE_VERTEX_BIT;

        break;
    }
    case e_ShaderType::GeometryShader:
    {
        createInfo.stage = VK_SHADER_STAGE_GEOMETRY_BIT;

        break;
    }
    case e_ShaderType::PixelShader:
    {
        createInfo.stage = VK_SHADER_STAGE_FRAGMENT_BIT;

        break;
    }
    }

    return createInfo;
}

VkFormat GetFormat(e_FieldType a_fieldType, int a_count)
{
    // Feel like there is a better way however unsure as to how to do better
    switch (a_fieldType)
    {
    case e_FieldType::Float:
    {
        switch (a_count)
        {
        case 1:
        {
            return VK_FORMAT_R32_SFLOAT;
        }
        case 2:
        {
            return VK_FORMAT_R32G32_SFLOAT;
        }
        case 3:
        {
            return VK_FORMAT_R32G32B32_SFLOAT;
        }
        case 4:
        {
            return VK_FORMAT_R32G32B32A32_SFLOAT;
        }
        default:
        {
            assert(0);

            break;
        }
        }
        break;
    }
    default:
    {
        assert(0);

        break;
    }
    }
}

Program::Program(const VertexShader* a_vertexShader, const GeometryShader* a_geometryShader, const PixelShader* a_pixelShader, VertexInfo* a_vertexInfo, int a_vertexInfoCount, int a_vertexSize, Pipeline* a_pipeline)
{
    m_pipeline = a_pipeline;

    m_vertexSize = a_vertexSize;

    m_vertexInfoCount = a_vertexInfoCount;
    m_vertexInfo = a_vertexInfo;

    const VkDevice device = m_pipeline->GetDevice();
    const VkDescriptorSetLayout layout = m_pipeline->GetDescriptorSetLayout();

    if (a_vertexShader != nullptr)
    {
        m_shaderStages.emplace_back(CreateShaderStageInfo(a_vertexShader));
    }

    if (a_geometryShader != nullptr)
    {
        m_shaderStages.emplace_back(CreateShaderStageInfo(a_geometryShader));
    }

    if (a_pixelShader != nullptr)
    {
        m_shaderStages.emplace_back(CreateShaderStageInfo(a_pixelShader));
    }    

    VkPipelineLayoutCreateInfo pipelineLayoutInfo;
    memset(&pipelineLayoutInfo, 0, sizeof(pipelineLayoutInfo));

    pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
    pipelineLayoutInfo.setLayoutCount = 1;
    pipelineLayoutInfo.pSetLayouts = &layout;

    assert(vkCreatePipelineLayout(device, &pipelineLayoutInfo, nullptr, &m_pipelineLayout) == VK_SUCCESS);

    AddShaderPipeline((RenderTexture*)m_pipeline->GetSwapChain()->GetRenderTexture());
}
Program::~Program()
{
    const VkDevice device = m_pipeline->GetDevice();

    ClearShaderPipeline();

    vkDestroyPipelineLayout(device, m_pipelineLayout, nullptr);
}

VkPipeline Program::GetShaderPipeline(RenderTexture* a_renderTexture) const
{
    auto iter = m_shaderPipelines.find(a_renderTexture);

    if (iter != m_shaderPipelines.end())
    {
        return iter->second;
    }

    return nullptr;
}

VkPipeline Program::AddShaderPipeline(RenderTexture* a_renderTexture)
{
    VkVertexInputBindingDescription bindingDescription;
    bindingDescription.binding = 0;
    bindingDescription.stride = m_vertexSize;
    bindingDescription.inputRate = VK_VERTEX_INPUT_RATE_VERTEX;

    VkVertexInputAttributeDescription* attributeInput = new VkVertexInputAttributeDescription[m_vertexInfoCount];

    for (int i = 0; i < m_vertexInfoCount; ++i)
    {
        // 2 step cast to stop compiler errors
        const intptr_t offset = (intptr_t)m_vertexInfo[i].Offset;

        attributeInput[i].binding = 0;
        attributeInput[i].location = i;
        attributeInput[i].format = GetFormat(m_vertexInfo[i].Type, m_vertexInfo[i].Count);        
        // Down cast but using local address so if there is an issue there are larger issues
        attributeInput[i].offset = (unsigned int)offset;
    }

    VkPipelineVertexInputStateCreateInfo vertexInputInfo;
    memset(&vertexInputInfo, 0, sizeof(vertexInputInfo));

    vertexInputInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
    vertexInputInfo.vertexBindingDescriptionCount = 1;
    vertexInputInfo.pVertexBindingDescriptions = &bindingDescription;
    vertexInputInfo.vertexAttributeDescriptionCount = m_vertexInfoCount;
    vertexInputInfo.pVertexAttributeDescriptions = attributeInput;

    VkPipelineInputAssemblyStateCreateInfo inputAssembly;
    memset(&inputAssembly, 0, sizeof(inputAssembly));

    inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
    inputAssembly.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
    inputAssembly.primitiveRestartEnable = VK_FALSE;

    const int width = a_renderTexture->GetWidth();
    const int height = a_renderTexture->GetHeight(); 

    VkViewport viewport;
    viewport.x = 0.0f;
    viewport.y = 0.0f;
    viewport.width = width;
    viewport.height = height;
    viewport.minDepth = 0.0f;
    viewport.maxDepth = 1.0f;
    
    VkRect2D scissor;
    scissor.offset = { 0, 0 };
    scissor.extent = { width, height };

    VkPipelineViewportStateCreateInfo viewportState;
    memset(&viewportState, 0, sizeof(viewportState));

    viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
    viewportState.viewportCount = 1;
    viewportState.pViewports = &viewport;
    viewportState.scissorCount = 1;
    viewportState.pScissors = &scissor;

    VkPipelineRasterizationStateCreateInfo rasterizer;
    memset(&rasterizer, 0, sizeof(rasterizer));

    rasterizer.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
    rasterizer.depthBiasClamp = VK_FALSE;
    rasterizer.rasterizerDiscardEnable = VK_FALSE;
    rasterizer.polygonMode = VK_POLYGON_MODE_FILL;
    rasterizer.lineWidth = 1.0f;
    rasterizer.cullMode = VK_CULL_MODE_BACK_BIT;
    rasterizer.frontFace = VK_FRONT_FACE_COUNTER_CLOCKWISE;
    rasterizer.depthBiasEnable = VK_FALSE;

    VkPipelineMultisampleStateCreateInfo multiSampling;
    memset(&multiSampling, 0, sizeof(multiSampling));

    multiSampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
    multiSampling.sampleShadingEnable = VK_FALSE;
    multiSampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;

    VkPipelineColorBlendAttachmentState colorBlendAttachment;
    memset(&colorBlendAttachment, 0, sizeof(colorBlendAttachment));

    colorBlendAttachment.colorWriteMask = VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
    colorBlendAttachment.blendEnable = VK_FALSE;

    VkPipelineColorBlendStateCreateInfo colorBlending;
    memset(&colorBlending, 0, sizeof(colorBlending));

    colorBlending.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
    colorBlending.logicOpEnable = VK_FALSE;
    colorBlending.attachmentCount = 1;
    colorBlending.pAttachments = &colorBlendAttachment;

    // VkDynamicState dynamicStates[] = 
    // {
    //     VK_DYNAMIC_STATE_VIEWPORT
    // };

    // VkPipelineDynamicStateCreateInfo dynamicState;
    // memset(&dynamicState, 0, sizeof(dynamicState));
    // 
    // dynamicState.dynamicStateCount = 1;
    // dynamicState.pDynamicStates = dynamicStates;

    VkGraphicsPipelineCreateInfo pipelineInfo;
    memset(&pipelineInfo, 0, sizeof(pipelineInfo));

    pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
    pipelineInfo.stageCount = 2;
    pipelineInfo.pStages = m_shaderStages.data();
    pipelineInfo.pVertexInputState = &vertexInputInfo;
    pipelineInfo.pInputAssemblyState = &inputAssembly;
    pipelineInfo.pViewportState = &viewportState;
    pipelineInfo.pRasterizationState = &rasterizer;
    pipelineInfo.pMultisampleState = &multiSampling;
    pipelineInfo.pColorBlendState = &colorBlending;
    pipelineInfo.layout = m_pipelineLayout;
    pipelineInfo.renderPass = a_renderTexture->GetRenderPass();
    pipelineInfo.basePipelineIndex = -1;

    VkPipeline pipeline;
    assert(vkCreateGraphicsPipelines(m_pipeline->GetDevice(), VK_NULL_HANDLE, 1, &pipelineInfo, nullptr, &pipeline) == VK_SUCCESS);

    delete[] attributeInput;
    
    m_shaderPipelines.insert(std::map<RenderTexture*, VkPipeline>::value_type(a_renderTexture, pipeline));

    return pipeline;
}
void Program::RemoveShaderPipeline(RenderTexture* a_renderTexture)
{
    auto iter = m_shaderPipelines.find(a_renderTexture);

    if (iter == m_shaderPipelines.end())
    {
        return;
    }

    vkDestroyPipeline(m_pipeline->GetDevice(), iter->second, nullptr);

    m_shaderPipelines.erase(iter);
}
void Program::ClearShaderPipeline()
{
    const VkDevice device = m_pipeline->GetDevice();

    for (auto iter = m_shaderPipelines.begin(); iter != m_shaderPipelines.end(); ++iter)
    {
        vkDestroyPipeline(device, iter->second, nullptr);
    }

    m_shaderPipelines.clear();
}

EExportFunc(Program*, Program_new(const VertexShader* a_vertexShader, const GeometryShader* a_geometryShader, const PixelShader* a_pixelShader, VertexInfo* a_vertexInfo, int a_vertexInfoCount, int a_vertexSize, Pipeline* a_pipeline));
EExportFunc(void, Program_delete(Program* a_ptr));

Program* Program_new(const VertexShader* a_vertexShader, const GeometryShader* a_geometryShader, const PixelShader* a_pixelShader, VertexInfo* a_vertexInfo, int a_vertexInfoCount, int a_vertexSize, Pipeline* a_pipeline) { return new Program(a_vertexShader, a_geometryShader, a_pixelShader, a_vertexInfo, a_vertexInfoCount, a_vertexSize, a_pipeline); }
void Program_delete(Program* a_ptr) { delete a_ptr; }