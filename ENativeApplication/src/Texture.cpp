#include "Texture.h"

#include <assert.h>
#include <string.h>

#include "Pipeline.h"

Texture::Texture(VkImage& a_image, unsigned int a_width, unsigned int a_height, const VkSurfaceFormatKHR& a_format, Pipeline* a_pipeline, bool a_destroy)
{
    m_width = a_width;
    m_height = a_height;

    m_destroy = a_destroy;

    m_pipeline = a_pipeline;

    m_image = a_image;

    m_format = a_format;

    VkImageViewCreateInfo createInfo;
    memset(&createInfo, 0, sizeof(createInfo));

    createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
    createInfo.image = m_image;
    createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    createInfo.format = m_format.format;
    createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    createInfo.subresourceRange.baseMipLevel = 0;
    createInfo.subresourceRange.levelCount = 1;
    createInfo.subresourceRange.baseArrayLayer = 0;
    createInfo.subresourceRange.layerCount = 1;

    assert(vkCreateImageView(m_pipeline->GetDevice(), &createInfo, nullptr, &m_imageView) == VK_SUCCESS); 
}
Texture::~Texture()
{
    const VkDevice device = m_pipeline->GetDevice();

    vkDestroyImageView(device, m_imageView, nullptr);

    if (m_destroy)
    {
        vkDestroyImage(device, m_image, nullptr);
    }
}