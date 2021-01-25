#pragma once

#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

typedef void (*EventFunction)();

class Application
{
private:
    GLFWwindow* m_window;

    int         m_width;
    int         m_height;

    bool        m_shouldClose;
protected:

public:
    Application(const char* a_title, int a_stateHint);
    ~Application();

    inline int GetWidth() const
    {
        return m_width;
    }
    inline int GetHeight() const
    {
        return m_height;
    }

    void Resize(int a_width, int a_height);

    inline bool ShouldClose() const
    {
        return m_shouldClose;
    }

    inline GLFWwindow* GetHandle() const
    {
        return m_window;
    }

    void Update(EventFunction a_resize, EventFunction a_close);

    void Close();
}; 