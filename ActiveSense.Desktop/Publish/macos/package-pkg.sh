#!/bin/bash
set -e

# Configuration
PROJECT_PATH="ActiveSense.Desktop.csproj"
OUTPUT_DIR="publish/macos"
APP_NAME="ActiveSense"
PKG_NAME="${APP_NAME}-Installer.pkg"
APP_BUNDLE="${APP_NAME}.app"
IDENTIFIER="ch.ost.activesense"
VERSION="1.0.0"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "Building ActiveSense for macOS..."
# Publish the app with explicit .NET 8.0 target
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

# Look for logo files - prioritize the new active-sense-logo files
if [ -f "Assets/active-sense-logo.icns" ]; then
  # Use the .icns file directly if it exists
  cp "Assets/active-sense-logo.icns" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/app-icon.icns"
  echo "Using active-sense-logo.icns for app icon"
elif [ -f "Assets/active-sense-logo.png" ]; then
  # Use PNG directly (no conversion to different sizes)
  cp "Assets/active-sense-logo.png" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/app-icon.png"
  echo "Using active-sense-logo.png for app icon"
elif [ -f "Assets/app-icon.icns" ]; then
  # Fall back to the original icon name mentioned in your script
  cp "Assets/app-icon.icns" "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources/app-icon.icns"
  echo "Using app-icon.icns for app icon"
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
	<string>${IDENTIFIER}</string>
	<key>CFBundleName</key>
	<string>${APP_NAME}</string>
	<key>CFBundleDisplayName</key>
	<string>${APP_NAME}</string>
	<key>CFBundleExecutable</key>
	<string>ActiveSense.Desktop</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
	<key>CFBundlePackageType</key>
	<string>APPL</string>
	<key>CFBundleShortVersionString</key>
	<string>${VERSION}</string>
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

# Create scripts directory for package installation scripts
mkdir -p "$OUTPUT_DIR/scripts"

# Create preinstall script - check for requirements
cat > "$OUTPUT_DIR/scripts/preinstall" << 'EOF'
#!/bin/bash

# Check if .NET runtime is installed (although we're using self-contained)
echo "Checking system requirements..."

# Check for R if your app requires it
if ! command -v R &> /dev/null; then
    echo "NOTE: R is not installed. ActiveSense may require R for some functionality."
    echo "Please install R from https://cran.r-project.org/bin/macosx/ if needed."
fi

exit 0
EOF
chmod +x "$OUTPUT_DIR/scripts/preinstall"

# Create postinstall script
cat > "$OUTPUT_DIR/scripts/postinstall" << EOF
#!/bin/bash

# Set permissions for the application
chmod -R a+rX "/Applications/${APP_BUNDLE}"

echo "ActiveSense has been installed successfully."
exit 0
EOF
chmod +x "$OUTPUT_DIR/scripts/postinstall"

# Create distribution.xml for productbuild
cat > "$OUTPUT_DIR/distribution.xml" << EOF
<?xml version="1.0" encoding="utf-8"?>
<installer-gui-script minSpecVersion="1">
    <title>${APP_NAME} ${VERSION}</title>
    <organization>ch.ost</organization>
    <domains enable_localSystem="true"/>
    <options customize="never" require-scripts="true" rootVolumeOnly="true" />
    <welcome file="welcome.txt" mime-type="text/plain" />
    <conclusion file="conclusion.txt" mime-type="text/plain" />
    <volume-check>
        <allowed-os-versions>
            <os-version min="10.15" />
        </allowed-os-versions>
    </volume-check>
    <choices-outline>
        <line choice="default">
            <line choice="${IDENTIFIER}.app"/>
        </line>
    </choices-outline>
    <choice id="default"/>
    <choice id="${IDENTIFIER}.app" visible="false">
        <pkg-ref id="${IDENTIFIER}.app"/>
    </choice>
    <pkg-ref id="${IDENTIFIER}.app" version="${VERSION}" onConclusion="none">activesense.pkg</pkg-ref>
</installer-gui-script>
EOF

# Create welcome and conclusion text files
echo "Welcome to the ${APP_NAME} Installer

This will install ${APP_NAME} version ${VERSION} on your Mac.

${APP_NAME} is a sensor data analysis application developed by the Ostschweizer Fachhochschule.
" > "$OUTPUT_DIR/welcome.txt"

echo "Installation Complete

${APP_NAME} has been successfully installed in your Applications folder.

To start using ${APP_NAME}, open your Applications folder and double-click the ${APP_NAME} icon.
" > "$OUTPUT_DIR/conclusion.txt"

# Create component package (activesense.pkg)
echo "Creating component package..."
pkgbuild \
  --install-location "/Applications" \
  --scripts "$OUTPUT_DIR/scripts" \
  --identifier "${IDENTIFIER}.app" \
  --version "${VERSION}" \
  --component "$OUTPUT_DIR/$APP_BUNDLE" \
  "$OUTPUT_DIR/activesense.pkg"

# Create final distribution package
echo "Creating final installer package..."
productbuild \
  --distribution "$OUTPUT_DIR/distribution.xml" \
  --resources "$OUTPUT_DIR" \
  --package-path "$OUTPUT_DIR" \
  "$OUTPUT_DIR/$PKG_NAME"

# Clean up
echo "Cleaning up..."
rm -rf "$OUTPUT_DIR/temp"
rm -rf "$OUTPUT_DIR/scripts"
rm "$OUTPUT_DIR/activesense.pkg"
rm "$OUTPUT_DIR/distribution.xml"
rm "$OUTPUT_DIR/welcome.txt"
rm "$OUTPUT_DIR/conclusion.txt"

echo "Done! PKG installer created at $OUTPUT_DIR/$PKG_NAME"
echo "You can distribute the PKG file to your users for installation."