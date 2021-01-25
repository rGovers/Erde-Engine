#include "Pipeline.h"

#include <assert.h>
#include <set>
#include <stdio.h>
#include <string.h>
#include <vector>

#include "Application.h"
#include "Export.h"
#include "Graphics.h"
#include "SwapChain.h"
#include "SwapTexture.h"
#include "UniformBufferObject.h"

const int MAX_FLIGHT_FRAMES = 2;

const std::vector<const char*> validationLayers = 
{
    "VK_LAYER_KHRONOS_validation"
};

const std::vector<const char*> deviceExtensions = 
{
    VK_KHR_SWAPCHAIN_EXTENSION_NAME
};

QueueFamilyIndices::QueueFamilyIndices()
{
    GraphicsFamily = -1;
    PresentFamily = -1;
}

SwapChainSupportDetails::SwapChainSupportDetails()
{
    memset(&Capabilities, 0, sizeof(Capabilities));

    FormatCount = 0;
    Formats = nullptr;

    PresentModeCount = 0;
    PresentModes = nullptr;
}

QueueFamilyIndices Pipeline::FindQueueFamily(const VkPhysicalDevice& a_device) const
{
    QueueFamilyIndices indices;

    unsigned int queueFamilyCount = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(a_device, &queueFamilyCount, nullptr);

    VkQueueFamilyProperties* queueFamilies = new VkQueueFamilyProperties[queueFamilyCount];
    vkGetPhysicalDeviceQueueFamilyProperties(a_device, &queueFamilyCount, queueFamilies);

    for (int i = 0; i < queueFamilyCount; ++i)
    {
        const VkQueueFamilyProperties queueFamily = queueFamilies[i];

        VkBool32 presentSupport;
        vkGetPhysicalDeviceSurfaceSupportKHR(a_device, i, m_surface, &presentSupport);

        if (presentSupport)
        {
            indices.PresentFamily = i;
        }

        if (queueFamily.queueFlags & VK_QUEUE_GRAPHICS_BIT)
        {
            indices.GraphicsFamily = i;
        }
    }

    delete[] queueFamilies;

    return indices;
}
SwapChainSupportDetails Pipeline::QuerySwapChainSupport(const VkPhysicalDevice& a_device, const VkSurfaceKHR& a_surface) const
{
    SwapChainSupportDetails details;
    vkGetPhysicalDeviceSurfaceCapabilitiesKHR(a_device, a_surface, &details.Capabilities);

    vkGetPhysicalDeviceSurfaceFormatsKHR(a_device, a_surface, &details.FormatCount, nullptr);
    if (details.FormatCount != 0)
    {
        details.Formats = new VkSurfaceFormatKHR[details.FormatCount];
        vkGetPhysicalDeviceSurfaceFormatsKHR(a_device, a_surface, &details.FormatCount, details.Formats);
    }

    vkGetPhysicalDeviceSurfacePresentModesKHR(a_device, a_surface, &details.PresentModeCount, nullptr);
    if (details.PresentModeCount != 0)
    {
        details.PresentModes = new VkPresentModeKHR[details.PresentModeCount];
        vkGetPhysicalDeviceSurfacePresentModesKHR(a_device, a_surface, &details.PresentModeCount, details.PresentModes);
    }

    return details;
}
unsigned int Pipeline::FindMemoryType(unsigned int a_typeFilter, VkMemoryPropertyFlags a_properties) const
{
    VkPhysicalDeviceMemoryProperties memProperties;
    vkGetPhysicalDeviceMemoryProperties(m_physicalDevice, &memProperties);

    for (unsigned int i = 0; i < memProperties.memoryTypeCount; ++i)
    {
        if (a_typeFilter & (1 << i) && (memProperties.memoryTypes[i].propertyFlags & a_properties) == a_properties)
        {
            return i;
        }
    }

    assert(0);

    return -1;
}

