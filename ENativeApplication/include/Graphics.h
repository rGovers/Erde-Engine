#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

#include "Pipeline.h"

class Graphics
{
private:
    Pipeline*        m_pipeline;

    unsigned int     m_bufferCount;
    VkCommandBuffer* m_commandBuffers;

    void InitCommandBuffers();
protected:

public:
    Graphics(Pipeline* a_pipeline);
    ~Graphics();

    void Update(double a_delta);

    void RefreshCommandBuffers();

    inline VkCommandBuffer GetCommandBuffer() const
    {
        return m_commandBuffers[m_pipeline->GetImageIndex()];
    }
};


