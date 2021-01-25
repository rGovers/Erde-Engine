#include "Application.h"

#include <assert.h>
#include <stdio.h>

#include "Export.h"

void ErrorCallback(int a_error, const char* a_description)
{
    printf("Error: %s\n", a_description);
}

Application::Application(const char* a_title, int a_stateHint)
{
    m_width = 1280;
    m_height = 720;

    m_shouldClose = false;

    assert(glfwInit());
    assert(glfwVulkanSupported());

    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);

    glfwSetErrorCallback(ErrorCallback);

    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 2);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 0);

    if (a_stateHint == 2)
    {
        m_window = glfwCreateWindow(m_width, m_height, a_title, glfwGetPrimaryMonitor(), NULL);
    }
    else
    {
        m_window = glfwCreateWindow(m_width, m_height, a_title, NULL, NULL);
    }

    assert(m_window);
}
Application::~Application()
{
    glfwTerminate();
}

void Application::Resize(int a_width, int a_height)
{
    glfwSetWindowSize(m_window, a_width, a_height);
}

void Application::Update(EventFunction a_resize, EventFunction a_close)
{
    glfwPollEvents();

    m_shouldClose = glfwWindowShouldClose(m_window);

    int oldWidth = m_width;
    int oldHeight = m_height;

    glfwGetWindowSize(m_window, &m_width, &m_height);

    if (a_resize != nullptr && (m_width != oldWidth || m_height != oldHeight))
    {
        a_resize();
    }

    if (a_close != nullptr && m_shouldClose)
    {
        a_close();
    }
}

void Application::Close()
{
    glfwDestroyWindow(m_window);
}

EExportFunc(Application*, ApplicationNew(const char* a_title, int a_stateHint));
EExportFunc(void, ApplicationDelete(Application* a_app));

EExportFunc(bool, ApplicationShouldClose(Application* a_app));

EExportFunc(int, ApplicationWidth(Application* a_app));
EExportFunc(int, ApplicationHeight(Application* a_app));

EExportFunc(void, ApplicationResize(Application* a_app, int a_width, int a_height));

EExportFunc(void, ApplicationUpdate(Application* a_app, EventFunction a_resize, EventFunction a_close));

EExportFunc(void, ApplicationClose(Application* a_app));

Application* ApplicationNew(const char* a_title, int a_stateHint) { return new Application(a_title, a_stateHint); }
void ApplicationDelete(Application* a_app) { delete a_app; }

bool ApplicationShouldClose(Application* a_app) { return a_app->ShouldClose(); }

int ApplicationWidth(Application* a_app) { return a_app->GetWidth(); }
int ApplicationHeight(Application* a_app) { return a_app->GetHeight(); }

void ApplicationResize(Application* a_app, int a_width, int a_height) { a_app->Resize(a_width, a_height); }

void ApplicationUpdate(Application* a_app, EventFunction a_resize, EventFunction a_close) { a_app->Update(a_resize, a_close); }

void ApplicationClose(Application* a_app) { a_app->Close(); }