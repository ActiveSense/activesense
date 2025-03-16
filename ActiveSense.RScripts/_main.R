# Set the repository
local({
  r <- getOption("repos")
  r["CRAN"] <- "http://cran.r-project.org"
  options(repos = r)
})

# Set the timezone 
Sys.setenv(TZ = "GMT")

# --- VARIABLES ---
timer <- FALSE
rerun <- FALSE # rerun the analysis

if (timer) {
  times = c()
  # Starting the timer
  cat("
      #.The time at the start of the analysis")
  print(Sys.time())
}

# --- INSTALLING LIBRARIES ---

source("utils/01_library_installer.R")

librarys <- c(
  "knitr",
  "ggplot2",
  "scales",
  "reshape2",
  "versions", 
  "GENEAread",
  "GENEAclassify"
)

library_installer(librarys)

library(knitr)
library(ggplot2)
library(scales)
library(reshape2)
library(versions)
library(GENEAread)
library(GENEAclassify)

# --- ALL FUNCTIONS FROM SOURCE ---

source("analysis/activity_analysis.R")
source("analysis/sleep_analysis.R")
source("utils/02_activity_combine_segment_data.R")
source("utils/02_sleep_combine_segment_data.R")
source("utils/03_naming_protocol.R")
source("utils/04_activity_create_df_pcp.R")
source("utils/04_sleep_create_df_pcp.R")
source("utils/05_number_of_days.R")
source("utils/06_bed_rise_detect.R")
source("utils/07_activity_state_rearrange.R")
source("utils/07_sleep_state_rearrange.R")
source("utils/08_activity_summary.R")
source("utils/08_sleep_summary.R")

if (timer) {
  # Starting the timer
  cat("
      #. Loading functions")
  print(Sys.time())
}


# --- CHECK IF FOLDER EXIST ---

dir.create(file.path(paste0(getwd(), "/data/")), showWarnings = FALSE) # Calibration values
dir.create(file.path(paste0(getwd(), "/outputs/")), showWarnings = FALSE) # Output reports
dir.create(file.path(paste0(getwd(), "/GENEAclassification/")), showWarnings = FALSE) # CSV values for decisions

# --- FINDING ALL BINARY FILES IN DIRECTORY ---

## Grab all bin files
# Taking all the bin files from the data folder.
# Finding the bin files inside the folder
BinPattern <- "*\\.[bB][iI][nN]$"
files <- list.files(path = paste0(getwd(), "/data"), pattern = BinPattern, full.names = TRUE)
# This will be changed to a parameter which can be feed as an output.

if (timer) {
  # Starting the timer
  cat("
      #. Finding files")
  print(Sys.time())
}

# --- CALLING SUBSCRIPTS ---
path <- getwd()

seq <- c(1:length(files))

# seq = seq[c(-3)] # Which numbers are corrupt from the files listed. Use this line to skip a file. Make sure to update on line 216 as well.

for (i in seq) {
  if (timer) {
    # Starting the timer
    cat("
        #.  File ", i, " starting at ", Sys.time())
  }

  binfile <- files[i]

  try({
    # Check that the classified CSV exsists, the Report and the Summary Metrics
    if (
      !file.exists(paste0(
        path, "/outputs/Activity_Report_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]],
        ".docx"
      ))
  
      | !file.exists(paste0(
          path, "/outputs/Activity_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]],
          ".csv"
        ))
  
      | !file.exists(file.path(path, "/outputs/", paste0(
          strsplit(
            unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))],
            ".bin"
          ),
          "_All_Data.rds"
        )))
  
      | rerun
    ) {
      
      # activity_analysis(
      #     binfile = binfile,
      #     summary_name = paste0("Activity_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]]),
      #     timer = timer
      # )

      sleep_analysis(
          binfile = binfile,
          summary_name = paste0("Sleep_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]]),
          timer = timer
      )
            
    } else {
      next
    }
  })
  
}

