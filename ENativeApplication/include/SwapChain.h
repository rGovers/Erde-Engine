#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

class Pipeline;
class SwapTexture;

class SwapChain
{
private:
    VkSwapchainKHR  m_swapChain;
    VkExtent2D      m_swapChainExtent;

    SwapTexture*    m_swapTexture;

    Pipeline*       m_pipeline;

    unsigned int    m_imageCount;

protected:

public:
    SwapChain(Pipeline* a_pipeline);
    ~SwapChain();

    inline VkSwapchainKHR GetSwapChain() const
    {
        return m_swapChain;
    }

    inline VkExtent2D GetExtents() const
    {
        return m_swapChainExtent;
    }

    inline SwapTexture* GetRenderTexture() const
    {
        return m_swapTexture;
    }

    inline unsigned int GetImageCount() const
    {
        return m_imageCount;
    }
};