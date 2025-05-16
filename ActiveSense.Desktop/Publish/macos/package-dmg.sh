#!/bin/bash
set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="Publish/macos"
APP_NAME="ActiveSense"
DMG_NAME="${APP_NAME}-Installer.dmg"
APP_BUNDLE="${APP_NAME}.app"

mkdir -p "$OUTPUT_DIR"

echo "Building ActiveSense for macOS..."

dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o "$OUTPUT_DIR/temp"

echo "Creating app bundle structure..."
mkdir -p "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS"
mkdir -p "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources"

# Copy the published app into the bundle
cp -r "$OUTPUT_DIR/temp/"* "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/"

if [ -f "Assets/active-sense-logo.png" ]; then
  cp "Assets/active-sense-logo.png" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/app-icon.png"
  echo "Using active-sense-logo.png for app icon"
else
  echo "Warning: No app icon found. The app will use the default icon."
fi

# Create Info.plist
cat > "$OUTPUT_DIR/$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleIconFile</key>
  <string>app-icon</string>
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

chmod +x "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/ActiveSense.Desktop"

# Create a temporary directory for DMG contents
mkdir -p "$OUTPUT_DIR/dmg_temp"
cp -r "$OUTPUT_DIR/$APP_BUNDLE" "$OUTPUT_DIR/dmg_temp/"

# allows users to drag and drop the app to their Applications folder
ln -s /Applications "$OUTPUT_DIR/dmg_temp/Applications"

echo "Creating DMG file..."
hdiutil create -volname "$APP_NAME" -srcfolder "$OUTPUT_DIR/dmg_temp" -ov -format UDZO "$OUTPUT_DIR/$DMG_NAME"

echo "Cleaning up..."
rm -rf "$OUTPUT_DIR/temp"
rm -rf "$OUTPUT_DIR/dmg_temp"

echo "Done! Installer package created at $OUTPUT_DIR/$DMG_NAME"
echo "You can distribute the DMG file to your users for easy installation."