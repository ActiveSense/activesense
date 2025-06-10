# **ActiveSense: Activity and Sleep Analysis Tool**
## **Overview**
ActiveSense is a desktop application that analyzes and visualizes activity and sleep data from **GENEActiv** accelerometer sensors.
## **Key Features**
- **Interactive Charts** – Clear visualizations for clinical and personal use
- **Cross-Platform** – Available for Windows and macOS
## **System Requirements**
**ActiveSense has been tested on:**
- **Windows 11**
- **macOS Sonoma and Sequoia (Intel only)**
- **Linux (Pop!_OS)**

**R 4.x or higher** (required for data processing)

## **Download & Installation**
### **Windows**
1. Download `ActiveSense-Setup.exe` from the [Releases](../../releases) page
2. Run the installer and follow the setup wizard
3. Launch ActiveSense from the Start Menu or Desktop shortcut
### **macOS**
1. Download `ActiveSense-Installer.dmg` from the [Releases](../../releases) page
2. Open the DMG file and drag ActiveSense to your Applications folder
3. **Important**: Since we don't have an Apple Developer account, the app is not code-signed. You'll need to allow it manually:
  - Try to launch ActiveSense from Applications
  - When blocked, go to **System Preferences > Privacy & Security**
  - Find ActiveSense in the security section and click **"Open Anyway"**
  - Confirm by clicking **"Open"** in the dialog that appears
### **Linux**
1. Download `ActiveSense-Linux-Installer.tar.gz` from the [Releases](../../releases) page
2. Extract the files: `tar -xzf ActiveSense-Linux-Installer.tar.gz`
3. Run the installer: `sudo chmod +x install.sh` and then `sudo ./install.sh`
   
## **Getting Started**
### **1. Import Your Data**
- **GENEActiv Files**: Upload `.bin` files directly from your sensor
- **PDF Reports**: Load existing ActiveSense reports to view or re-export
### **2. Configure Analysis**
- Choose analysis type: Sleep, Activity, or Both
- Adjust sensor thresholds if needed (defaults work for most cases)
### **3. Process & View Results**
- Click "Start Analysis" to process your data
- View interactive charts and summary statistics
- Compare data across multiple time periods
### **4. Export Results**
- **PDF Report**: Summary with most important statistics
- **CSV + PDF**: Complete data export with raw measurements
  
## **Supported Data Formats**
- **Input**: GENEActiv `.bin` files, ActiveSense PDF reports
- **Output**: PDF reports, CSV data files