bool SupportsValidationLayers()
{
    unsigned int layerCount;
    vkEnumerateInstanceLayerProperties(&layerCount, nullptr);

    VkLayerProperties* availableLayers = new VkLayerProperties[layerCount];
    vkEnumerateInstanceLayerProperties(&layerCount, availableLayers);

    for (int i = 0; i < validationLayers.size(); ++i)
    {
        bool layerFound = false;

        const char* layerName = validationLayers[i];

        for (int j = 0; j < layerCount; ++j)
        {
            if (strcmp(layerName, availableLayers[j].layerName) == 0)
            {
                layerFound = true;

                break;
            }
        }

        if (!layerFound)
        {
            delete[] availableLayers;

            return false;
        }
    }

    delete[] availableLayers;

    return true;
}

std::vector<const char*> GetExtensions(bool a_vlEnabled)
{
    uint32_t extensionCount;
    const char** extensions = glfwGetRequiredInstanceExtensions(&extensionCount);

    std::vector<const char*> ext = std::vector<const char*>(extensions, extensions + extensionCount);
    if (a_vlEnabled)
    {
        ext.push_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
    }

    return ext;
}

VKAPI_ATTR VkBool32 VKAPI_CALL DebugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT a_messageSeverity, 
    VkDebugUtilsMessageTypeFlagsEXT a_messageType,
    const VkDebugUtilsMessengerCallbackDataEXT* a_callbackData,
    void* a_userData)
{
    printf("Vulkan Validation Layer: %s \n", a_callbackData->pMessage);

    return VK_FALSE;
}

void Pipeline::InitInstance(const char* a_appName)
{
    VkApplicationInfo appInfo;
    memset(&appInfo, 0, sizeof(appInfo));

    appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    appInfo.pApplicationName = a_appName;
    appInfo.applicationVersion = VK_MAKE_VERSION(1, 0, 0);
    appInfo.pEngineName = "Erde Engine";
    appInfo.engineVersion = VK_MAKE_VERSION(1, 0, 0);
    appInfo.apiVersion = VK_API_VERSION_1_0;

    VkInstanceCreateInfo createInfo;
    memset(&createInfo, 0, sizeof(createInfo));

    createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    createInfo.pApplicationInfo = &appInfo;

    m_enableValidationLayers = false;

#ifndef NDEBUG
    if (SupportsValidationLayers())
    {
        m_enableValidationLayers = true;

        createInfo.enabledLayerCount = (unsigned int)validationLayers.size();
        createInfo.ppEnabledLayerNames = validationLayers.data();

        VkDebugUtilsMessengerCreateInfoEXT debugCreateInfo;
        memset(&debugCreateInfo, 0, sizeof(debugCreateInfo));

        debugCreateInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
        debugCreateInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
        debugCreateInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
        debugCreateInfo.pfnUserCallback = DebugCallback;

        createInfo.pNext = &debugCreateInfo;
    }
#endif

    std::vector<const char*> ext = GetExtensions(m_enableValidationLayers);

    createInfo.enabledExtensionCount = (unsigned int)ext.size();
    createInfo.ppEnabledExtensionNames = ext.data();

    assert(vkCreateInstance(&createInfo, nullptr, &m_instance) == VK_SUCCESS);

    if (m_enableValidationLayers)
    {
        VkDebugUtilsMessengerCreateInfoEXT debugCreateInfo;
        memset(&debugCreateInfo, 0, sizeof(debugCreateInfo));

        debugCreateInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
        debugCreateInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
        debugCreateInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
        debugCreateInfo.pfnUserCallback = DebugCallback;

        PFN_vkCreateDebugUtilsMessengerEXT func = (PFN_vkCreateDebugUtilsMessengerEXT)vkGetInstanceProcAddr(m_instance, "vkCreateDebugUtilsMessengerEXT");
        assert(func != nullptr);

        assert(func(m_instance, &debugCreateInfo, nullptr, &m_debugMessenger) == VK_SUCCESS);
    }
}

void Pipeline::InitSurface()
{
    assert(glfwCreateWindowSurface(m_instance, m_application->GetHandle(), nullptr, &m_surface) == VK_SUCCESS);
}

