# ==================================
# BASIC
# ==================================

# Sets up renv // automatic dependency management
#source("utils/renv_setup.R")

# clears persistent data objects
rm(list=ls())

# Sets repository
options(repos = c(
  ACTIVESENSE_UNIVERSE = "https://activesense.r-universe.dev",
  CRAN = "http://cran.r-project.org"
))

# time zone
Sys.setenv(TZ = "GMT")

# mmap configuration
mmap_setting_from_test <- getOption("myapp_use_mmap")

if (!is.null(mmap_setting_from_test)) {
  print(paste0("Using testing mmap.load via options: ", mmap_setting_from_test))
  mmap.load <- mmap_setting_from_test
} else {
  mmap.load <- (.Machine$sizeof.pointer >= 8)
  print(paste0("Using default mmap.load: ", mmap.load))
}

# Needs early source, since helper functions needed
source("functions/01_library_installer.R")

# ==================================
# PARAMETERS
# ==================================

# Create directories
dir.create(file.path(paste0(getwd(), "/data/")), showWarnings = FALSE)
dir.create(file.path(paste0(getwd(), "/libraries/")), showWarnings = FALSE)

# Add further entries to libraries
.libPaths(c("./libraries", .libPaths()))

message("--- Library paths:--- \n")
message(.libPaths())

if ("optparse" %in% rownames(installed.packages()) == FALSE) {
  message("==== Installing optparse====")
  install_package("optparse")
}

library(optparse)

# Define command line options
option_list <- list(
  make_option(c( "-d", "--directory"), type="character", default=paste0(getwd(), "/outputs/"),
              help="Base directory for output [default: current directory]"),
  make_option(c("-a", "--activity"), type="logical", default=TRUE,
              help="Run activity analysis [default: %default]"),
  make_option(c("-s", "--sleep"), type="logical", default=TRUE,
              help="Run sleep analysis [default: %default]"),
  make_option(c("-l", "--legacy"), type="logical", default=FALSE,
              help="Use old GENEA libraries [default: %default]"),
  make_option(c("-c", "--clipping"), type="logical", default=FALSE,
              help="Clip start and end of activity data [default: %default]"),
  
  # --- LEFT WRIST ---
  make_option("--sedentary_left", type="double", default=0.04,
              help="Pass sedentary threshold (left wrist) [default: %default]"),
  make_option("--light_left", type="double", default=217,
              help="Pass light threshold (left wrist) [default: %default]"),
  make_option("--moderate_left", type="double", default=644,
              help="Pass moderate threshold (left wrist) [default: %default]"),
  make_option("--vigorous_left", type="double", default=1810,
              help="Pass Vigorous threshold (left wrist) [default: %default]"),
  
  # --- RIGHT WRIST ---
  make_option("--sedentary_right", type="double", default=0.04,
              help="Pass sedentary threshold (right wrist) [default: %default]"),
  make_option("--light_right", type="double", default=386,
              help="Pass light threshold (right wrist) [default: %default]"),
  make_option("--moderate_right", type="double", default=439,
              help="Pass moderate threshold (right wrist) [default: %default]"),
  make_option("--vigorous_right", type="double", default=2098,
              help="Pass Vigorous threshold (right wrist) [default: %default]")
)

# Parse command line arguments
opt_parser <- OptionParser(option_list=option_list)
opt <- parse_args(opt_parser)

# Set variables based on arguments
output_dir <- opt$directory
analyze_activity <- opt$activity
analyze_sleep <- opt$sleep
use_legacy <- opt$legacy
use_clipping <- opt$clipping

# left wrist
sedentary_left <- opt$sedentary_left
light_left <- opt$light_left
moderate_left <- opt$moderate_left
vigorous_left <- opt$vigorous_left

# right wrist
sedentary_right <- opt$sedentary_right
light_right <- opt$light_right
moderate_right <- opt$moderate_right
vigorous_right <- opt$vigorous_right

# Create directories
dir.create(file.path(output_dir), showWarnings = FALSE)

# ==================================
# LIBRARIES
# ==================================

libraries <- c(
  "GENEAread",
  "GENEAclassify"
)

# If use_legacy == TRUE, only use CRAN
if (use_legacy) { options(repos = c(CRAN = "http://cran.r-project.org")) }

install_libraries(libraries)

library(GENEAread)
library(GENEAclassify)

# ==================================
# SOURCE
# ==================================

# functions
source("analysis/activity_analysis.R")
source("analysis/sleep_analysis.R")
source("functions/01_library_installer.R")
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

# utils
source("utils/timer.R")
source("utils/cleanup.R")

# ==================================
# DATACOLS
# ==================================

activity_datacols = c(
  "UpDown.mean", "UpDown.var", "UpDown.mad",
  "Degrees.mean", "Degrees.var", "Degrees.mad",
  "Magnitude.mean",
  "Light.mean",
  "Temp.mean", "Temp.sumdiff", "Temp.abssumdiff",
  "Step.GENEAcount", "Step.sd", "Step.mean", "Step.median"
)

sleep_datacols = c(
  "UpDown.mean", "UpDown.var", "UpDown.sd",
  "Degrees.mean", "Degrees.var", "Degrees.sd",
  "Magnitude.mean", "Magnitude.var", "Magnitude.meandiff", "Magnitude.mad",
  "Light.mean", "Light.max",
  "Temp.mean", "Temp.sumdiff", "Temp.meandiff", "Temp.abssumdiff",
  "Temp.sddiff", "Temp.var", "Temp.GENEAskew", "Temp.mad",
  "Principal.Frequency.mean", "Principal.Frequency.median"
)

# ==================================
# HELPER FUNCTIONS
# ==================================

getBinFiles <- function() {
  BinPattern <- "*\\.[bB][iI][nN]$"
  files <- list.files(path = paste0(getwd(), "/data"), pattern = BinPattern, full.names = TRUE)
  return(files)
}

getSummaryName <- function(name) {
  summary_name = paste0(name, strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]])
  return(summary_name)
}

getPages <- function(binfile) {
  initial_run_data <- read.bin(binfile, mmap.load = TRUE, pagerefs = TRUE, virtual = TRUE)
  return(initial_run_data$pagerefs)
}