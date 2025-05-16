#!/bin/bash
# ActiveSense Desktop Linux Installer
# Simple installer script for ActiveSense Desktop

# Define colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Define application details
APP_NAME="ActiveSense"
INSTALL_DIR="/opt/ActiveSense"
DESKTOP_FILE="/usr/share/applications/activesense.desktop"
BIN_LINK="/usr/local/bin/activesense"

# Display banner
echo -e "${BLUE}================================================${NC}"
echo -e "${BLUE}  ActiveSense Desktop Linux Installer${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# Check if running with sudo/root
if [ "$EUID" -ne 0 ]; then
  echo -e "${RED}Please run this installer with sudo or as root${NC}"
  echo "Example: sudo bash $0"
  exit 1
fi

# Check if archive exists in the current directory
if [ ! -f "./ActiveSense-linux.tar.gz" ]; then
  echo -e "${RED}Error: ActiveSense-linux.tar.gz not found in the current directory${NC}"
  exit 1
fi

# Create installation directory
echo -e "${BLUE}Creating installation directory...${NC}"
mkdir -p "$INSTALL_DIR"

# Extract application
echo -e "${BLUE}Extracting application files...${NC}"
tar -xzf "./ActiveSense-linux.tar.gz" -C "$INSTALL_DIR"

# Make the main executable executable
echo -e "${BLUE}Setting permissions...${NC}"
chmod +x "$INSTALL_DIR/ActiveSense.Desktop"

# Create desktop entry
echo -e "${BLUE}Creating desktop entry...${NC}"
cat > "$DESKTOP_FILE" << EOF
[Desktop Entry]
Name=ActiveSense
Comment=Sensor Data Analysis Application
Exec=$INSTALL_DIR/ActiveSense.Desktop
Icon=$INSTALL_DIR/active-sense-logo.png
Terminal=false
Type=Application
Categories=Education;Science;
EOF

# Create symlink
echo -e "${BLUE}Creating command-line link...${NC}"
ln -sf "$INSTALL_DIR/ActiveSense.Desktop" "$BIN_LINK"

# Update desktop database
echo -e "${BLUE}Updating desktop database...${NC}"
update-desktop-database 2>/dev/null || true

echo ""
echo -e "${GREEN}==========================================${NC}"
echo -e "${GREEN}  ActiveSense Desktop installed successfully!${NC}"
echo -e "${GREEN}==========================================${NC}"
echo ""
echo "You can now launch ActiveSense Desktop from your application menu"
echo "or by typing 'activesense' in a terminal."
echo ""
echo "Installation directory: $INSTALL_DIR"
echo ""

exit 0