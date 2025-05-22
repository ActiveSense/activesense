#!/bin/bash
# ActiveSense Desktop Linux Package Builder
# Script to create the Linux installer package

set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="Publish/linux"
PACKAGE_DIR="$OUTPUT_DIR/package"
INSTALLER_DIR="$OUTPUT_DIR/installer"
R_PROJECT_DIR_SOURCE="../ActiveSense.RScripts"

mkdir -p "$OUTPUT_DIR"
mkdir -p "$PACKAGE_DIR"
mkdir -p "$INSTALLER_DIR"

# Installing dependencies for R
if [ -d "$R_PROJECT_DIR_SOURCE" ]; then
  echo "Setting up R environment in source directory: $R_PROJECT_DIR_SOURCE..."
  (cd "$R_PROJECT_DIR_SOURCE" && Rscript ./utils/renv_setup.R)
  echo "R environment setup in source complete."
else
  echo "Warning: R project source directory '$R_PROJECT_DIR_SOURCE' not found. dotnet publish will copy it as-is."
fi

echo "Building ActiveSense for Linux..."
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o "$PACKAGE_DIR"

echo "Copying installer files..."
if [ -f "Assets/active-sense-logo.png" ]; then
  cp "Assets/active-sense-logo.png" "$PACKAGE_DIR/"
  echo "icon copied to package"
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