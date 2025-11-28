#!/bin/bash
# build.sh - Build script for Linux/macOS

echo "Edit Sharp Build Script"
echo "======================="
echo ""

# Detect OS
OS="linux"
if [[ "$OSTYPE" == "darwin"* ]]; then
    OS="osx"
fi

# Build Trimmed AOT version
echo "Building Trimmed AOT Edition..."
dotnet publish edit-sharp/edit-sharp.csproj \
    --configuration Release-Trimmed \
    --runtime ${OS}-x64 \
    --self-contained true \
    --output ./build/trimmed-aot-${OS}-x64 \
    /p:PublishSingleFile=true \
    /p:PublishReadyToRun=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:IncludeAllContentForSelfExtract=true

if [ $? -eq 0 ]; then
    echo "✅ Trimmed AOT build complete: ./build/trimmed-aot-${OS}-x64/"
else
    echo "❌ Trimmed AOT build failed"
    exit 1
fi

echo ""

# Build Full version
echo "Building Full Edition..."
dotnet publish edit-sharp/edit-sharp.csproj \
    --configuration Release-Full \
    --runtime ${OS}-x64 \
    --self-contained true \
    --output ./build/full-${OS}-x64 \
    /p:PublishSingleFile=true \
    /p:PublishReadyToRun=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:IncludeAllContentForSelfExtract=true

if [ $? -eq 0 ]; then
    echo "✅ Full build complete: ./build/full-${OS}-x64/"
else
    echo "❌ Full build failed"
    exit 1
fi

echo ""
echo "Build Summary"
echo "============="
echo "Trimmed AOT: ./build/trimmed-aot-${OS}-x64/edit-sharp"
echo "Full:        ./build/full-${OS}-x64/edit-sharp"
echo ""

# Show file sizes
echo "File Sizes:"
if [ -f "./build/trimmed-aot-${OS}-x64/edit-sharp" ]; then
    TRIMMED_SIZE=$(ls -lh "./build/trimmed-aot-${OS}-x64/edit-sharp" | awk '{print $5}')
    echo "  Trimmed AOT: ${TRIMMED_SIZE}"
fi

if [ -f "./build/full-${OS}-x64/edit-sharp" ]; then
    FULL_SIZE=$(ls -lh "./build/full-${OS}-x64/edit-sharp" | awk '{print $5}')
    echo "  Full:        ${FULL_SIZE}"
fi

echo ""
echo "To run:"
echo "  Trimmed: ./build/trimmed-aot-${OS}-x64/edit-sharp"
echo "  Full:    ./build/full-${OS}-x64/edit-sharp"