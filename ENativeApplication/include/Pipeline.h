#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

#include <list>

class Application;
class Graphics;
class SwapChain;
class UniformBufferObject;

struct QueueFamilyIndices
{
    unsigned int GraphicsFamily;
    unsigned int PresentFamily;

    QueueFamilyIndices();

    inline bool IsSuitable() const
    {
        return GraphicsFamily != -1 && PresentFamily != -1;
    }
};

struct SwapChainSupportDetails
{
    VkSurfaceCapabilitiesKHR Capabilities;
    
    unsigned int FormatCount;
    VkSurfaceFormatKHR* Formats;

    unsigned int PresentModeCount;
    VkPresentModeKHR* PresentModes;

    SwapChainSupportDetails();

    inline bool IsSuitable() const
    {
        return FormatCount > 0 && PresentModeCount > 0;
    }
};

class Pipeline
{
private:
    Application*                    m_application;
        
    VkInstance                      m_instance;
        
    bool                            m_enableValidationLayers;
    VkDebugUtilsMessengerEXT        m_debugMessenger;
        
    VkPhysicalDevice                m_physicalDevice;
    VkDevice                        m_device;
        
    VkQueue                         m_graphicsQueue;
    VkQueue                         m_presentQueue;
        
    VkCommandPool                   m_commandPool;
        
    VkSurfaceKHR                    m_surface;
        
    VkSemaphore*                    m_imageSemaphore;
    VkSemaphore*                    m_renderSemaphore;
    VkFence*                        m_flightFences;
    VkFence*                        m_flightImages;
        
    SwapChain*                      m_swapChain;

    VkDescriptorSetLayout           m_descriptorLayout;
    VkDescriptorPool                m_descriptorPool;
    VkDescriptorSet*                m_descriptorSets;
        
    unsigned int                    m_imageIndex;
        
    unsigned int                    m_currFrame;
        
    bool                            m_resize;

    std::list<UniformBufferObject*> m_ubos;

    void InitInstance(const char* a_appName);
    void InitSurface();
    void InitDevice();
    void InitLogicalDevice();
    void InitSyncObjects();
    void InitCommandPool();

    void CreateDescriptors();
    void DestroyDescriptors();

protected:

public:
    Pipeline(const char* a_appName, Application* a_application);
    ~Pipeline();

    QueueFamilyIndices FindQueueFamily(const VkPhysicalDevice& a_device) const;
    SwapChainSupportDetails QuerySwapChainSupport(const VkPhysicalDevice& a_device, const VkSurfaceKHR& a_surface) const;
    unsigned int FindMemoryType(unsigned int a_typeFilter, VkMemoryPropertyFlags a_properties) const;

    bool PreUpdate(Graphics* a_graphics);
    void PostUpdate(Graphics* a_graphics);

    void RefreshSwapChain(Graphics* a_graphics);

    void Resize();

    void AddUniformBufferObject(UniformBufferObject* a_bufferObject);
    void RemoveUniformBufferObject(UniformBufferObject* a_bufferObject);

    inline Application* GetApplication() const
    {
        return m_application;
    }

    inline VkDevice GetDevice() const
    {
        return m_device;
    }
    inline VkPhysicalDevice GetPhysicalDevice() const
    {
        return m_physicalDevice;
    }

    inline SwapChain* GetSwapChain() const
    {
        return m_swapChain;
    }
    inline VkSurfaceKHR GetSurface() const
    {
        return m_surface;
    }

    inline VkCommandPool GetCommandPool() const
    {
        return m_commandPool;
    }

    inline unsigned int GetImageIndex() const
    {
        return m_imageIndex;
    }

    inline VkQueue GetGraphicsQueue() const
    {
        return m_graphicsQueue;
    }

    inline VkDescriptorSet GetDescriptorSet(unsigned int a_index) const
    {
        return m_descriptorSets[a_index];
    }

    inline VkDescriptorSetLayout GetDescriptorSetLayout()
    {
        return m_descriptorLayout;
    }
};