void Pipeline::InitDevice()
{
    m_physicalDevice = VK_NULL_HANDLE;

    unsigned int deviceCount;
    vkEnumeratePhysicalDevices(m_instance, &deviceCount, nullptr);

    assert(deviceCount != 0);

    VkPhysicalDevice* devices = new VkPhysicalDevice[deviceCount];
    vkEnumeratePhysicalDevices(m_instance, &deviceCount, devices);

    VkPhysicalDeviceProperties prevProperties;
    memset(&prevProperties, 0, sizeof(prevProperties));

    for (int i = 0; i < deviceCount; ++i)
    {
        VkPhysicalDevice device = devices[i];

        VkPhysicalDeviceFeatures features;
        vkGetPhysicalDeviceFeatures(device, &features);
        VkPhysicalDeviceProperties properties;
        vkGetPhysicalDeviceProperties(device, &properties);

        const QueueFamilyIndices indices = FindQueueFamily(device);

        unsigned int extensionCount;
        vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, nullptr);

        VkExtensionProperties* extensions = new VkExtensionProperties[extensionCount];
        vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, extensions);

        const unsigned int deviceExtensionCount = (unsigned int)deviceExtensions.size();
        unsigned int foundExtensions = 0;
        for (int j = 0; j < deviceExtensionCount; ++j)
        {
            for (int k = 0; k < extensionCount; ++k)
            {
                if (strcmp(extensions[k].extensionName, deviceExtensions[j]) == 0)
                {
                    ++foundExtensions;

                    break;
                }
            }
        }
        delete[] extensions;

        if (deviceExtensionCount != foundExtensions)
        {
            continue;
        }

        const SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device, m_surface);

        if (swapChainSupport.IsSuitable() && indices.IsSuitable() && features.geometryShader)
        {
            if (m_physicalDevice == VK_NULL_HANDLE)
            {
                m_physicalDevice = device;
                prevProperties = properties;
            }
            else if (prevProperties.deviceType != VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU)
            {
                m_physicalDevice = device;
                prevProperties = properties;
            }
        }

        if (swapChainSupport.Formats != nullptr)
        {
            delete[] swapChainSupport.Formats;
        }
        if (swapChainSupport.PresentModes != nullptr)
        {
            delete[] swapChainSupport.PresentModes;
        }
    }

    delete[] devices;

    assert(m_physicalDevice != VK_NULL_HANDLE);
}

void Pipeline::InitLogicalDevice()
{
    const QueueFamilyIndices indices = FindQueueFamily(m_physicalDevice);

    const std::set<unsigned int> uniqueQueues = { indices.GraphicsFamily, indices.PresentFamily };
    const unsigned int uniqueQueueCount = (unsigned int)uniqueQueues.size();
    VkDeviceQueueCreateInfo* queueCreateInfos = new VkDeviceQueueCreateInfo[uniqueQueueCount];

    const float priority = 1.0f;
    unsigned int index = 0;
    for (auto iter = uniqueQueues.begin(); iter != uniqueQueues.end(); ++iter)
    {
        VkDeviceQueueCreateInfo queueCreateInfo;
        memset(&queueCreateInfo, 0, sizeof(queueCreateInfo));

        queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
        queueCreateInfo.queueFamilyIndex = *iter;
        queueCreateInfo.queueCount = 1;
        queueCreateInfo.pQueuePriorities = &priority;

        queueCreateInfos[index] = queueCreateInfo;

        ++index;
    }

    VkPhysicalDeviceFeatures deviceFeatures;
    memset(&deviceFeatures, 0, sizeof(deviceFeatures));

    VkDeviceCreateInfo createInfo;
    memset(&createInfo, 0, sizeof(createInfo));

    createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    createInfo.pQueueCreateInfos = queueCreateInfos;
    createInfo.queueCreateInfoCount = uniqueQueueCount;
    createInfo.pEnabledFeatures = &deviceFeatures;
    createInfo.enabledLayerCount = 0;
    createInfo.enabledExtensionCount = (unsigned int)deviceExtensions.size();
    createInfo.ppEnabledExtensionNames = deviceExtensions.data();

    if (m_enableValidationLayers)
    {
        createInfo.enabledLayerCount = (unsigned int)validationLayers.size();
        createInfo.ppEnabledLayerNames = validationLayers.data();
    }

    assert(vkCreateDevice(m_physicalDevice, &createInfo, nullptr, &m_device) == VK_SUCCESS);

    vkGetDeviceQueue(m_device, indices.GraphicsFamily, 0, &m_graphicsQueue);
    vkGetDeviceQueue(m_device, indices.PresentFamily, 0, &m_presentQueue);

    delete[] queueCreateInfos;
}

