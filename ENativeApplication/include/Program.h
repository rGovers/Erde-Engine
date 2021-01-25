#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

#include <map>
#include <vector>

class GeometryShader;
class Pipeline;
class PixelShader;
class RenderTexture;
class VertexShader;

enum class e_FieldType : int
{
    Float = 0
};

struct VertexInfo
{
    // Marshaling joy there is better ways to do it but lazy
    void* Offset;
    unsigned int Count;
    e_FieldType Type;
};

class Program
{
private:
    Pipeline*                                    m_pipeline;

    std::vector<VkPipelineShaderStageCreateInfo> m_shaderStages;

    VkPipelineLayout                             m_pipelineLayout;

    std::map<RenderTexture*, VkPipeline>         m_shaderPipelines;

    int                                          m_vertexInfoCount;
    VertexInfo*                                  m_vertexInfo;

    int                                          m_vertexSize;

protected:

public:
    Program(const VertexShader* a_vertexShader, const GeometryShader* a_geometryShader, const PixelShader* a_pixelShader, VertexInfo* a_vertexInfo, int a_vertexInfoCount, int a_vertexSize, Pipeline* a_pipeline);
    ~Program();

    VkPipeline GetShaderPipeline(RenderTexture* a_renderTexture) const;

    VkPipeline AddShaderPipeline(RenderTexture* a_renderTexture);
    void RemoveShaderPipeline(RenderTexture* a_renderTexture);
    void ClearShaderPipeline(); 
};