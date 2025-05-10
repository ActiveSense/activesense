#!/bin/bash
set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="publish/linux"
PACKAGE_NAME="ActiveSense-linux.tar.gz"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "Building ActiveSense for Linux..."
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o "$OUTPUT_DIR"

echo "Creating archive..."
cd publish
tar -czf "$PACKAGE_NAME" linux
cd ..

echo "Package created at publish/$PACKAGE_NAME"