void Pipeline::InitSyncObjects()
{
    VkSemaphoreCreateInfo semaphoreInfo;
    memset(&semaphoreInfo, 0, sizeof(semaphoreInfo));

    semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;

    VkFenceCreateInfo fenceInfo;
    memset(&fenceInfo, 0, sizeof(fenceInfo));

    fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
    fenceInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;

    m_imageSemaphore = new VkSemaphore[MAX_FLIGHT_FRAMES];
    m_renderSemaphore = new VkSemaphore[MAX_FLIGHT_FRAMES];
    m_flightFences = new VkFence[MAX_FLIGHT_FRAMES];
    m_flightImages = new VkFence[m_swapChain->GetRenderTexture()->GetImageCount()];

    for (int i = 0; i < MAX_FLIGHT_FRAMES; ++i)
    {
        assert(vkCreateSemaphore(m_device, &semaphoreInfo, nullptr, &m_imageSemaphore[i]) == VK_SUCCESS);
        assert(vkCreateSemaphore(m_device, &semaphoreInfo, nullptr, &m_renderSemaphore[i]) == VK_SUCCESS);
        assert(vkCreateFence(m_device, &fenceInfo, nullptr, &m_flightFences[i]) == VK_SUCCESS);
        m_flightImages[i] = VK_NULL_HANDLE;
    }
}

void Pipeline::InitCommandPool()
{
    const QueueFamilyIndices queueFamilyIndices = FindQueueFamily(m_physicalDevice);

    VkCommandPoolCreateInfo poolInfo;
    memset(&poolInfo, 0, sizeof(poolInfo));

    poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    poolInfo.queueFamilyIndex = queueFamilyIndices.GraphicsFamily;
    poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

    assert(vkCreateCommandPool(m_device, &poolInfo, nullptr, &m_commandPool) == VK_SUCCESS);
}
void Pipeline::CreateDescriptors()
{
    const unsigned int bindingCount = 4;
    const unsigned int imageCount = m_swapChain->GetImageCount();

    VkDescriptorSetLayoutBinding* uboLayoutBinding = new VkDescriptorSetLayoutBinding[bindingCount];
    for (int i = 0; i < bindingCount; ++i)
    {
        uboLayoutBinding[i].binding = i;
        uboLayoutBinding[i].descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
        uboLayoutBinding[i].descriptorCount = 1;
        uboLayoutBinding[i].stageFlags = VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT;
        uboLayoutBinding[i].pImmutableSamplers = nullptr;
    }
    
    VkDescriptorSetLayoutCreateInfo layoutInfo;
    memset(&layoutInfo, 0, sizeof(layoutInfo));

    layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
    layoutInfo.bindingCount = bindingCount;
    layoutInfo.pBindings = uboLayoutBinding;

    assert(vkCreateDescriptorSetLayout(m_device, &layoutInfo, nullptr, &m_descriptorLayout) == VK_SUCCESS);

    VkDescriptorPoolSize poolSize;
    poolSize.type =  VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER; 
    poolSize.descriptorCount = imageCount * bindingCount;

    VkDescriptorPoolCreateInfo poolInfo;
    memset(&poolInfo, 0, sizeof(poolInfo));

    poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
    poolInfo.poolSizeCount = 1;
    poolInfo.pPoolSizes = &poolSize;
    poolInfo.maxSets = imageCount;

    assert(vkCreateDescriptorPool(m_device, &poolInfo, nullptr, &m_descriptorPool) == VK_SUCCESS);

    VkDescriptorSetLayout* layouts = new VkDescriptorSetLayout[imageCount];
    for (int i = 0; i < imageCount; ++i)
    {
        layouts[i] = m_descriptorLayout;
    }

    VkDescriptorSetAllocateInfo allocInfo;
    memset(&allocInfo, 0, sizeof(allocInfo));

    allocInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
    allocInfo.descriptorPool = m_descriptorPool;
    allocInfo.descriptorSetCount = imageCount;
    allocInfo.pSetLayouts = layouts;

    m_descriptorSets = new VkDescriptorSet[imageCount];    

    assert(vkAllocateDescriptorSets(m_device, &allocInfo, m_descriptorSets) == VK_SUCCESS);

    delete[] layouts;
}
void Pipeline::DestroyDescriptors()
{
    vkDestroyDescriptorPool(m_device, m_descriptorPool, nullptr);
    vkDestroyDescriptorSetLayout(m_device, m_descriptorLayout, nullptr);

    delete[] m_descriptorSets;
}

