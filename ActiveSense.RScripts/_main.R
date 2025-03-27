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

plan(multisession, workers = 2)

# Retrieving all .bin files in /data/ directory
files <- getBinFiles()

# Process each file
for (binfile in files) {

  #Run analysis in parallel
  future_activity <- future({
    activity_analysis(
      binfile = binfile,
      summary_name = getSummaryName("Activity_Summary_Metrics_")
    )
  })
  
  future_sleep <- future({
    sleep_analysis(
      binfile = binfile,
      summary_name = getSummaryName("Sleep_Summary_Metrics_")
    )
  })

  timer_activity <- value(future_activity)
  timer_sleep <- value(future_sleep)
}

plan(sequential)

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