cmake_minimum_required (VERSION 2.6)
project (BtWrapNative)

FILE (STRINGS "VERSION" BULLET_VERSION)

include_directories(
	"${PROJECT_SOURCE_DIR}/Bullet"
	"${PROJECT_SOURCE_DIR}/include"
)

set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_BINARY_DIR}/lib>)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${CMAKE_BINARY_DIR}/lib>)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${PROJECT_SOURCE_DIR}/../bin/>)


OPTION(USE_DOUBLE_PRECISION "Use double precision"	OFF)
OPTION(USE_GRAPHICAL_BENCHMARK "Use Graphical Benchmark" OFF)
OPTION(BUILD_SHARED_LIBS "Use shared libraries" OFF)
OPTION(USE_SOFT_BODY_MULTI_BODY_DYNAMICS_WORLD "Use btSoftMultiBodyDynamicsWorld" ON)

OPTION(BULLET2_MULTITHREADING "Build Bullet 2 libraries with mutex locking around certain operations (required for multi-threading)" OFF)
IF (BULLET2_MULTITHREADING)
    OPTION(BULLET2_USE_OPEN_MP_MULTITHREADING "Build Bullet 2 with support for multi-threading with OpenMP (requires a compiler with OpenMP support)" OFF)
    OPTION(BULLET2_USE_TBB_MULTITHREADING "Build Bullet 2 with support for multi-threading with Intel Threading Building Blocks (requires the TBB library to be already installed)" OFF)
    IF (MSVC)
        OPTION(BULLET2_USE_PPL_MULTITHREADING "Build Bullet 2 with support for multi-threading with Microsoft Parallel Patterns Library (requires MSVC compiler)" OFF)
    ENDIF (MSVC)
ENDIF (BULLET2_MULTITHREADING)

IF(NOT WIN32)
	SET(DL ${CMAKE_DL_LIBS})
	IF(CMAKE_SYSTEM_NAME MATCHES "Linux")
		MESSAGE("Linux")
		SET(OSDEF -D_LINUX)
	ELSE(CMAKE_SYSTEM_NAME MATCHES "Linux")
		IF(APPLE)
			MESSAGE("Apple")
			SET(OSDEF -D_DARWIN)
		ELSE(APPLE)
			MESSAGE("BSD?")
			SET(OSDEF -D_BSD)
		ENDIF(APPLE)
	ENDIF(CMAKE_SYSTEM_NAME MATCHES "Linux")
ENDIF(NOT WIN32)

OPTION(USE_MSVC_INCREMENTAL_LINKING "Use MSVC Incremental Linking" OFF)
OPTION(USE_CUSTOM_VECTOR_MATH "Use custom vectormath library" OFF)

OPTION(BUILD_BULLET3 "Set when you want to build Bullet 3" ON)

file(GLOB SOURCES "src/*.cpp")

add_subdirectory("Bullet")

add_library(BtWrapNative SHARED ${SOURCES})
target_link_libraries(BtWrapNative BulletInverseDynamics BulletSoftBody BulletCollision BulletDynamics LinearMath Bullet3Common Bullet3Dynamics Bullet3Collision Bullet3Geometry)