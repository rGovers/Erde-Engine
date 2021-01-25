#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

class Pipeline;

class Model
{
private:
    Pipeline*      m_pipeline;
    
    VkBuffer       m_buffer;
    VkDeviceMemory m_bufferMemory;

    unsigned int   m_indexOffset;
    unsigned int   m_indexCount;

    void InitBuffer(void* a_data, unsigned int a_dataSize, unsigned int a_offset);
protected:

public:
    Model(void* a_vertexData, unsigned int a_vertexDataSize, unsigned short* a_indexData, unsigned int a_indexCount, Pipeline* a_pipeline);
    ~Model();

    inline unsigned int GetIndexCount() const
    {
        return m_indexCount;
    }
    inline unsigned int GetIndexOffset() const
    {
        return m_indexOffset;
    }
};