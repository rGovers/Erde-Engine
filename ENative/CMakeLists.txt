cmake_minimum_required (VERSION 2.6)
project (ENative)

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${PROJECT_SOURCE_DIR}/../bin/>)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${PROJECT_SOURCE_DIR}/../bin/>)

list(APPEND CMAKE_MODULE_PATH "${PROJECT_SOURCE_DIR}/modules")

find_package(Mono REQUIRED) 

include_directories(
	"${PROJECT_SOURCE_DIR}/include/"
	${MONO_INCLUDE_PATH}
)

file(GLOB SOURCES "src/*.cpp")

add_executable(ENative ${SOURCES})
target_link_libraries(ENative ${MONO_LIBRARIES})