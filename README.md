# **ActiveSense: Activity and Sleep Analysis Tool**

## **Project Overview**
**ActiveSense** is a desktop application designed to analyze and visualize activity and sleep data collected from **GENEActiv** accelerometer sensors. The application streamlines workflows for healthcare professionals, researchers, and patients by providing an intuitive interface for data processing, analysis, and visualization.

---

## **Key Features**
- ⚙️ **Efficient Data Processing**  
  Optimized R-based analysis engine significantly reduces processing time compared to the original workflow.

- 🔄 **Dual Analysis**  
  Simultaneous processing of both sleep and activity data.

- 📊 **Interactive Visualizations**  
  Clear and informative charts suitable for both clinical professionals and patients.

- 💻 **Cross-Platform Support**  
  Compatible with **Windows** and **macOS**.

- 🚀 **Streamlined Workflow**  
  Simplified process from data import to report generation.

- 📁 **Data Export**  
  Export analysis results in **PDF** and **CSV** formats for sharing and archiving.

---

## **Technical Architecture**

The application leverages a modern, efficient technology stack:

- **Frontend**: `C#/.NET` with **Avalonia UI** for cross-platform compatibility  
- **Backend Analysis**: Optimized `R scripts`  
- **Data Visualization**: `LiveChartsCore.SkiaSharpView`  
- **Data Serialization**: `JSON` for configuration and export  

---

## **Project Structure**

The solution is divided into two main components:

### **1. ActiveSense.Desktop**
The primary desktop application including:
- User interface components  
- Data processing services  
- Visualization models  
- Export functionality  

### **2. ActiveSense.RScripts**
The R-based analysis engine including:
- Data segmentation algorithms  
- Sleep analysis functions  
- Activity analysis functions  
- Utility functions  

---

## **Development Requirements**
- `.NET 9.0 SDK`  
- `R 4.x` (with required packages)  
- `Visual Studio` or `JetBrains Rider`  

---

## **Building the Project**

1. Clone the repository  
2. Open `ActiveSense.Desktop.sln` in Visual Studio or Rider  
3. Restore NuGet packages  
4. Build the solution  

---

## **Usage**

1. **Import Data**  
   Upload GENEActiv `.bin` files or previously analyzed `.csv` files

2. **Configure Analysis**  
   Select analysis types: *Sleep*, *Activity*, or *Both*

3. **Run Analysis**  
   Start processing using optimized algorithms

4. **View Results**  
   Explore interactive visualizations and summary statistics

5. **Export Data**  
   Generate **PDF** reports or **CSV** exports for further analysis

---

## **Key Optimizations**

ActiveSense includes several performance optimizations:

- **Parallelized data processing** across multiple CPU cores  
- **Efficient memory mapping** for handling large files  
- **Streamlined data segmentation** for faster analysis  
- **Combined sleep and activity analysis** to minimize redundancy  

---

## **Testing**

The project includes comprehensive testing:

- ✅ **Unit Tests** – Validate core functionality  
- 🔄 **Integration Tests** – Ensure reliable interaction between components  
- 🧪 **Regression Tests** – Maintain consistency with previous results  

