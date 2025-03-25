# ==================================
# CONFIGURATION
# ==================================

source("utils/config.R")

if (timer) {
  timer_total <- create_timer("Total Timer")
  timer_total <- append.timer(timer_total, "Start of the analysis") 
}

# ==================================
# MAIN LOGIC
# ==================================

# Retrieving all .bin files in /data/ directory

files <- getBinFiles()

# Calling subscripts

seq <- c(1:length(files))

for (i in seq) {

  binfile <- files[i]
  
  timer_activity <- activity_analysis(
      binfile = binfile,
      summary_name = getSummaryName("Activity_Summary_Metrics_")
  )

  # timer_sleep <- sleep_analysis(
  #   binfile = binfile,
  #   summary_name = getSummaryName("Sleep_Summary_Metrics_")
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