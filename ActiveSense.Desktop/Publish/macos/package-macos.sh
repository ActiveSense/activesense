#!/bin/bash
set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="publish/macos"
APP_NAME="ActiveSense"
DMG_NAME="${APP_NAME}-Installer.dmg"
APP_BUNDLE="${APP_NAME}.app"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "Building ActiveSense for macOS..."
# Publish the app
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o "$OUTPUT_DIR/temp"

# Create the app bundle structure
echo "Creating app bundle structure..."
mkdir -p "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS"
mkdir -p "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources"

# Copy the published app into the bundle
cp -r "$OUTPUT_DIR/temp/"* "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/"

# Handle icons - check for various possible icon files
if [ -f "Assets/active-sense-logo.icns" ]; then
  # Use the .icns file directly if it exists
  cp "Assets/active-sense-logo.icns" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/AppIcon.icns"
  ICON_NAME="AppIcon"
  echo "Using existing .icns file for the app icon"
elif [ -f "Assets/active-sense-logo.png" ]; then
  # Convert PNG to ICNS format if the PNG exists
  echo "Converting PNG logo to ICNS format..."
  
  # Create temporary iconset directory
  ICON_SET_DIR="$OUTPUT_DIR/AppIcon.iconset"
  mkdir -p "$ICON_SET_DIR"
  
  # Generate different icon sizes
  if command -v sips &> /dev/null; then
    sips -z 16 16 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_16x16.png"
    sips -z 32 32 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_16x16@2x.png"
    sips -z 32 32 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_32x32.png"
    sips -z 64 64 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_32x32@2x.png"
    sips -z 128 128 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_128x128.png"
    sips -z 256 256 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_128x128@2x.png"
    sips -z 256 256 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_256x256.png"
    sips -z 512 512 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_256x256@2x.png"
    sips -z 512 512 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_512x512.png"
    sips -z 1024 1024 "Assets/active-sense-logo.png" --out "$ICON_SET_DIR/icon_512x512@2x.png"
    
    # Convert the iconset to icns
    if command -v iconutil &> /dev/null; then
      iconutil -c icns "$ICON_SET_DIR" -o "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/AppIcon.icns"
      ICON_NAME="AppIcon"
      echo "Successfully converted PNG to ICNS format"
    else
      echo "Warning: iconutil not found, cannot convert iconset to icns"
      # Copy the PNG as a fallback
      cp "Assets/active-sense-logo.png" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/AppIcon.png"
      ICON_NAME="AppIcon"
    fi
    
    # Clean up the temporary iconset directory
    rm -rf "$ICON_SET_DIR"
  else
    echo "Warning: sips not found, cannot resize PNG for icon conversion"
    # Copy the PNG as a fallback
    cp "Assets/active-sense-logo.png" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/AppIcon.png"
    ICON_NAME="AppIcon"
  fi
elif [ -f "Assets/app-icon.icns" ]; then
  # Fallback to the original icon name mentioned in your script
  cp "Assets/app-icon.icns" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/app-icon.icns"
  ICON_NAME="app-icon"
  echo "Using app-icon.icns for the app icon"
else
  echo "Warning: No app icon found. The app will use the default icon."
  ICON_NAME=""
fi

# Create Info.plist
cat > "$OUTPUT_DIR/$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
EOF

# Add icon information if an icon was found
if [ ! -z "$ICON_NAME" ]; then
cat >> "$OUTPUT_DIR/$APP_BUNDLE/Contents/Info.plist" << EOF
	<key>CFBundleIconFile</key>
	<string>${ICON_NAME}</string>
EOF
fi

# Continue with the rest of Info.plist
cat >> "$OUTPUT_DIR/$APP_BUNDLE/Contents/Info.plist" << EOF
	<key>CFBundleIdentifier</key>
	<string>ch.ost.activesense</string>
	<key>CFBundleName</key>
	<string>ActiveSense</string>
	<key>CFBundleDisplayName</key>
	<string>ActiveSense</string>
	<key>CFBundleExecutable</key>
	<string>ActiveSense.Desktop</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
	<key>CFBundlePackageType</key>
	<string>APPL</string>
	<key>CFBundleShortVersionString</key>
	<string>1.0.0</string>
	<key>CFBundleVersion</key>
	<string>1</string>
	<key>LSMinimumSystemVersion</key>
	<string>10.15</string>
	<key>NSHighResolutionCapable</key>
	<true/>
	<key>NSHumanReadableCopyright</key>
	<string>© Ostschweizer Fachhochschule</string>
</dict>
</plist>
EOF

# Set executable permissions
chmod +x "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/ActiveSense.Desktop"

# Create a temporary directory for DMG contents
mkdir -p "$OUTPUT_DIR/dmg_temp"
cp -r "$OUTPUT_DIR/$APP_BUNDLE" "$OUTPUT_DIR/dmg_temp/"

# Create a Applications symlink in the temporary directory
# This allows users to drag and drop the app to their Applications folder
ln -s /Applications "$OUTPUT_DIR/dmg_temp/Applications"

# Create a README file with installation instructions
cat > "$OUTPUT_DIR/dmg_temp/README.txt" << EOF
ActiveSense Installation

To install ActiveSense:
1. Drag the ActiveSense app to the Applications folder
2. Double-click the app in your Applications folder to run it

For help, contact support@ost.ch
EOF

# Create the DMG
echo "Creating DMG file..."
hdiutil create -volname "$APP_NAME" -srcfolder "$OUTPUT_DIR/dmg_temp" -ov -format UDZO "$OUTPUT_DIR/$DMG_NAME"

# Clean up
echo "Cleaning up..."
rm -rf "$OUTPUT_DIR/temp"
rm -rf "$OUTPUT_DIR/dmg_temp"

echo "Done! Installer package created at $OUTPUT_DIR/$DMG_NAME"
echo "You can distribute the DMG file to your users for easy installation."