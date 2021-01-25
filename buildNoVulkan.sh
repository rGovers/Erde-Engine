#!/bin/sh

if [ -e CMakeCache.txt ]; then
  rm CMakeCache.txt
fi

mkdir -p build
cd build
cmake -DCMAKE_BUILD_TYPE=Release -DBuildVulkan=OFF ..
make -j
msbuild /p:Configuration=Release .. 

echo ""
echo "-------------------------------------------------------------------"
echo "Build Complete"
echo "Output in bin"
echo "-------------------------------------------------------------------"