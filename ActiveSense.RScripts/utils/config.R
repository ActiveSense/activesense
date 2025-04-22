# ==================================
# BASIC
# ==================================

# clears persistent data objects
rm(list=ls())

# Sets repository
local({
  r <- getOption("repos")
  r["CRAN"] <- "http://cran.r-project.org"
  options(repos = r)
})

# time zone
Sys.setenv(TZ = "GMT")

# execution control parameters
timer <- FALSE
rerun <- FALSE
mmap.load = TRUE

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
# LIBRARIES
# ==================================

librarys <- c(
  "GENEAread",
  "GENEAclassify",
  "scales",
  "reshape2",
  "future",
  "promises",
  "versions",
  "optparse",
  "testthat"
)

library_installer(librarys)

library(GENEAread)
library(GENEAclassify)
library(scales)
library(reshape2)
library(future)
library(promises)
library(optparse)
library(testthat)

# ==================================
# PARAMETERS
# ==================================

# Define command line options
option_list <- list(
  make_option(c("-d", "--directory"), type="character", default=paste0(getwd(), "/outputs/"),
              help="Base directory for output [default: current directory]"),
  make_option(c("-a", "--activity"), type="logical", default=TRUE,
              help="Run activity analysis [default: %default]"),
  make_option(c("-s", "--sleep"), type="logical", default=TRUE,
              help="Run sleep analysis [default: %default]")
)

# Parse command line arguments
opt_parser <- OptionParser(option_list=option_list)
opt <- parse_args(opt_parser)

# Set variables based on arguments
output_dir <- opt$directory
analyze_activity <- opt$activity
analyze_sleep <- opt$sleep

# Create directories
dir.create(file.path(paste0(getwd(), "/data/")), showWarnings = FALSE)
dir.create(file.path(output_dir), showWarnings = FALSE)

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
  "Step.GENEAcount", "Step.sd", "Step.mean", 
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


