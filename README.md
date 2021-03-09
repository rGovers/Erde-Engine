# Erde Engine
Erde is a C# cross platform 3D game engine for Windows and Linux. It is developed to allow flexibility of modules and more exotic features from a Game Engine.  

## Prerequisites
* cmake
* Vulkan SDK

## Cloning
```bash
git clone https://github.com/rGovers/Erde-Engine.git
cd Erde-Engine
git submodule update --init --recursive
```

## Building
Warning this is heavy on first compile and will take a while to compile on low end systems. **Known to occasionally crash systems when compiling.**
### Windows
First generate the build files by executing the following commands.
```bash
mkdir build
cd build
cmake ..
```
Open the resulting sln in the build directory and compile all.

When that is finished go back to the main directory and open Erde.sln and build.

All resulting files will be in bin in the main directory when finished. 

### Linux
Run the desired build script and make sure execute as program is enabled.
Eg.
``` bash
./build.sh
```
All resulting files will be in the bin directory in the main directory when finished

## License
[MIT](https://choosealicense.com/licenses/mit/)
