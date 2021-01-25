cmake_minimum_required (VERSION 3.10.2)
project (ENativeApplication)

find_package(Vulkan REQUIRED)

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${PROJECT_SOURCE_DIR}/../bin/>)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${PROJECT_SOURCE_DIR}/../bin/>)

# set(CMAKE_SHARED_LIBRARY_PREFIX "")

set(SHADERC_SKIP_INSTALL ON)
set(SHADERC_SKIP_TESTS ON)

set(GLFW_BUILD_EXAMPLES OFF)
set(GLFW_BUILD_TESTS OFF)
set(GLFW_BUILD_DOCS OFF)
set(GLFW_INSTALL OFF)

add_subdirectory("${PROJECT_SOURCE_DIR}/lib/glfw")
add_subdirectory("${PROJECT_SOURCE_DIR}/lib/shaderc")

include_directories(
    "${PROJECT_SOURCE_DIR}/include"
    "${PROJECT_SOURCE_DIR}/lib/glfw/include"
    "${PROJECT_SOURCE_DIR}/lib/glm"
    "${PROJECT_SOURCE_DIR}/lib/shaderc/libshaderc/include"
    ${Vulkan_INCLUDE_DIRS}
)

file(GLOB SOURCES "src/*.cpp")

add_library(ENativeApplication SHARED ${SOURCES})
target_link_libraries(ENativeApplication glfw ${GLFW_LIBRARIES} ${Vulkan_LIBRARIES} shaderc)
