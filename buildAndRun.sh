#!/bin/bash

if [ ! -f "TankGL_fbo.slnx" ] && [ ! -d "src" ]; then
    exit 1
fi

dotnet restore
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build -c Release
if [ $? -ne 0 ]; then
    exit 1
fi

if ! command -v wine &> /dev/null; then
    exit 1
fi

wine src/TankGL_fbo.WPF/bin/Release/net10.0-windows/win-x64/TankGL_fbo.WPF.exe