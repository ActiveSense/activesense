# ==================================
# CONFIGURATION
# ==================================

source("utils/config.R")

# ==================================
# MAIN LOGIC
# ==================================

# num_cores <- parallel::detectCores() - 1
# plan(multisession, workers = num_cores)

files <- getBinFiles()

# future_tasks <- list()

for (binfile in files) {
  
  my_pagerefs <- getPages(binfile)

  activity_analysis(
    binfile = binfile,
    summary_name = getSummaryName("Activity_Summary_Metrics_"),
    pagerefs = my_pagerefs
  )

  sleep_analysis(
    binfile = binfile,
    summary_name = getSummaryName("Sleep_Summary_Metrics_"),
    pagerefs = my_pagerefs
  )
  
  # if (analyze_activity) {
  #   future_tasks[[paste0(binfile, "_activity")]] <- future({
  #     activity_analysis(
  #       binfile = binfile,
  #       summary_name = getSummaryName("Activity_Summary_Metrics_")
  #     )
  #   })
  # }
  # 
  # if (analyze_sleep) {
  #   future_tasks[[paste0(binfile, "_sleep")]] <- future({
  #     sleep_analysis(
  #       binfile = binfile,
  #       summary_name = getSummaryName("Sleep_Summary_Metrics_")
  #     )
  #   })
  # }
  
}

# plan(sequential)

# ==================================
# END OF PROGRAM
# ==================================

cleanup_classification()

