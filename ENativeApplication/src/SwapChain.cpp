#include "SwapChain.h"

#include <algorithm>
#include <assert.h>
#include <string.h>

#include "Application.h"
#include "Pipeline.h"
#include "SwapTexture.h"

VkExtent2D ChooseSwapExtent(unsigned int a_width, unsigned int a_height, const VkSurfaceCapabilitiesKHR& a_capabilities)
{
    VkExtent2D extents;

    extents.width = std::max(a_capabilities.minImageExtent.width, std::min(a_capabilities.maxImageExtent.width, a_width));
    extents.height = std::max(a_capabilities.minImageExtent.height, std::min(a_capabilities.maxImageExtent.height, a_height));

    return extents;   
}

VkSurfaceFormatKHR ChooseSwapSurfaceFormat(const VkSurfaceFormatKHR* a_availableFormats, unsigned int a_formatCount)
{
    for (int i = 0; i < a_formatCount; ++i)
    {
        const VkSurfaceFormatKHR format = a_availableFormats[i];

        if (format.format == VK_FORMAT_B8G8R8A8_SRGB && a_availableFormats->colorSpace == VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
        {
            return format;
        }
    }

    return a_availableFormats[0];
}

SwapChain::SwapChain(Pipeline* a_pipeline)
{
    m_pipeline = a_pipeline;
    const Application* app = m_pipeline->GetApplication();

    const VkPhysicalDevice physicalDevice = m_pipeline->GetPhysicalDevice();
    const VkSurfaceKHR surface = m_pipeline->GetSurface();

    const SwapChainSupportDetails swapChainSupport = m_pipeline->QuerySwapChainSupport(physicalDevice, surface);

    const VkSurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats, swapChainSupport.FormatCount);
    const VkPresentModeKHR presentMode = VK_PRESENT_MODE_FIFO_KHR;

    m_swapChainExtent = ChooseSwapExtent(app->GetWidth(), app->GetHeight(), swapChainSupport.Capabilities);

    m_imageCount = swapChainSupport.Capabilities.minImageCount + 1; 

    if (swapChainSupport.Capabilities.maxImageCount > 0 && m_imageCount > swapChainSupport.Capabilities.maxImageCount)
    {
        m_imageCount = swapChainSupport.Capabilities.maxImageCount;
    }

    VkSwapchainCreateInfoKHR createInfo;
    memset(&createInfo, 0, sizeof(createInfo));

    createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    createInfo.surface = surface;
    createInfo.minImageCount = m_imageCount;
    createInfo.imageFormat = surfaceFormat.format;
    createInfo.imageColorSpace = surfaceFormat.colorSpace;
    createInfo.imageExtent = m_swapChainExtent;
    createInfo.imageArrayLayers = 1;
    createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

    const QueueFamilyIndices indices = m_pipeline->FindQueueFamily(physicalDevice);
    const unsigned int queueFamilyIndices[] = { indices.GraphicsFamily, indices.PresentFamily };

    if (indices.GraphicsFamily != indices.PresentFamily)
    {
        createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
        createInfo.queueFamilyIndexCount = 2;
        createInfo.pQueueFamilyIndices = queueFamilyIndices;
    }
    else
    {
        createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    }

    createInfo.preTransform = swapChainSupport.Capabilities.currentTransform;
    createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    createInfo.presentMode = presentMode;
    createInfo.clipped = VK_TRUE;
    createInfo.oldSwapchain = VK_NULL_HANDLE;

    assert(vkCreateSwapchainKHR(m_pipeline->GetDevice(), &createInfo, nullptr, &m_swapChain) == VK_SUCCESS);

    if (swapChainSupport.PresentModes != nullptr)
    {
        delete[] swapChainSupport.PresentModes;
    }
    if (swapChainSupport.Formats != nullptr)
    {
        delete[] swapChainSupport.Formats;
    }

    m_swapTexture = new SwapTexture(m_swapChainExtent.width, m_swapChainExtent.height, surfaceFormat, m_pipeline, this);
}
SwapChain::~SwapChain()
{
    const VkDevice device = m_pipeline->GetDevice();

    vkDeviceWaitIdle(device);

    delete m_swapTexture;

    vkDestroySwapchainKHR(device, m_swapChain, nullptr);
}