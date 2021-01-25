#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

#include "RenderTexture.h"

class SwapChain;

class SwapTexture : public RenderTexture
{
private:
    
protected:

public:
    SwapTexture(unsigned int a_width, unsigned int a_height, const VkSurfaceFormatKHR& a_format, Pipeline* a_pipeline, SwapChain* a_swapChain);
    virtual ~SwapTexture();

};
