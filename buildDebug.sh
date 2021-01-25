#!/bin/sh

if [ -e CMakeCache.txt ]; then
  rm CMakeCache.txt
fi

mkdir -p build
cd build
cmake -DCMAKE_BUILD_TYPE=Debug ..
make -j
msbuild /p:Configuration=Debug .. 

echo ""
echo "-------------------------------------------------------------------"
echo "Build Complete"
echo "Output in bin"
echo "-------------------------------------------------------------------"