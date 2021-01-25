#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

class Pipeline;

class UniformBufferObject
{
private:
    Pipeline*        m_pipeline;

    unsigned int     m_size;
    unsigned int     m_binding;

    VkBuffer*        m_uniformBuffers;
    VkDeviceMemory*  m_bufferMemory;

protected:

public:
    UniformBufferObject(unsigned int a_size, unsigned int a_binding, Pipeline* a_pipeline);
    ~UniformBufferObject();
    
    void UpdateBuffer(void* a_data);

    void InitBuffer();
    void DestroyBuffer();

    void InitDescriptor();
};
