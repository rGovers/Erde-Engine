#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

class Pipeline;

class Texture
{
private:
    bool               m_destroy;

    Pipeline*          m_pipeline;
    
    VkImage            m_image;
    VkImageView        m_imageView;

    VkSurfaceFormatKHR m_format;
    
    unsigned int       m_width;
    unsigned int       m_height;

protected:

public:
    Texture(VkImage& a_image, unsigned int a_width, unsigned int a_height, const VkSurfaceFormatKHR& a_format, Pipeline* a_pipeline, bool a_destroy = true);
    ~Texture();

    inline VkImage GetImage() const
    {
        return m_image;
    }
    inline VkImageView GetImageView() const
    {
        return m_imageView;
    }

    inline unsigned int GetWidth() const
    {
        return m_width;
    }
    inline unsigned int GetHeight() const
    {
        return m_height;
    }
};
