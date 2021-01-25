#include "Model.h"

#include <assert.h>
#include <string.h>

#include "Export.h"
#include "Pipeline.h"

void CreateBuffer(unsigned int a_size, VkBufferUsageFlags a_usage, VkMemoryPropertyFlags a_properties, VkBuffer* a_buffer, VkDeviceMemory* a_bufferMemory, const Pipeline* a_pipeline)
{
    const VkDevice device = a_pipeline->GetDevice();

    VkBufferCreateInfo bufferInfo;
    memset(&bufferInfo, 0, sizeof(bufferInfo));

    bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
    bufferInfo.size = a_size;
    bufferInfo.usage = a_usage;
    bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

    assert(vkCreateBuffer(device, &bufferInfo, nullptr, a_buffer) == VK_SUCCESS);

    VkMemoryRequirements memRequirements;
    vkGetBufferMemoryRequirements(device, *a_buffer, &memRequirements);

    VkMemoryAllocateInfo allocInfo;
    memset(&allocInfo, 0, sizeof(allocInfo));

    allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    allocInfo.allocationSize = memRequirements.size;
    allocInfo.memoryTypeIndex = a_pipeline->FindMemoryType(memRequirements.memoryTypeBits, a_properties);

    assert(vkAllocateMemory(device, &allocInfo, nullptr, a_bufferMemory) == VK_SUCCESS);
    vkBindBufferMemory(device, *a_buffer, *a_bufferMemory, 0);
}

void Model::InitBuffer(void* a_data, unsigned int a_dataSize, unsigned int a_offset)
{
    const VkDevice device = m_pipeline->GetDevice();

    VkBuffer stagingBuffer;
    VkDeviceMemory stagingBufferMemory;
    CreateBuffer(a_dataSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, &stagingBuffer, &stagingBufferMemory, m_pipeline);

    void* data;
    vkMapMemory(device, stagingBufferMemory, 0, a_dataSize, 0, &data);
    memcpy(data, a_data, a_dataSize);
    vkUnmapMemory(device, stagingBufferMemory);

    VkCommandBufferAllocateInfo allocInfo;
    memset(&allocInfo, 0, sizeof(allocInfo));

    allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    allocInfo.commandPool = m_pipeline->GetCommandPool();
    allocInfo.commandBufferCount = 1;

    VkCommandBuffer commandBuffer;
    vkAllocateCommandBuffers(device, &allocInfo, &commandBuffer);

    VkCommandBufferBeginInfo beginInfo;
    memset(&beginInfo, 0, sizeof(beginInfo));

    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;

    vkBeginCommandBuffer(commandBuffer, &beginInfo);
    {
        VkBufferCopy copyRegion;
        copyRegion.srcOffset = 0;
        copyRegion.dstOffset = a_offset;
        copyRegion.size = a_dataSize;

        vkCmdCopyBuffer(commandBuffer, stagingBuffer, m_buffer, 1, &copyRegion);
    }
    vkEndCommandBuffer(commandBuffer);

    VkSubmitInfo submitInfo;
    memset(&submitInfo, 0, sizeof(submitInfo));

    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &commandBuffer;

    const VkQueue graphicsQueue = m_pipeline->GetGraphicsQueue();

    vkQueueSubmit(graphicsQueue, 1, &submitInfo, VK_NULL_HANDLE);
    vkQueueWaitIdle(graphicsQueue);

    vkDestroyBuffer(device, stagingBuffer, nullptr);
    vkFreeMemory(device, stagingBufferMemory, nullptr);
}

Model::Model(void* a_vertexData, unsigned int a_vertexDataSize, unsigned short* a_indexData, unsigned int a_indexCount, Pipeline* a_pipeline)
{
    m_pipeline = a_pipeline;

    m_indexCount = a_indexCount;
    m_indexOffset = a_vertexDataSize;

    const unsigned int indexDataSize = m_indexCount * sizeof(unsigned short);
    const unsigned int bufferSize = m_indexOffset + indexDataSize;

    CreateBuffer(bufferSize, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT, &m_buffer, &m_bufferMemory, m_pipeline);
    
    InitBuffer(a_vertexData, m_indexOffset, 0);
    InitBuffer(a_indexData, indexDataSize, m_indexOffset);
}
Model::~Model()
{
    const VkDevice device = m_pipeline->GetDevice();

    vkDestroyBuffer(device, m_buffer, nullptr);
    vkFreeMemory(device, m_bufferMemory, nullptr);
}

EExportFunc(Model*, Model_new(void* a_vertexData, unsigned int a_vertexDataSize, unsigned short* a_indexData, unsigned int a_indexCount, Pipeline* a_pipeline));
EExportFunc(void, Model_delete(Model* a_ptr));

Model* Model_new(void* a_vertexData, unsigned int a_vertexDataSize, unsigned short* a_indexData, unsigned int a_indexCount, Pipeline* a_pipeline) { return new Model(a_vertexData, a_vertexDataSize, a_indexData, a_indexCount, a_pipeline); }
void Model_delete(Model* a_ptr) { delete a_ptr; }