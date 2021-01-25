#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

#include "Texture.h"

class Pipeline;

class RenderTexture
{
private:

protected:
    unsigned int   m_imageCount;

    Pipeline*      m_pipeline;
    
    Texture**      m_texture;

    VkRenderPass   m_renderPass;

    VkFramebuffer* m_frameBuffers;

public:
    RenderTexture();
    virtual ~RenderTexture();

    inline VkRenderPass GetRenderPass() const
    {
        return m_renderPass;
    }

    inline VkFramebuffer GetFrameBuffer(unsigned int a_index) const
    {
        return m_frameBuffers[a_index];
    }

    inline unsigned int GetImageCount() const
    {
        return m_imageCount;
    }

    inline unsigned int GetWidth() const
    {
        return m_texture[0]->GetWidth();
    }
    inline unsigned int GetHeight() const
    {
        return m_texture[0]->GetHeight();
    }
};