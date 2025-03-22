# ==================================
# CONFIGURATION
# ==================================

# TODO: This clears list, even when session are being reused.
rm(list=ls())

source("utils/timer.R")

# Set the repository
local({
  r <- getOption("repos")
  r["CRAN"] <- "http://cran.r-project.org"
  options(repos = r)
})

# Set the timezone 
Sys.setenv(TZ = "CET")

# Variables
timer <- TRUE
rerun <- FALSE # rerun the analysis

if (timer) {
  timer_total <- create_timer("Total Timer")
  timer_total <- append.timer(timer_total, "Start of the analysis") 
}

# Installing libraries

source("functions/01_library_installer.R")

# Do I really need all of these libraries?

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
source("functions/02_activity_combine_segment_data.R")
source("functions/02_sleep_combine_segment_data.R")
source("functions/03_naming_protocol.R")
source("functions/04_activity_create_df_pcp.R")
source("functions/04_sleep_create_df_pcp.R")
source("functions/05_number_of_days.R")
source("functions/06_bed_rise_detect.R")
source("functions/07_activity_state_rearrange.R")
source("functions/07_sleep_state_rearrange.R")
source("functions/08_activity_summary.R")
source("functions/08_sleep_summary.R")

# --- CHECK IF FOLDER EXIST ---

dir.create(file.path(paste0(getwd(), "/data/")), showWarnings = FALSE) # Calibration values
dir.create(file.path(paste0(getwd(), "/outputs/")), showWarnings = FALSE) # Output reports
dir.create(file.path(paste0(getwd(), "/GENEAclassification/")), showWarnings = FALSE) # CSV values for decisions

# ==================================
# MAIN LOGIC
# ==================================

# Retrieving all .bin files in /data/ directory

BinPattern <- "*\\.[bB][iI][nN]$"
files <- list.files(path = paste0(getwd(), "/data"), pattern = BinPattern, full.names = TRUE)

# Calling subscripts

# path <- getwd()

seq <- c(1:length(files))

for (i in seq) {

  binfile <- files[i]
  
  timer_activity <- activity_analysis(
      binfile = binfile,
      summary_name = paste0("Activity_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]]),
      timer = timer
  )

  # timer_sleep <- sleep_analysis(
  #   binfile = binfile,
  #   summary_name = paste0("Sleep_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]]),
  #   timer = timer
  # )

}

# ==================================
# END OF PROGRAM // TIMER END
# ==================================

if (timer) {
  timer_total <- append.timer(timer_total, "End of Analyis")
  
  analysis_list <- list(timer_total)
  if (exists("timer_activity")) analysis_list <- c(analysis_list, list(timer_activity))
  if (exists("timer_sleep")) analysis_list <- c(analysis_list, list(timer_sleep)) 
  
  timer_merge(analysis_list, binfile)
}