#!/bin/bash
# Blazor Radzen Playwright Sample - Build Script

echo "Building BlazorRadzenPlaywrightSample..."
dotnet build BlazorRadzenPlaywrightSample.sln

if [ $? -eq 0 ]; then
    echo "Build succeeded!"
else
    echo "Build failed!"
    exit 1
fi
