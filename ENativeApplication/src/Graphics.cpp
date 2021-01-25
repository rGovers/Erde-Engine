#include "Graphics.h"

#include <assert.h>
#include <string.h>

#include "Export.h"
#include "SwapChain.h"
#include "SwapTexture.h"

Graphics::Graphics(Pipeline* a_pipeline)
{
    m_pipeline = a_pipeline;

    m_bufferCount = m_pipeline->GetSwapChain()->GetRenderTexture()->GetImageCount();

    m_commandBuffers = new VkCommandBuffer[m_bufferCount];

    InitCommandBuffers();
}
Graphics::~Graphics()
{
    const VkDevice device = m_pipeline->GetDevice();

    vkDeviceWaitIdle(device);

    vkFreeCommandBuffers(device, m_pipeline->GetCommandPool(), m_bufferCount, m_commandBuffers);

    delete[] m_commandBuffers;
}

void Graphics::InitCommandBuffers()
{
    const VkDevice device = m_pipeline->GetDevice();
    const SwapTexture* swapTexture = m_pipeline->GetSwapChain()->GetRenderTexture();

    VkCommandBufferAllocateInfo allocInfo;
    memset(&allocInfo, 0, sizeof(allocInfo));

    allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocInfo.commandPool = m_pipeline->GetCommandPool();
    allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    allocInfo.commandBufferCount = m_bufferCount;

    assert(vkAllocateCommandBuffers(device, &allocInfo, m_commandBuffers) == VK_SUCCESS);

    for (int i = 0; i < m_bufferCount; ++i)
    {
        VkCommandBufferBeginInfo beginInfo;
        memset(&beginInfo, 0, sizeof(beginInfo));

        beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

        assert(vkBeginCommandBuffer(m_commandBuffers[i], &beginInfo) == VK_SUCCESS);

        VkRenderPassBeginInfo renderPassInfo;
        memset(&renderPassInfo, 0, sizeof(renderPassInfo));

        renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
        renderPassInfo.renderPass = swapTexture->GetRenderPass();
        renderPassInfo.framebuffer = swapTexture->GetFrameBuffer(i);
        renderPassInfo.renderArea.offset = { 0, 0 };
        renderPassInfo.renderArea.extent = { swapTexture->GetWidth(), swapTexture->GetHeight() };

        const VkClearValue clearColor = { 1.0f, 0.0f, 0.0f, 1.0f };
        renderPassInfo.clearValueCount = 1;
        renderPassInfo.pClearValues = &clearColor;

        vkCmdBeginRenderPass(m_commandBuffers[i], &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
        {
            
        }
        vkCmdEndRenderPass(m_commandBuffers[i]);

        assert(vkEndCommandBuffer(m_commandBuffers[i]) == VK_SUCCESS);
    }
}

void Graphics::Update(double a_delta)
{
    const SwapTexture* swapTexture = m_pipeline->GetSwapChain()->GetRenderTexture();

    const unsigned int imageIndex = m_pipeline->GetImageIndex();

    VkCommandBuffer commandBuffer = m_commandBuffers[imageIndex];

    vkResetCommandBuffer(commandBuffer, 0);

    VkCommandBufferBeginInfo beginInfo;
    memset(&beginInfo, 0, sizeof(beginInfo));

    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

    assert(vkBeginCommandBuffer(commandBuffer, &beginInfo) == VK_SUCCESS);

    VkRenderPassBeginInfo renderPassInfo;
    memset(&renderPassInfo, 0, sizeof(renderPassInfo));

    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    renderPassInfo.renderPass = swapTexture->GetRenderPass();
    renderPassInfo.framebuffer = swapTexture->GetFrameBuffer(imageIndex);
    renderPassInfo.renderArea.offset = { 0, 0 };
    renderPassInfo.renderArea.extent = { swapTexture->GetWidth(), swapTexture->GetHeight() };

    const VkClearValue clearColor = { 0.2f, 0.2f, 0.2f, 1.0f };
    renderPassInfo.clearValueCount = 1;
    renderPassInfo.pClearValues = &clearColor;

    vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    {
        
    }
    vkCmdEndRenderPass(commandBuffer);

    assert(vkEndCommandBuffer(commandBuffer) == VK_SUCCESS);
}

void Graphics::RefreshCommandBuffers()
{
    const VkDevice device = m_pipeline->GetDevice();

    vkDeviceWaitIdle(device);

    vkFreeCommandBuffers(device, m_pipeline->GetCommandPool(), m_bufferCount, m_commandBuffers);

    InitCommandBuffers();
}

EExportFunc(Graphics*, Graphics_new(Pipeline* a_pipeline));
EExportFunc(void, Graphics_delete(Graphics* a_graphics));

EExportFunc(void, Graphics_Update(Graphics* a_graphics, double a_delta));

Graphics* Graphics_new(Pipeline* a_pipeline) { return new Graphics(a_pipeline); }
void Graphics_delete(Graphics* a_graphics) { delete a_graphics; }

void Graphics_Update(Graphics* a_graphics, double a_delta) { a_graphics->Update(a_delta); }