Pipeline::Pipeline(const char* a_appName, Application* a_application)
{
    m_resize = false;

    m_application = a_application;

    InitInstance(a_appName);
    InitSurface();
    InitDevice();
    InitLogicalDevice();

    m_swapChain = new SwapChain(this);

    InitSyncObjects();
    InitCommandPool();

    CreateDescriptors();
}
Pipeline::~Pipeline()
{
    vkDeviceWaitIdle(m_device);

    DestroyDescriptors();

    vkDestroyCommandPool(m_device, m_commandPool, nullptr);

    for (int i = 0; i < MAX_FLIGHT_FRAMES; ++i)
    {
        vkDestroySemaphore(m_device, m_renderSemaphore[i], nullptr);
        vkDestroySemaphore(m_device, m_imageSemaphore[i], nullptr);
        vkDestroyFence(m_device, m_flightFences[i], nullptr);
    }

    delete[] m_imageSemaphore;
    delete[] m_renderSemaphore;
    delete[] m_flightFences;

    delete m_swapChain;

    if (m_enableValidationLayers)
    {
        PFN_vkDestroyDebugUtilsMessengerEXT func = (PFN_vkDestroyDebugUtilsMessengerEXT)vkGetInstanceProcAddr(m_instance, "vkDestroyDebugUtilsMessengerEXT");
        
        if (func != nullptr)
        {
            func(m_instance, m_debugMessenger, nullptr);
        }
    }

    vkDestroyDevice(m_device, nullptr);
    vkDestroySurfaceKHR(m_instance, m_surface, nullptr);
    vkDestroyInstance(m_instance, nullptr);
}

