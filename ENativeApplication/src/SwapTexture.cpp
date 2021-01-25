#include "SwapTexture.h"

#include <assert.h>
#include <string.h>

#include "Pipeline.h"
#include "SwapChain.h"
#include "Texture.h"

SwapTexture::SwapTexture(unsigned int a_width, unsigned int a_height, const VkSurfaceFormatKHR& a_format, Pipeline* a_pipeline, SwapChain* a_swapChain)
{
    m_pipeline = a_pipeline;

    const VkDevice device = m_pipeline->GetDevice();
    const VkSwapchainKHR swapChain = a_swapChain->GetSwapChain();

    vkGetSwapchainImagesKHR(device, swapChain, &m_imageCount, nullptr);
    VkImage* swapChainImages = new VkImage[m_imageCount];

    vkGetSwapchainImagesKHR(device, swapChain, &m_imageCount, swapChainImages);

    m_texture = new Texture*[m_imageCount];

    for (int i = 0; i < m_imageCount; ++i)
    {   
        // By what I can tell I do not own the textures so setting the flag for the texture to not be destroyed
        m_texture[i] = new Texture(swapChainImages[i], a_width, a_height, a_format, m_pipeline, false);
    }

    VkAttachmentDescription colorAttachment;
    memset(&colorAttachment, 0, sizeof(colorAttachment));
    
    colorAttachment.format = a_format.format;
    colorAttachment.samples = VK_SAMPLE_COUNT_1_BIT;

    colorAttachment.loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
    colorAttachment.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
    colorAttachment.stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
    colorAttachment.stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
    colorAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    colorAttachment.finalLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

    VkAttachmentReference colorAttachmentRef;
    memset(&colorAttachmentRef, 0, sizeof(colorAttachmentRef));

    colorAttachmentRef.attachment = 0;
    colorAttachmentRef.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

    VkSubpassDescription subpass;
    memset(&subpass, 0, sizeof(subpass));

    subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
    subpass.colorAttachmentCount = 1;
    subpass.pColorAttachments = &colorAttachmentRef;

    VkSubpassDependency dependency;
    memset(&dependency, 0, sizeof(dependency));
    
    dependency.srcSubpass = VK_SUBPASS_EXTERNAL;
    dependency.srcStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    dependency.dstStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    dependency.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;

    VkRenderPassCreateInfo renderPassInfo;
    memset(&renderPassInfo, 0, sizeof(renderPassInfo));

    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
    renderPassInfo.attachmentCount = 1;
    renderPassInfo.pAttachments = &colorAttachment;
    renderPassInfo.subpassCount = 1;
    renderPassInfo.pSubpasses = &subpass;
    renderPassInfo.dependencyCount = 1;
    renderPassInfo.pDependencies = &dependency;

    assert(vkCreateRenderPass(device, &renderPassInfo, nullptr, &m_renderPass) == VK_SUCCESS);

    m_frameBuffers = new VkFramebuffer[m_imageCount];

    for (int i = 0; i < m_imageCount; ++i)
    {
        VkImageView attachments[] = 
        {
            m_texture[i]->GetImageView()
        };

        VkFramebufferCreateInfo frameBufferInfo;
        memset(&frameBufferInfo, 0, sizeof(frameBufferInfo));

        frameBufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
        frameBufferInfo.renderPass = m_renderPass;
        frameBufferInfo.attachmentCount = 1;
        frameBufferInfo.pAttachments = attachments;
        frameBufferInfo.width = m_texture[i]->GetWidth();
        frameBufferInfo.height = m_texture[i]->GetHeight();
        frameBufferInfo.layers = 1;

        assert(vkCreateFramebuffer(device, &frameBufferInfo, nullptr, &m_frameBuffers[i]) == VK_SUCCESS);
    }
}
SwapTexture::~SwapTexture()
{
    const VkDevice device = m_pipeline->GetDevice();

    for (int i = 0; i < m_imageCount; ++i)
    {
        vkDestroyFramebuffer(device, m_frameBuffers[i], nullptr);

        delete m_texture[i];
    }

    delete[] m_texture;
    delete[] m_frameBuffers;

    vkDestroyRenderPass(device, m_renderPass, nullptr);
}