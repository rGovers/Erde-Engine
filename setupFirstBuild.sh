#!/bin/sh

git submodule update --init --recursive

nuget restore

cd ENativeApplication/lib/shaderc/
./utils/git-sync-deps