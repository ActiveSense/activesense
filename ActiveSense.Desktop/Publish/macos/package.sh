#!/bin/bash
set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="publish/macos"
PACKAGE_NAME="ActiveSense-macos.tar.gz"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "Building ActiveSense for macOS..."
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o "$OUTPUT_DIR"

echo "Creating archive..."
cd publish
tar -czf "$PACKAGE_NAME" macos
cd ..

echo "Done! Package created at publish/$PACKAGE_NAME"