bool Pipeline::PreUpdate(Graphics* a_graphics)
{
    m_currFrame = (m_currFrame + 1) % MAX_FLIGHT_FRAMES;

    vkWaitForFences(m_device, 1, &m_flightFences[m_currFrame], VK_TRUE, UINT64_MAX);
    
    const VkResult result = vkAcquireNextImageKHR(m_device, m_swapChain->GetSwapChain(), UINT64_MAX, m_imageSemaphore[m_currFrame], VK_NULL_HANDLE, &m_imageIndex);  

    switch (result)
    {
    case VK_ERROR_OUT_OF_DATE_KHR:
    case VK_SUBOPTIMAL_KHR:
    {
        RefreshSwapChain(a_graphics);

        return false;
    }
    default:
    {
        assert(result == VK_SUCCESS);

        break;
    }
    }
    
    if (m_flightImages[m_imageIndex] != VK_NULL_HANDLE)
    {
        vkWaitForFences(m_device, 1, &m_flightImages[m_imageIndex], VK_TRUE, UINT64_MAX);
    }  

    m_flightImages[m_imageIndex] = m_flightFences[m_currFrame];

    return true;
}
void Pipeline::PostUpdate(Graphics* a_graphics)
{
    VkSubmitInfo submitInfo;
    memset(&submitInfo, 0, sizeof(submitInfo));

    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;

    const VkPipelineStageFlags waitStages[] =
    {
        VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT
    };

    const VkCommandBuffer commandBuffer = a_graphics->GetCommandBuffer();

    submitInfo.waitSemaphoreCount = 1;
    submitInfo.pWaitSemaphores = &m_imageSemaphore[m_currFrame];
    submitInfo.pWaitDstStageMask = waitStages;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &commandBuffer;
    submitInfo.signalSemaphoreCount = 1;
    submitInfo.pSignalSemaphores = &m_renderSemaphore[m_currFrame];

    vkResetFences(m_device, 1, &m_flightFences[m_currFrame]);

    assert(vkQueueSubmit(m_graphicsQueue, 1, &submitInfo, m_flightFences[m_currFrame]) == VK_SUCCESS);

    VkPresentInfoKHR presentInfo;
    memset(&presentInfo, 0, sizeof(presentInfo));

    const VkSwapchainKHR swapChain = m_swapChain->GetSwapChain();

    presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    presentInfo.waitSemaphoreCount = 1;
    presentInfo.pWaitSemaphores = &m_renderSemaphore[m_currFrame];
    presentInfo.swapchainCount = 1;
    presentInfo.pSwapchains = &swapChain;
    presentInfo.pImageIndices = &m_imageIndex;

    const VkResult result = vkQueuePresentKHR(m_presentQueue, &presentInfo);

    switch (result)
    {
    case VK_ERROR_OUT_OF_DATE_KHR:
    case VK_SUBOPTIMAL_KHR:
    {
        RefreshSwapChain(a_graphics);

        break;
    }
    default:
    {
        assert(result == VK_SUCCESS);

        break;
    }
    }

    if (m_resize)
    {
        m_resize = false;

        RefreshSwapChain(a_graphics);
    }
}

void Pipeline::RefreshSwapChain(Graphics* a_graphics)
{
    delete m_swapChain;

    for (auto iter = m_ubos.begin(); iter != m_ubos.end(); ++iter)
    {
        (*iter)->DestroyBuffer();
    }

    DestroyDescriptors();

    m_swapChain = new SwapChain(this);

    CreateDescriptors();

    for (auto iter = m_ubos.begin(); iter != m_ubos.end(); ++iter)
    {
        (*iter)->InitBuffer();
        (*iter)->InitDescriptor();
    }

    a_graphics->RefreshCommandBuffers();
}

void Pipeline::Resize()
{
    m_resize = true;
}

void Pipeline::AddUniformBufferObject(UniformBufferObject* a_bufferObject)
{
    m_ubos.emplace_back(a_bufferObject);
}
void Pipeline::RemoveUniformBufferObject(UniformBufferObject* a_bufferObject)
{
    m_ubos.remove(a_bufferObject);
}

EExportFunc(Pipeline*, Pipeline_new(const char* a_appName, Application* a_app));
EExportFunc(void, Pipeline_delete(Pipeline* a_pipeline));

EExportFunc(bool, Pipeline_PreUpdate(Pipeline* a_pipeline, Graphics* a_graphics));
EExportFunc(void, Pipeline_PostUpdate(Pipeline* a_pipeline, Graphics* a_graphics));

EExportFunc(void, Pipeline_Resize(Pipeline* a_pipeline));

Pipeline* Pipeline_new(const char* a_appName, Application* a_app) { return new Pipeline(a_appName, a_app); }
void Pipeline_delete(Pipeline* a_pipeline) { delete a_pipeline; }

bool Pipeline_PreUpdate(Pipeline* a_pipeline, Graphics* a_graphics) { a_pipeline->PreUpdate(a_graphics); }
void Pipeline_PostUpdate(Pipeline* a_pipeline, Graphics* a_graphics) { a_pipeline->PostUpdate(a_graphics); }

void Pipeline_Resize(Pipeline* a_pipeline) { a_pipeline->Resize(); };
