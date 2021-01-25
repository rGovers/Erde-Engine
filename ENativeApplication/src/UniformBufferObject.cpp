#include "UniformBufferObject.h"

#include <assert.h>
#include <string.h>

#include "Export.h"
#include "Pipeline.h"
#include "SwapChain.h"

void UniformBufferObject::InitBuffer()
{
    const VkDevice device = m_pipeline->GetDevice();

    const SwapChain* swapChain = m_pipeline->GetSwapChain();
    const unsigned int imageCount = swapChain->GetImageCount();

    m_uniformBuffers = new VkBuffer[imageCount];
    m_bufferMemory = new VkDeviceMemory[imageCount];

    for (int i = 0; i < imageCount; ++i)
    {
        VkBufferCreateInfo bufferInfo;
        memset(&bufferInfo, 0, sizeof(bufferInfo));

        bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
        bufferInfo.size = m_size;
        bufferInfo.usage = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
        bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

        assert(vkCreateBuffer(device, &bufferInfo, nullptr, &m_uniformBuffers[i]) == VK_SUCCESS);

        VkMemoryRequirements memReqirements;
        vkGetBufferMemoryRequirements(device, m_uniformBuffers[i], &memReqirements);

        VkMemoryAllocateInfo allocInfo;
        memset(&allocInfo, 0, sizeof(allocInfo));

        allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
        allocInfo.allocationSize = memReqirements.size;
        allocInfo.memoryTypeIndex = m_pipeline->FindMemoryType(memReqirements.memoryTypeBits, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);

        assert(vkAllocateMemory(device, &allocInfo, nullptr, &m_bufferMemory[i]) == VK_SUCCESS);
        vkBindBufferMemory(device, m_uniformBuffers[i], m_bufferMemory[i], 0);
    }
}
void UniformBufferObject::DestroyBuffer()
{
    const VkDevice device = m_pipeline->GetDevice();

    const SwapChain* swapChain = m_pipeline->GetSwapChain();
    const unsigned int imageCount = swapChain->GetImageCount();

    vkDeviceWaitIdle(device);

    for (int i = 0; i < imageCount; ++i)
    {
        vkDestroyBuffer(device, m_uniformBuffers[i], nullptr);
        vkFreeMemory(device, m_bufferMemory[i], nullptr);
    }

    delete[] m_uniformBuffers;
    delete[] m_bufferMemory;
}

void UniformBufferObject::InitDescriptor()
{
    const VkDevice device = m_pipeline->GetDevice();

    const SwapChain* swapChain = m_pipeline->GetSwapChain();
    const unsigned int imageCount = swapChain->GetImageCount();

    for (int i = 0; i < imageCount; ++i)
    {
        VkDescriptorBufferInfo bufferInfo;

        bufferInfo.buffer = m_uniformBuffers[i];
        bufferInfo.offset = 0;
        bufferInfo.range = m_size;

        VkWriteDescriptorSet descriptorWrite;
        memset(&descriptorWrite, 0, sizeof(descriptorWrite));

        descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
        descriptorWrite.dstSet = m_pipeline->GetDescriptorSet(i);
        descriptorWrite.dstBinding = m_binding;
        descriptorWrite.dstArrayElement = 0;
        descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
        descriptorWrite.descriptorCount = 1;
        descriptorWrite.pBufferInfo = &bufferInfo;

        vkUpdateDescriptorSets(device, 1, &descriptorWrite, 0, nullptr);
    }
}

UniformBufferObject::UniformBufferObject(unsigned int a_size, unsigned int a_binding, Pipeline* a_pipeline)
{
    m_pipeline = a_pipeline;

    m_size = a_size;
    m_binding = a_binding;
    
    InitBuffer();
    InitDescriptor();

    m_pipeline->AddUniformBufferObject(this);
}
UniformBufferObject::~UniformBufferObject()
{
    m_pipeline->RemoveUniformBufferObject(this);

    DestroyBuffer();
}

void UniformBufferObject::UpdateBuffer(void* a_data)
{
    const VkDevice device = m_pipeline->GetDevice();

    const unsigned int curImage = m_pipeline->GetImageIndex();

    void* data;
    vkMapMemory(device, m_bufferMemory[curImage], 0, m_size, 0, &data);
    memcpy(data, a_data, m_size);
    vkUnmapMemory(device, m_bufferMemory[curImage]);
}

EExportFunc(UniformBufferObject*, UniformBufferObject_new(unsigned int a_dataSize, unsigned int a_binding, Pipeline* a_pipeline));
EExportFunc(void, UniformBufferObject_delete(UniformBufferObject* a_ptr));

EExportFunc(void, UniformBufferObject_UpdateBuffer(UniformBufferObject* a_ptr, void* a_data));

UniformBufferObject* UniformBufferObject_new(unsigned int a_dataSize, unsigned int a_binding, Pipeline* a_pipeline) { return new UniformBufferObject(a_dataSize, a_binding, a_pipeline); }
void UniformBufferObject_delete(UniformBufferObject* a_ptr) { delete a_ptr; }

void UniformBufferObject_UpdateBuffer(UniformBufferObject* a_ptr, void* a_data) { a_ptr->UpdateBuffer(a_data); }