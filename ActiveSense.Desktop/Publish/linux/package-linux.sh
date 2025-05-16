#!/bin/bash
# ActiveSense Desktop Linux Package Builder
# Script to create the Linux installer package

set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="Publish/linux"
PACKAGE_DIR="$OUTPUT_DIR/package"
INSTALLER_DIR="$OUTPUT_DIR/installer"

# Create output directories
mkdir -p "$OUTPUT_DIR"
mkdir -p "$PACKAGE_DIR"
mkdir -p "$INSTALLER_DIR"

echo "Building ActiveSense for Linux..."
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o "$PACKAGE_DIR"

echo "Copying installer files..."
# Copy the icon to the package directory
if [ -f "Assets/active-sense-logo.png" ]; then
  cp "Assets/active-sense-logo.png" "$PACKAGE_DIR/"
  echo "Custom icon copied to package"
fi

# Copy the installer script
cp "Publish/linux/install.sh" "$INSTALLER_DIR/"
chmod +x "$INSTALLER_DIR/install.sh"

echo "Creating archive..."
cd "$PACKAGE_DIR"
tar -czf "../ActiveSense-linux.tar.gz" .
cd ../../..

# Move the archive to the installer directory
mv "$OUTPUT_DIR/ActiveSense-linux.tar.gz" "$INSTALLER_DIR/"

echo "Creating final installer package..."
cd "$INSTALLER_DIR"
tar -czf "../ActiveSense-Linux-Installer.tar.gz" .
cd ../../..

echo "Linux installer package created at: $OUTPUT_DIR/ActiveSense-Linux-Installer.tar.gz"
echo "To install, extract the archive and run 'sudo bash install.sh'"