cmake_minimum_required (VERSION 2.6)
project(ErdeNative)

option(BuildNative "Builds the Native to Managed Wrapper Application" ON)
option(BuildVulkan "Builds Vulkan Native Libraries" ON)

if (BuildVulkan)
    add_subdirectory("ENativeApplication")
endif(BuildVulkan)

if (BuildNative)
    add_subdirectory("ENative")
endif(BuildNative)

add_subdirectory("ENativePhysics")